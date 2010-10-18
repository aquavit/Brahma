using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    public sealed class NameGenerator
    {
        private const string VarNameFormat = "var{0}";
        private const string FuncNameFormat = "Func{0}";
        
        private int _varCounter = 0;

        public string NewVarName()
        {
            return string.Format(VarNameFormat, _varCounter++);
        }

        private int _funcCounter = 0;
        public string NewFuncName()
        {
            return string.Format(FuncNameFormat, _funcCounter++);
        }
    }
}