// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    public static partial class Extensions
    {
        public static T MaxBy<T, R>(this IEnumerable<T> items, Func<T, R> selectRank, IComparer<R> comparer = null)
        {
            comparer = comparer ?? Comparer<R>.Default;

            var bestItem = default(T);
            var bestRank = default(R);
            using (var item = items.GetEnumerator())
            {
                if (item.MoveNext())
                {
                    bestItem = item.Current;
                    bestRank = selectRank(item.Current);
                }

                while (item.MoveNext())
                {
                    var rank = selectRank(item.Current);
                    var compare = comparer.Compare(rank, bestRank);
                    if (compare > 0)
                    {
                        bestItem = item.Current;
                        bestRank = rank;
                    }
                }
            }

            return bestItem;
        }
    }
}

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    public static partial class Extensions
    {
        public static void Push<T>(this IList<T> stack, T item)
        {
            stack.Add(item);
        }

        public static T Pop<T>(this List<T> stack)
        {
            var top = stack.Peek();
            stack.RemoveAt(stack.Count - 1);
            return top;
        }

        public static T Peek<T>(this IReadOnlyList<T> stack)
        {
            if (stack.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty");
            }

            return stack[stack.Count - 1];
        }

        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }

        public static V GetOrAdd<K, V>(this IDictionary<K, V> valueByKey, K key, Func<K, V> make)
        {
            V value;
            if (!valueByKey.TryGetValue(key, out value))
            {
                value = make(key);
                valueByKey.Add(key, value);
            }

            return value;
        }

        public static bool Equals<T>(this IReadOnlyList<T> one, IReadOnlyList<T> two, IEqualityComparer<T> comparer)
        {
            if (object.Equals(one, two))
            {
                return true;
            }

            if (one.Count != two.Count)
            {
                return false;
            }

            for (int index = 0; index < one.Count; ++index)
            {
                if (!comparer.Equals(one[index], two[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public static IReadOnlyList<R> ToList<T, R>(this IReadOnlyList<T> source, Func<T, R> selector)
        {
            var count = source.Count;
            var target = new R[count];
            for (int index = 0; index < count; ++index)
            {
                target[index] = selector(source[index]);
            }

            return target;
        }
    }
}
