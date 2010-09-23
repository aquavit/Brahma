using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenCL.Net;

using NUnit.Framework;

namespace Brahma.OpenCL.Tests
{
    [TestFixture]
    public sealed class ComputeProviderTests
    {
        [Test]
        public void CreateComputeProviderFromDeviceList()
        {
            Cl.ErrorCode error;
            // Select the device(s) we want to create a ComputeProvider for
            var devices = (from dev in
                               Cl.GetDeviceIDs(
                                   (from platform in Cl.GetPlatformIDs(out error)
                                    select platform).First(), Cl.DeviceType.Default, out error)
                           select dev).ToArray();
            // Just create and get rid of it.
            var provider = new ComputeProvider(devices);
            provider.Dispose();
        }
    }
}

/*
    // Usage example
    public static class Program
    {
        public struct Coefficients
        {
            public float a;
            public float b;
            public float factor;
        }
        
        static void main()
        {
            ComputeProvider provider = null;
            CommandQueue queue = new CommandQueue(provider);
            var coefficients = new Buffer<Coefficients>();
            var coeffData = new Coefficients[10];
            var image2D = new Image2D<A<Float>>();

            queue.Add(
                "write" <= coefficients.WriteAsync(0, 10, coeffData),
                (WaitFor)"write" & provider.Compile<Coefficients, float>(coeffs => from c in coeffs select c.factor).Run(coefficients),
                coefficients.Read(0, 10, coeffData)
                );
        }
    }
*/