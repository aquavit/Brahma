using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Brahma
{
    public abstract class Command
    {
        private readonly List<string> _waitList = new List<string>();
        
        protected Command(string name)
        { 
        }

        protected internal List<string> WaitList
        {
            get
            {
                return _waitList;
            }
        }
    }
    
    public abstract class Command<T>: Command where T: struct
    {
        protected Command(string name)
            : base(name)
        { 
        }
    }

    public abstract class Command<T1, T2>: Command where T1: struct
                                                   where T2: struct
    {
        protected Command(string name)
            : base(name)
        {
        }
    }

    public abstract class Command<T1, T2, T3>: Command where T1: struct
                                                       where T2: struct
                                                       where T3: struct
    {
        protected Command(string name)
            : base(name)
        {
        }
    }
}