using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugger
{
    public sealed class ReferenceEquality<T> : IEqualityComparer<T>
    {
        public static readonly IEqualityComparer<T> Instance = new ReferenceEquality<T>();
        private ReferenceEquality()
        {
        }
        bool IEqualityComparer<T>.Equals(T x, T y) => object.ReferenceEquals(x, y);
        int IEqualityComparer<T>.GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }

    public sealed class Identifier<T> : IEnumerable<KeyValuePair<int, T>>
    {
        private readonly Dictionary<T, int> codeByItem = new Dictionary<T, int>(ReferenceEquality<T>.Instance);
        private readonly Dictionary<int, T> itemByCode = new Dictionary<int, T>();
        private readonly object gate = new object();
        private int last = 0;

        public int Add(T item)
        {
            lock (gate)
            {
                if (!this.codeByItem.TryGetValue(item, out var code))
                {
                    // avoid falsey values
                    code = ++last;
                    this.codeByItem.Add(item, code);
                    this.itemByCode.Add(code, item);
                }

                return code;
            }
        }
        public void Remove(T item)
        {
            lock (gate)
            {
                var code = this.codeByItem[item];
                this.itemByCode.Remove(code);
                this.codeByItem.Remove(item);
            }
        }

        IEnumerator<KeyValuePair<int, T>> IEnumerable<KeyValuePair<int, T>>.GetEnumerator()
        {
            lock (gate)
            {
                return this.itemByCode.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<int, T>>)this).GetEnumerator();

        public T this[int code]
        {
            get
            {
                lock (gate)
                {
                    return this.itemByCode[code];
                }
            }
        }
        public int this[T item]
        {
            get
            {
                lock (gate)
                {
                    return this.codeByItem[item];
                }
            }
        }

        public bool TryGetValue(int code, out T item)
        {
            lock (gate)
            {
                return this.itemByCode.TryGetValue(code, out item);
            }
        }
    }
}
