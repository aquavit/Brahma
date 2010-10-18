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
using System.Linq;
using Brahma.OpenCL;
using Brahma.Types;
using OpenCL.Net;

namespace SimpleMath
{
    class Program
    {
        static void Main(string[] args)
        {
            Cl.ErrorCode error;

            var devices = (from dev in
                               Cl.GetDeviceIDs(
                                   (from platform in Cl.GetPlatformIDs(out error)
                                    select platform).Last(), Cl.DeviceType.Default, out error)
                           select dev).ToArray();
            var provider = new ComputeProvider(devices);

            var commandQueue = new CommandQueue(provider, devices.First());

            var random = new Random();

            var inputData = (from index in Enumerable.Range(0, 100)
                             select (single)random.NextDouble()).ToArray();
            var outputData = (from index in Enumerable.Range(0, 100)
                              select (single)0f).ToArray();

            var kernel = provider.Compile<_1D, Buffer<single>, Buffer<single>>(
                (range, input, output) => from r in range
                                          select new[]
                                          {
                                              output[r.GlobalIDs.x] <= input[r.GlobalIDs.x]
                                          });

            var inputBuffer = new Buffer<single>(provider, Operations.ReadOnly, false, 100);
            var outputBuffer = new Buffer<single>(provider, Operations.WriteOnly, false, 100);

            commandQueue.Add(
                inputBuffer.Write(0, 100, inputData),
                kernel.Run(new _1D(100), inputBuffer, outputBuffer),
                outputBuffer.Read(0, 100, outputData));

            for (int i = 0; i < 100; i++)
                if (inputData[i] != outputData[i])
                    throw new Exception();

            commandQueue.Dispose();
        }
    }
}