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
        private string _name = string.Empty;

        protected internal List<string> WaitList
        {
            get
            {
                return _waitList;
            }
        }

        public string Name
        {
            get 
            {
                return _name;
            }
            protected set 
            {
                _name = value;
            }
        }

        public static Command operator <=(string name, Command command)
        {
            command._name = name;
            return command;
        }

        public static Command operator >=(string name, Command command)
        {
            throw new NotSupportedException();
        }
    }
    
    public abstract class Command<T>: Command where T: struct
    {
    }

    public abstract class Command<T1, T2>: Command where T1: struct
                                                   where T2: struct
    {
    }

    public abstract class Command<T1, T2, T3>: Command where T1: struct
                                                       where T2: struct
                                                       where T3: struct
    {
    }
}