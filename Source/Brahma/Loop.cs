using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    public static class Loop
    {
        public static IEnumerable<int> For(Func<int> startValue, Func<int, bool> condition, Func<int, int> increment)
        {
            for (int i = startValue(); condition(i); i = increment(i))
                yield return i;
        }

        public static IEnumerable<bool> While(Func<bool> condition)
        {
            bool conditionValue = condition();
            while (conditionValue)
            {
                conditionValue = condition();
                yield return conditionValue;
            }
        }
    }
}