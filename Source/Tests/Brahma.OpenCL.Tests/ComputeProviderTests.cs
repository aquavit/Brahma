using System;
using System.Linq;
using NUnit.Framework;
using OpenCL.Net;

namespace Brahma.OpenCL.Tests
{
    [TestFixture]
    public sealed class ComputeProviderTests
    {
        public object[] GetPlatforms()
        {
            Cl.ErrorCode error;
            return (from platform in Cl.GetPlatformIDs(out error)
                    select (object) platform).ToArray();
        }

        [Test] 
        [TestCaseSource("GetPlatforms")]
        [Category(Categories.Correctness)]
        [Description("Creates (and disposes) one ComputeProvider for each platform found")]
        [ExpectedException]
        public void CreateComputeProvider(Cl.Platform platform)
        {
            Cl.ErrorCode error;

            // Test invalid values
            Assert.Throws<ArgumentNullException>(() => new ComputeProvider());
            Assert.Throws<ArgumentException>(() => new ComputeProvider(new Cl.Device[]{ }));
            Assert.Throws<CLException>(() =>new ComputeProvider(new Cl.Device()));

            Console.WriteLine("Creating compute provider for {0}",
                              Cl.GetPlatformInfo(platform, Cl.PlatformInfo.Name, out error));
            using (var provider = new ComputeProvider(Cl.GetDeviceIDs(platform, Cl.DeviceType.All, out error)))
            {
                // Do nothing
            }
        }
    }
}