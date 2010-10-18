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

namespace Brahma
{
    public abstract class Set
    {
    }

    public sealed class Set<T> : Set where T : struct
    {
        private readonly T _lhs;
        private readonly T _rhs;

        public Set(T lhs, T rhs)
        {
            _lhs = lhs;
            _rhs = rhs;
        }

        public T Lhs
        {
            get
            {
                return _lhs;
            }
        }

        public T Rhs
        {
            get
            {
                return _rhs;
            }
        }
    }
}