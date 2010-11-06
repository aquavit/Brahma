#region License and Copyright Notice
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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Brahma.OpenCL;
using Brahma.OpenCL.Commands;
using OpenCL.Net;

namespace Grayscale
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RGB: Brahma.IMem
        {
            public byte R;
            public byte G;
            public byte B;

            public RGB(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
            }

            IntPtr Brahma.IMem.Size
            {
                get
                {
                    return (IntPtr)3;
                }
            }

            object Brahma.IMem.Data
            {
                get
                {
                    return this;
                }
            }

            public static Brahma.Set<RGB> operator <=(RGB lhs, RGB rhs)
            {
                return new Brahma.Set<RGB>(lhs, rhs);
            }

            public static Brahma.Set<RGB> operator >=(RGB lhs, RGB rhs)
            {
                throw new NotSupportedException();
            }
        }
        
        static void Main(string[] args)
        {
            Cl.ErrorCode error;

            var devices = (from dev in
                               Cl.GetDeviceIDs(
                                   (from platform in Cl.GetPlatformIDs(out error)
                                    select platform).Last(), Cl.DeviceType.Default, out error)
                           select dev).ToArray();
            var provider = new ComputeProvider(devices);

            var image = Image.FromFile("Dylan1.jpg") as Bitmap;
            var imageWidth = image.Width;
            var imageHeight = image.Height;

            var rect = new Rectangle(0, 0, imageWidth, imageHeight);
            System.Drawing.Imaging.BitmapData bmpData =
                image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                image.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            var inputBuffer = new Buffer<RGB>(provider, Operations.ReadOnly, false, imageWidth * imageHeight);
            var outputBuffer = new Buffer<RGB>(provider, Operations.WriteOnly, false, imageWidth * imageHeight);

            //var grayscale = provider.Compile<_2D, Buffer<RGB>, Buffer<RGB>>(
            //    (range, input, output) => from r in range
            //                              let index = r.GlobalIDs.y * imageWidth + r.GlobalIDs.x
            //                              let color = input[index]
            //                              let gray = (byte)((color.R + color.G + color.B) / 3.0f)
            //                              select new[]
            //                              {
            //                                  output[index] <= new RGB(gray, gray, gray)
            //                              });

            //var grayscaleProper = provider.Compile<_2D, Buffer<RGB>, Buffer<RGB>>(
            //    (range, input, output) => from r in range
            //                              let index = r.GlobalIDs.y*imageWidth + r.GlobalIDs.x
            //                              let color = input[index]
            //                              let gray = color.R * 0.3f + color.G * 0.59f + color.B * 0.11f
            //                              let clampedGray = (byte)(gray > 255 ? 255 : gray)
            //                              select new[]
            //                                         {
            //                                             output[index] <= new RGB(clampedGray, clampedGray, clampedGray)
            //                                         });

            var averageBlur = provider.Compile<_2D, Buffer<RGB>, Buffer<RGB>>(
                (range, input, output) => from r in range
                                          let index = r.GlobalIDs.y * imageWidth + r.GlobalIDs.x

                                          let xM1yM1 = input[(r.GlobalIDs.y - 1) * imageWidth + (r.GlobalIDs.x - 1)]
                                          let xyM1 = input[(r.GlobalIDs.y - 1) * imageWidth + r.GlobalIDs.x]
                                          let xP1yM1 = input[(r.GlobalIDs.y - 1) * imageWidth + (r.GlobalIDs.x + 1)]
                                          let xM1y = input[r.GlobalIDs.y * imageWidth + (r.GlobalIDs.x - 1)]
                                          let xy = input[index]
                                          let xP1y = input[r.GlobalIDs.y * imageWidth + (r.GlobalIDs.x + 1)]
                                          let xM1yP1 = input[(r.GlobalIDs.y + 1) * imageWidth + (r.GlobalIDs.x - 1)]
                                          let xyP1 = input[(r.GlobalIDs.y + 1) * imageWidth + r.GlobalIDs.x]
                                          let xP1yP1 = input[(r.GlobalIDs.y + 1) * imageWidth + (r.GlobalIDs.x + 1)]
                                          select new[]
                                                     {
                                                         output[index] <= new RGB(
                                                             (byte)((xM1yM1.R + xyM1.R + xP1yM1.R + xM1y.R + xy.R + xP1y.R + xM1yP1.R + xyP1.R + xP1yP1.R) / 9),
                                                             (byte)((xM1yM1.G + xyM1.G + xP1yM1.G + xM1y.G + xy.G + xP1y.G + xM1yP1.G + xyP1.G + xP1yP1.G) / 9),
                                                             (byte)((xM1yM1.B + xyM1.B + xP1yM1.B + xM1y.B + xy.B + xP1y.B + xM1yP1.B + xyP1.B + xP1yP1.B) / 9))
                                                     });

            //var sepia = provider.Compile<_2D, Buffer<RGB>, Buffer<RGB>>(
            //    (range, input, output) => from r in range
            //                              let index = r.GlobalIDs.y * imageWidth + r.GlobalIDs.x
            //                              let color = input[index]
            //                              let sepiaRed = color.R * 0.393f + color.G * 0.769f + color.B * 0.189f
            //                              let sepiaGreen = color.R * 0.349f + color.G * 0.686f + color.B * 0.168f
            //                              let sepiaBlue = color.R * 0.272f + color.G * 0.534f + color.B * 0.131f
            //                              let clampedSepiaRed = (byte)(sepiaRed > 255 ? 255 : sepiaRed)
            //                              let clampedSepiaGreen = (byte)(sepiaGreen > 255 ? 255 : sepiaGreen)
            //                              let clampedSepiaBlue = (byte)(sepiaBlue > 255 ? 255 : sepiaBlue)
            //                              select new[]
            //                              {
            //                                  output[index] <= new RGB(clampedSepiaRed, clampedSepiaGreen, clampedSepiaBlue)
            //                              });

            var commandQueue = new CommandQueue(provider, devices.First());
            commandQueue.Add(
                inputBuffer.Write(0, imageWidth * imageHeight, bmpData.Scan0),
                "kernel" <= averageBlur.Run(new _2D(imageWidth, imageHeight), inputBuffer, outputBuffer),
                (WaitFor)"kernel" & outputBuffer.Read(0, imageWidth * imageHeight, bmpData.Scan0)
                );
            image.UnlockBits(bmpData);
            image.Save("Output.jpg");

            commandQueue.Dispose();
            provider.Dispose();
        }
    }
}