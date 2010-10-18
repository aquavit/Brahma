using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma.OpenCL
{
    public static class StackExtensions
    {
        public static Stack<T> Reverse<T>(this Stack<T> stack)
        {
            var result = new Stack<T>();
            while (stack.Count > 0)
            {
                T element = stack.Pop();
                result.Push(element);
            }

            return result;
        }
    }
}
