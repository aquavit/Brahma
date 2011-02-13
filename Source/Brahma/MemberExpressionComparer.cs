﻿#region License and Copyright Notice
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

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Brahma
{
    public sealed class MemberExpressionComparer : IEqualityComparer<MemberExpression>
    {
        #region IEqualityComparer<MemberExpression> Members

        public bool Equals(MemberExpression x, MemberExpression y)
        {
            return x.ToString() == y.ToString();
        }

        public int GetHashCode(MemberExpression obj)
        {
            return obj.ToString().GetHashCode();
        }

        #endregion
    }
}
