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

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    /// <summary>
    /// Allow the resolution of values based on type and optionally tag.
    /// </summary>
    /// <remarks>
    /// The tag should be restrictive to services registered with that tag.
    /// </remarks>
    public interface IResolver
    {
        bool TryResolve(Type type, object tag, out object value);
    }

    public delegate bool TryResolve(Type type, object tag, out object value);

    public static partial class Extensions
    {
        public static bool CanResolve(this IResolver resolver, Type type, object tag)
        {
            object value;
            return resolver.TryResolve(type, tag, out value);
        }

        public static object Resolve(this IResolver resolver, Type type, object tag)
        {
            object value;
            if (!resolver.TryResolve(type, null, out value))
            {
                throw new ArgumentOutOfRangeException();
            }

            return value;
        }

        public static bool TryResolve<T>(this IResolver resolver, object tag, out T value)
        {
            object inner;
            if (resolver.TryResolve(typeof(T), tag, out inner))
            {
                value = (T)inner;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
    }

    public sealed class NullResolver : IResolver
    {
        public static readonly IResolver Instance = new NullResolver();

        private NullResolver()
        {
        }

        bool IResolver.TryResolve(Type type, object tag, out object value)
        {
            value = null;
            return false;
        }
    }

    public sealed class NoneResolver : IResolver
    {
        public static readonly IResolver Instance = new NoneResolver();

        private NoneResolver()
        {
        }

        public static readonly object BoxedToken = CancellationToken.None;

        bool IResolver.TryResolve(Type type, object tag, out object value)
        {
            if (typeof(CancellationToken).IsAssignableFrom(type))
            {
                value = BoxedToken;
                return true;
            }

            value = null;
            return false;
        }
    }

    public abstract class DelegatingResolver : IResolver
    {
        protected readonly IResolver inner;

        protected DelegatingResolver(IResolver inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        public virtual bool TryResolve(Type type, object tag, out object value)
        {
            return inner.TryResolve(type, tag, out value);
        }
    }

    public sealed class EnumResolver : DelegatingResolver
    {
        public EnumResolver(IResolver inner)
            : base(inner)
        {
        }

        public override bool TryResolve(Type type, object tag, out object value)
        {
            if (type.IsEnum)
            {
                var name = tag as string;
                if (name != null)
                {
                    if (Enum.IsDefined(type, name))
                    {
                        value = Enum.Parse(type, name);
                        return true;
                    }
                }
            }

            return base.TryResolve(type, tag, out value);
        }
    }

    public sealed class ArrayResolver : DelegatingResolver
    {
        private readonly IReadOnlyList<object> services;

        public ArrayResolver(IResolver inner, IReadOnlyList<object> services)
            : base(inner)
        {
            SetField.NotNull(out this.services, nameof(services), services);
        }

        public ArrayResolver(IResolver inner, params object[] services)
            : this(inner, (IReadOnlyList<object>)services)
        {
        }

        public override bool TryResolve(Type type, object tag, out object value)
        {
            if (tag == null)
            {
                for (int index = 0; index < this.services.Count; ++index)
                {
                    var service = this.services[index];
                    if (service != null)
                    {
                        var serviceType = service.GetType();
                        if (type.IsAssignableFrom(serviceType))
                        {
                            value = service;
                            return true;
                        }
                    }
                }
            }

            return base.TryResolve(type, tag, out value);
        }
    }
}
