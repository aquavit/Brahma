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
using NUnit.Framework;
using OpenCL.Net;

namespace Brahma.OpenCL.Tests
{
    [TestFixture]
    public sealed class ComputeProviderTests
    {
        public object[] GetPlatforms()
        {
            ErrorCode error;
            return (from platform in Cl.GetPlatformIDs(out error)
                    select (object) platform).ToArray();
        }

        [Test] 
        [TestCaseSource("GetPlatforms")]
        [Category(Categories.Correctness)]
        [Description("Creates (and disposes) one ComputeProvider for each platform found")]
        [ExpectedException]
        public void CreateComputeProvider(Platform platform)
        {
            ErrorCode error;

            // Test invalid values
            Assert.Throws<ArgumentNullException>(() => new ComputeProvider());
            Assert.Throws<ArgumentException>(() => new ComputeProvider(new Device[]{ }));
            Assert.Throws<CLException>(() =>new ComputeProvider(new Device()));

            Console.WriteLine("Creating compute provider for {0}",
                              Cl.GetPlatformInfo(platform, PlatformInfo.Name, out error));
            using (var provider = new ComputeProvider(Cl.GetDeviceIDs(platform, DeviceType.All, out error)))
            {
                // Do nothing
            }
        }
    }
}