using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    public abstract class WaitFor: Command
    {
        private const string AnonymousWaitForName = "WaitFor_{0}";
        private static int _anonymousWaitForID = 0;

        private WaitFor()
            : base(string.Format(AnonymousWaitForName, _anonymousWaitForID++))
        {
        }

        protected WaitFor(string name)
            : this()
        {
        }

        public static Command operator &(WaitFor wait, Command command)
        {
            command.WaitList.AddRange(wait.WaitList);
            return command;
        }

        public static WaitFor operator &(WaitFor wait1, WaitFor wait2)
        {
            wait1.WaitList.AddRange(wait2.WaitList);
            return wait1;
        }
    }
}