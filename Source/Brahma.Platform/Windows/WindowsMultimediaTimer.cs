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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Brahma.Platform.Windows
{
    internal sealed class WindowsMultimediaTimer: IPerformanceTimer
    {
        // Contains Win32 API bindings for using the high performance counter

        private readonly long _frequency; // Frequency of the counter
        private readonly Stack<long> _startTimeStack = new Stack<long>(); // A stack to contain start times of this counter

        public WindowsMultimediaTimer()
        {
            // Try to initialize, throw an exception if we can't
            if (NativeCode.QueryPerformanceFrequency(out _frequency) == false)
                throw new Win32Exception("High peformance counters are not supported on this system");
        }

        #region IPerformanceTimer Members

        void IPerformanceTimer.Start()
        {
            long startTime;
            // Find the time at which this start happened
            NativeCode.QueryPerformanceCounter(out startTime);

            // Push it onto a stack, so we can pop it when stopped
            _startTimeStack.Push(startTime);
        }

        double IPerformanceTimer.Stop()
        {
            // Are the starts and stops matched?
            if (_startTimeStack.Count == 0)
                throw new InvalidOperationException("Unmatched starts and stops, please match the number of starts and stops");

            long stopTime;
            // Find the stop time
            NativeCode.QueryPerformanceCounter(out stopTime);

            // Find out how much time passed between the last Start() and Stop() by popping values from _startTimeStack
            return (double)(stopTime - _startTimeStack.Pop()) / _frequency;
        }

        #endregion

        private sealed class NativeCode
        {
            [DllImport("Kernel32.dll")]
            public static extern bool QueryPerformanceCounter(out long count);

            [DllImport("Kernel32.dll")]
            public static extern bool QueryPerformanceFrequency(out long frequency);
        }
    }
}