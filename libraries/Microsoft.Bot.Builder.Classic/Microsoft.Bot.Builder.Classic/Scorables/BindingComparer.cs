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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    public sealed class BindingComparer : IComparer<IBinding>
    {
        public static readonly IComparer<IBinding> Instance = new BindingComparer();

        private BindingComparer()
        {
        }

        private bool TryCompareParameterTypeAssignability(IReadOnlyList<Type> one, IReadOnlyList<Type> two, int index, out int compare)
        {
            var l = one[index];
            var r = two[index];
            if (l.Equals(r))
            {
                compare = 0;
                return true;
            }
            if (l.IsAssignableFrom(r))
            {
                compare = -1;
                return true;
            }
            else if (r.IsAssignableFrom(l))
            {
                compare = +1;
                return true;
            }

            compare = 0;
            return false;
        }

        public static int UpdateComparisonConsistently(IBinding one, IBinding two, int compareOld, int compareNew)
        {
            if (compareOld == 0)
            {
                return compareNew;
            }
            else if (compareNew == 0)
            {
                return compareOld;
            }
            else if (compareNew == compareOld)
            {
                return compareOld;
            }
            else
            {
                throw new MethodResolutionException("inconsistent parameter overrides", one, two);
            }
        }

        int IComparer<IBinding>.Compare(IBinding one, IBinding two)
        {
            int compare = 0;

            var oneTypes = one.Method.CachedParameterTypes();
            var twoTypes = two.Method.CachedParameterTypes();

            var count = Math.Min(oneTypes.Count, twoTypes.Count);
            for (int index = 0; index < count; ++index)
            {
                int parameter;
                if (TryCompareParameterTypeAssignability(oneTypes, twoTypes, index, out parameter))
                {
                    compare = UpdateComparisonConsistently(one, two, compare, parameter);
                }
                else
                {
                    throw new MethodResolutionException("inconsistent parameter types", one, two);
                }
            }

            int length = oneTypes.Count.CompareTo(twoTypes.Count);
            compare = UpdateComparisonConsistently(one, two, compare, length);
            return compare;
        }
    }

    [Serializable]
    public sealed class MethodResolutionException : Exception
    {
        public MethodResolutionException(string message, IBinding one, IBinding two)
            : base($"{message}: {one} and {two}")
        {
        }

        private MethodResolutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
