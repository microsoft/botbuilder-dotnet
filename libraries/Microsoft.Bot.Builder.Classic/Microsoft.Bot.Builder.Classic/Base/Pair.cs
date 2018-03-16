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
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    public static partial class Pair
    {
        public static Pair<T1, T2> Create<T1, T2>(T1 one, T2 two)
        where T1 : IEquatable<T1>, IComparable<T1>
        where T2 : IEquatable<T2>, IComparable<T2>
        {
            return new Pair<T1, T2>(one, two);
        }
    }

    public struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>, IComparable<Pair<T1, T2>>
        where T1 : IEquatable<T1>, IComparable<T1>
        where T2 : IEquatable<T2>, IComparable<T2>
    {
        public Pair(T1 one, T2 two)
        {
            this.One = one;
            this.Two = two;
        }

        public T1 One { get; }
        public T2 Two { get; }

        public int CompareTo(Pair<T1, T2> other)
        {
            var compare = this.One.CompareTo(other.One);
            if (compare != 0)
            {
                return compare;
            }

            return this.Two.CompareTo(other.Two);
        }

        public bool Equals(Pair<T1, T2> other)
        {
            return object.Equals(this.One, other.One)
                && object.Equals(this.Two, other.Two);
        }

        public override bool Equals(object other)
        {
            return other is Pair<T1, T2> && Equals((Pair<T1, T2>)other);
        }

        public override int GetHashCode()
        {
            return this.One.GetHashCode() ^ this.Two.GetHashCode();
        }
    }
}
