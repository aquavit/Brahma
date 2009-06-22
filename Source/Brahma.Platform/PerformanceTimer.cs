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
using System.Collections.Generic;

using Brahma.Platform.Windows;
using Brahma.Platform.X11;

namespace Brahma.Platform
{
    public static class PerformanceTimer
    {
        private static readonly Dictionary<Platform.WindowManager, Type> _implementations =
            new Dictionary<Platform.WindowManager, Type>
                {
                    // Register all implementations here
                    { Platform.WindowManager.Windows, typeof (WindowsMultimediaTimer) },
                    { Platform.WindowManager.X11, typeof (TickCounterTimer) }
                };

        private static readonly IPerformanceTimer _performanceTimer;

        static PerformanceTimer()
        {
            // Create our platform-specific instance
            _performanceTimer = Activator.CreateInstance(_implementations[Platform.WindowingManager]) as IPerformanceTimer;
        }

        // Expose the timing methods
        public static void Start()
        {
            _performanceTimer.Start();
        }

        public static double Stop()
        {
            return _performanceTimer.Stop();
        }
    }
}