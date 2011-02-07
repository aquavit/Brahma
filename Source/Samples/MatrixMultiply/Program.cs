﻿#region License and Copyright Notice
// Copyright (c) 2010 Ananth B.
// All rights reserved.
// 
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.
#endregion

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Brahma.OpenCL;
using Brahma.Types;
using OpenCL.Net;

namespace Brahma.Samples.MatrixMultiply
{
    class Program
    {
        private static readonly Random _random = new Random();
        
        private static float32[] MakeMatrix(int rows, int cols)
        {
            var result = new float32[rows * cols];
            for (int i = 0; i < rows * cols; i++)
                result[i] = (float32)_random.NextDouble();

            return result;
        }

        private static void GetOutputMatrixDimensions(int aRows, int aCols, int bRows, int bCols, out int cRows, out int cCols)
        {
            if (aCols != bRows)
                throw new InvalidOperationException("Cannot multiply these two matrices");

            cRows = aRows;
            cCols = bCols;
        }

        private static void Multiply(float32[] a, int aRows, int aCols, float32[] b, int bRows, int bCols, ref float32[] c)
        {
            int cRows;
            int cCols;
            GetOutputMatrixDimensions(aRows, aCols, bRows, bCols, out cRows, out cCols);

            for (int i = 0; i < cRows; i++)
                for (int j = 0; j < cCols; j++)
                {
                    float32 tmp = 0;
                    for (int k = 0; k < aCols; k++)
                        tmp += a[i * aCols + k] * b[k * bCols + j];

                    c[i * cCols + j] = tmp;
                }
        }

        static void Main(string[] args)
        {
            string platformName = "*";

            int rows = 100;
            int columns = 100;
            int localWorkSize = 10;
            int iterations = 100;
            Cl.DeviceType deviceType = Cl.DeviceType.Default;

            args.Process(() => Console.WriteLine("Usage is {0} platform=<platform name> device=<Cpu or Gpu or Default (default = Default)> rows=<rows (default = 100)> cols=<columns (default = 100)> localWorkSize=<local work size (default = 10)> iterations=<Number of iterations to run for each (default = 100)>",
                Assembly.GetEntryAssembly().CodeBase),
                new CommandLine.Switch("platform", v => platformName = v.First()),
                new CommandLine.Switch("device", v => deviceType = (Cl.DeviceType)Enum.Parse(typeof(Cl.DeviceType), v.First())),
                new CommandLine.Switch("rows", v => rows = int.Parse(v.First(), CultureInfo.CurrentCulture)),
                new CommandLine.Switch("cols", v => columns = int.Parse(v.First(), CultureInfo.CurrentCulture)),
                new CommandLine.Switch("localWorkSize", v => localWorkSize = int.Parse(v.First(), CultureInfo.CurrentCulture)),
                new CommandLine.Switch("iterations", v => iterations = int.Parse(v.First(), CultureInfo.CurrentCulture)));

            var platformNameRegex = new Regex(platformName.WildcardToRegex(), RegexOptions.IgnoreCase);
            Cl.Platform? currentPlatform = null;
            Cl.ErrorCode error;
            foreach (Cl.Platform platform in Cl.GetPlatformIDs(out error))
                if (platformNameRegex.Match(Cl.GetPlatformInfo(platform, Cl.PlatformInfo.Name, out error).ToString()).Success)
                {
                    currentPlatform = platform;
                    break;
                }

            if (currentPlatform == null)
            {
                Console.WriteLine("Could not find any OpenCL platforms that match \"{0}\"", platformName);
                return;
            }

            var compatibleDevices = from device in Cl.GetDeviceIDs(currentPlatform.Value, deviceType, out error)
                                   select device;
            if (compatibleDevices.Count() == 0)
            {
                Console.WriteLine("Could not find a device with type {0} on platform {1}", 
                    deviceType, Cl.GetPlatformInfo(currentPlatform.Value, Cl.PlatformInfo.Name, out error));
                return;
            }

            Console.WriteLine("Using platform {0} with device type {1}",
                              Cl.GetPlatformInfo(currentPlatform.Value, Cl.PlatformInfo.Name, out error),
                              deviceType);

            var provider = new OpenCL.ComputeProvider(compatibleDevices.ToArray().First());
            var commandQueue = new OpenCL.CommandQueue(provider, provider.Devices.First());

            var aValues = MakeMatrix(rows, columns);
            var bValues = MakeMatrix(rows, columns);
            var cParallel = new float32[rows * columns];

            var aBuffer = new OpenCL.Buffer<float32>(provider, Operations.ReadOnly, Memory.Device, aValues);
            var bBuffer = new OpenCL.Buffer<float32>(provider, Operations.ReadOnly, Memory.Device, bValues);
            var cBuffer = new OpenCL.Buffer<float32>(provider, Operations.ReadWrite, Memory.Device, cParallel);

            var matrixMult = provider.Compile<_2D, Buffer<float32>, Buffer<float32>, Buffer<float32>>(
                (range, a, b, c) => from r in range
                                    let tx = r.GlobalID0
                                    let ty = r.GlobalID1

                                    let value = 0.0f
                                    let sum = provider.Loop(0, columns, kIndices => from k in kIndices
                                                                                    let elementA = a[ty * columns + k]
                                                                                    let elementB = b[k * columns + tx]
                                                                                    select new[]
                                                                                               {
                                                                                                   value <= value + (elementA * elementB)
                                                                                               })
                                    select new[]
                                               {
                                                   c[ty * columns + tx] <= value
                                               });

            Console.Write("Multiplying two {0}x{1} matrices {2} times using .NET...", rows, columns, iterations);
            var cNormal = new float32[rows * columns];
            for (int i = 0; i < iterations; i++)
            {
                Timer<string>.Global.Start();
                Multiply(aValues, rows, columns, bValues, rows, columns, ref cNormal);
                Timer<string>.Global.Lap(".NET");
            }
            Console.WriteLine("done.");

            Console.Write("Multiplying two {0}x{1} matrices {2} times using Brahma.OpenCL and selected platform/device...", rows, columns, iterations);
            for (int i = 0; i < iterations; i++)
            {
                Timer<string>.Global.Start();
                commandQueue.Add(matrixMult.Run(new _2D(rows, columns, localWorkSize, localWorkSize), aBuffer, bBuffer, cBuffer))
                    .Finish();
                Timer<string>.Global.Lap("OpenCL");
            }
            Console.WriteLine("done.");

            Console.Write("Verifying results...");
            commandQueue.Add(cBuffer.Read(0, rows * columns, cParallel))
                .Finish();
            for (int i = 0; i < rows * columns; i++)
                if (System.Math.Abs(cParallel[i] - cNormal[i]) > 0.00001f)
                    throw new InvalidProgramException(string.Format("Expected: {0} Actual: {1} Error = {2}", cNormal[i], cParallel[i],
                                                                    System.Math.Abs(cParallel[i] - cNormal[i])));
            Console.WriteLine("done.");

            Console.WriteLine("Avg. time, C#: {0}", Timer<string>.Global.Average(".NET"));
            Console.WriteLine("Avg. time, OpenCL: {0}", Timer<string>.Global.Average("OpenCL"));

            aBuffer.Dispose();
            bBuffer.Dispose();
            cBuffer.Dispose();

            commandQueue.Dispose();
            provider.Dispose();
        }
    }
}