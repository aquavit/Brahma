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
using OpenCL.Net;

namespace OpenCLPlatforms
{
    class Program
    {
        static void Main(string[] args)
        {
            // Error declaration
            Cl.ErrorCode error;

            foreach (var platform in Cl.GetPlatformIDs(out error))
            {
                Console.WriteLine("Name: {0}\tVendor: {1}",
                    Cl.GetPlatformInfo(platform, Cl.PlatformInfo.Name, out error).ToString(),
                    Cl.GetPlatformInfo(platform, Cl.PlatformInfo.Vendor, out error).ToString());

                foreach (var device in Cl.GetDeviceIDs(platform, Cl.DeviceType.All, out error))
                    Console.WriteLine("\tDevice name: {0}", Cl.GetDeviceInfo(device, Cl.DeviceInfo.Name, out error));
            }
        }
    }
}