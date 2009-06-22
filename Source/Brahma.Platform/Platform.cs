#region License and Copyright Notice

//Brahma 2.0: Framework for streaming/parallel computing with an emphasis on GPGPU

//Copyright (c) 2007 Ananth B.
//All rights reserved.

//The contents of this file are made available under the terms of the
//Eclipse Public License v1.0 (the "License") which accompanies this
//distribution, and is available at the following URL:
//http://www.opensource.org/licenses/eclipse-1.0.php

//Software distributed under the License is distributed on an "AS IS" basis,
//WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
//the specific language governing rights and limitations under the License.

//By using this software in any fashion, you are agreeing to be bound by the
//terms of the License.

#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Brahma.Platform
{
    public static class Platform
    {
        #region WindowManager enum

        public enum WindowManager
        {
            Unknown,
            Windows,
            OSX,
            X11
        }

        #endregion

        private static readonly WindowManager _windowingManager = WindowManager.Unknown;

        // Code taken from "Mono: A Developer's Notebook"

        static Platform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.WinCE:
                case PlatformID.Win32Windows:
                case PlatformID.Win32S:
                case PlatformID.Win32NT:
                    _windowingManager = WindowManager.Windows; // Windows
                    
                    break;
                
                case PlatformID.Unix:
                    switch (GetKernelName())
                    {
                        case "Unix":
                        case "Linux":
                            _windowingManager = WindowManager.X11; // Linux
                            break;

                        case "Darwin":
                            _windowingManager = WindowManager.OSX; // Mac OS
                            break;

                        default:
                            throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
                                                                          "The platform \"{0}\" is not supported. Please send this error message along with your platform details to ananth<at>ananthonline<dot>net",
                                                                          GetKernelName()));
                    }

                    break;

                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
                                                                  "The platform \"{0}\" is not supported. Please send this error message along with your platform details to ananth<at>ananthonline<dot>net",
                                                                  GetKernelName()));
            }
        }

        public static WindowManager WindowingManager
        {
            get
            {
                return _windowingManager;
            }
        }

        private static string GetKernelName()
        {
            var startInfo = new ProcessStartInfo
                                {
                                    Arguments = "-s",
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    UseShellExecute = false
                                };
            foreach (string unameprog in new[] { "/usr/bin/uname", "/bin/uname", "uname" })
            {
                try
                {
                    startInfo.FileName = unameprog;
                    Process uname = Process.Start(startInfo);
                    if (uname == null)
                        throw new InvalidOperationException("Could not start uname process on current platform");

                    StreamReader stdout = uname.StandardOutput;
                    return stdout.ReadLine().Trim();
                }
                catch (FileNotFoundException)
                {
                    // The requested executable doesn't exist, try next one.
                    continue;
                }
                catch (Win32Exception)
                {
                    continue; // We can't seem to execute it, could be due to a variety of reasons
                }
            }
            return null;
        }
    }
}