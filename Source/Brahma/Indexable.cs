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

namespace Brahma
{
    public delegate TValue IndexerGetter<TKey, TValue>(TKey key);

    public delegate void IndexerSetter<TKey, TValue>(TKey key, TValue value);

    public sealed class Indexable<TKey, TValue>
    {
        private readonly IndexerGetter<TKey, TValue> getter;
        private readonly IndexerSetter<TKey, TValue> setter;

        public Indexable(IndexerGetter<TKey, TValue> getter)
            : this(getter, null)
        {
        }

        public Indexable(IndexerSetter<TKey, TValue> setter)
            : this(null, setter)
        {
        }

        public Indexable(IndexerGetter<TKey, TValue> getter, IndexerSetter<TKey, TValue> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public TValue this[TKey key]
        {
            get
            {
                return getter(key);
            }
            set
            {
                setter(key, value);
            }
        }
    }
}