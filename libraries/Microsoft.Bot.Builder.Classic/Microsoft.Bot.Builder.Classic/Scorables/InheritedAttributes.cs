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

using Microsoft.Bot.Builder.Classic.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    /// <summary>
    /// Helper methods to enumerate inherited attributes for a method.
    /// </summary>
    /// <remarks>
    /// http://bradwilson.typepad.com/blog/2011/08/interface-attributes-class-attributes.html
    /// </remarks>
    public static partial class InheritedAttributes
    {
        public static readonly ConcurrentDictionary<MethodInfo, IReadOnlyList<Attribute>> AttributesByMethod
            = new ConcurrentDictionary<MethodInfo, IReadOnlyList<Attribute>>();

        private static IReadOnlyList<Attribute> ForHelper(MethodInfo method)
        {
            var declaring = method.DeclaringType;
            var interfaces = declaring.GetInterfaces();

            var methods =
                from i in interfaces
                let map = declaring.GetInterfaceMap(i)
                let index = Array.IndexOf(map.TargetMethods, method)
                where index >= 0
                let source = map.InterfaceMethods[index]
                select source;

            Func<MethodInfo, IEnumerable<Attribute>> ExpandAttributes = m =>
            {
                var ma = m.GetCustomAttributes<Attribute>(inherit: true);
                var ta = m.DeclaringType.GetCustomAttributes<Attribute>(inherit: true);

                return ma.Concat(ta);
            };

            return ExpandAttributes(method).Concat(methods.SelectMany(m => ExpandAttributes(m))).ToArray();
        }

        public static IEnumerable<A> For<A>(MethodInfo method) where A : Attribute
        {
            return AttributesByMethod
                .GetOrAdd(method, m => ForHelper(m))
                .OfType<A>();
        }
    }
}
