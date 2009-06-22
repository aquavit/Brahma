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

namespace Brahma.Platform.X11
{
    internal sealed class TickCounterTimer: IPerformanceTimer
    {
        private readonly Stack<long> _startTimeStack = new Stack<long>(); // A stack to contain start times of this counter

        #region IPerformanceTimer Members

        void IPerformanceTimer.Start()
        {
            // Find the time at which this start happened
            // Push it onto a stack, so we can pop it when stopped
            _startTimeStack.Push(DateTime.Now.Ticks);
        }

        double IPerformanceTimer.Stop()
        {
            // Are the starts and stops matched?
            if (_startTimeStack.Count == 0)
                throw new InvalidOperationException("Unmatched starts and stops, please match the number of starts and stops");

            // Find the stop time
            long stopTime = DateTime.Now.Ticks;

            // Find out how much time passed between the last Start() and Stop() by popping values from _startTimeStack
            // Remember, subtracting two DateTimes gives us a TimeSpan
            return (new DateTime(stopTime) - new DateTime(_startTimeStack.Pop())).Seconds;
        }

        #endregion
    }
}