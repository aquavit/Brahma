using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenCL.Net;

using NUnit.Framework;

namespace Brahma.OpenCL.Tests
{
    public struct TestStructure
    {
        public float a;
        public float b;
        public int c;
    }
    
    [TestFixture]
    public sealed class BufferTests
    {
        private ComputeProvider _provider;
        
        [TestFixtureSetUp]
        public void Setup()
        {
            Cl.ErrorCode error;
            _provider = new ComputeProvider((from dev in
                               Cl.GetDeviceIDs(
                                   (from platform in Cl.GetPlatformIDs(out error)
                                    select platform).First(), Cl.DeviceType.Default, out error)
                           select dev).ToArray());
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }
        
        [Test]
        public void CreateBuffer()
        {
            // TODO: Continue here...
        }
    }
}
