#region License and Copyright Notice
// Copyright (c) 2010 Ananth B.
// All rights reserved.
// 
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.
#endregion

using System;
using System.Collections.Generic;

namespace Brahma.OpenCL
{
    internal static class Translator<T>
    {
        private static readonly List<KeyValuePair<Func<T, bool>, Func<CLCodeGenerator.ExpressionProcessor, T, string>>> _translators = 
            new List<KeyValuePair<Func<T, bool>, Func<CLCodeGenerator.ExpressionProcessor, T, string>>>();
        private static Func<CLCodeGenerator.ExpressionProcessor, T, string> _default = null;

        public static void Register(Func<T, bool> matcher, Func<CLCodeGenerator.ExpressionProcessor, T, string> translator)
        {
            _translators.Add(new KeyValuePair<Func<T, bool>, Func<CLCodeGenerator.ExpressionProcessor, T, string>>(matcher, translator));
        }

        public static void Register(IEnumerable<KeyValuePair<Func<T, bool>, Func<CLCodeGenerator.ExpressionProcessor, T, string>>> translators)
        {
            foreach (var translator in translators)
                _translators.Add(translator);
        }

        public static void RegisterDefault(Func<CLCodeGenerator.ExpressionProcessor, T, string> defaultTranslator)
        {
            _default = defaultTranslator;
        }

        public static bool CanTranslate(T obj)
        {
            foreach (var translator in _translators)
                if (translator.Key(obj))
                    return true;

            return false;
        }

        public static string Translate(CLCodeGenerator.ExpressionProcessor visitor, T obj)
        {
            foreach (var translator in _translators)
                if (translator.Key(obj))
                    return translator.Value(visitor, obj);

            return (_default== null) ? string.Empty : _default(visitor, obj);
        }

        public static Func<CLCodeGenerator.ExpressionProcessor, T, string> Default
        {
            get
            {
                return _default;
            }
        }
    }
}