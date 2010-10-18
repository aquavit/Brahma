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

using System.Linq;
using Brahma.OpenCL;
using OpenCL.Net;

namespace Initialization
{
    class Program
    {
        static void Main(string[] args)
        {
            Cl.ErrorCode error;
            var devices = (from dev in
                               Cl.GetDeviceIDs(
                                   (from platform in Cl.GetPlatformIDs(out error)
                                    select platform).First(), Cl.DeviceType.Default, out error)
                           select dev).ToArray();
            var provider = new ComputeProvider(devices);

            provider.Dispose(); // Remember to Dispose() everything! No finalizers!
        }
    }
}