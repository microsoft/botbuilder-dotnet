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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    /// <summary>
    /// Represents a binding of arguments to a method's parameters.
    /// </summary>
    public interface IBinding
    {
        MethodBase Method { get; }
        Task InvokeAsync(CancellationToken token);
    }

    /// <summary>
    /// Represents a binding of arguments to a method's parameter,
    /// where the method returns a value of type <typeparamref name="R"/>.
    /// </summary>
    /// <typeparam name="R">The return value type.</typeparam>
    public interface IBinding<R> : IBinding
    {
        new Task<R> InvokeAsync(CancellationToken token);
    }

    public class Binding : IBinding, IEquatable<Binding>
    {
        protected readonly MethodBase method;
        protected readonly object instance;
        protected readonly object[] arguments;

        public Binding(MethodBase method, object instance, object[] arguments)
        {
            SetField.NotNull(out this.method, nameof(method), method);
            if (this.method.IsStatic)
            {
                this.instance = instance;
            }
            else
            {
                SetField.NotNull(out this.instance, nameof(instance), instance);
            }

            SetField.NotNull(out this.arguments, nameof(arguments), arguments);
        }

        MethodBase IBinding.Method => this.method;

        Task IBinding.InvokeAsync(CancellationToken token)
        {
            try
            {
                var arguments = MakeArguments(this.method, this.arguments, token);

                var result = this.method.Invoke(this.instance, arguments);
                // if the result is a task, wait for its completion and propagate any exceptions
                var task = result as Task;
                if (task != null)
                {
                    return task;
                }

                return Task.CompletedTask;
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }

        public static object[] MakeArguments(MethodBase method, IReadOnlyList<object> source, CancellationToken token)
        {
            // late-bound provide the CancellationToken
            var parameters = method.CachedParameters();
            var target = new object[parameters.Count];
            for (int index = 0; index < parameters.Count; ++index)
            {
                var type = parameters[index].ParameterType;
                bool cancel = type.IsAssignableFrom(typeof(CancellationToken));
                if (cancel)
                {
                    var resolved = (CancellationToken)source[index];
                    if (resolved.CanBeCanceled)
                    {
                        // consider CancellationTokenSource.CreateLinkedTokenSource
                        // but remember to CancellationTokenSource.Dispose
                        throw new NotSupportedException();
                    }

                    target[index] = token;
                }
                else
                {
                    target[index] = source[index];
                }
            }

            return target;
        }

        public override string ToString()
        {
            return this.method.ToString();
        }

        public override int GetHashCode()
        {
            return this.method.GetHashCode();
        }

        public override bool Equals(object other)
        {
            IEquatable<Binding> equatable = this;
            return equatable.Equals(other as Binding);
        }

        bool IEquatable<Binding>.Equals(Binding other)
        {
            return other != null
                && object.Equals(this.method, other.method)
                && object.Equals(this.instance, other.instance)
                && Builder.Classic.Internals.Fibers.Extensions.Equals(this.arguments, other.arguments, EqualityComparer<object>.Default);
        }
    }

    public sealed class Binding<R> : Binding, IBinding<R>, IEquatable<Binding>
    {
        public Binding(MethodBase method, object instance, object[] arguments)
            : base(method, instance, arguments)
        {
        }

        async Task<R> IBinding<R>.InvokeAsync(CancellationToken token)
        {
            var arguments = MakeArguments(this.method, this.arguments, token);

            var result = this.method.Invoke(this.instance, arguments);
            // if the result is a task, wait for its completion and propagate any exceptions
            var task = result as Task<R>;
            if (task != null)
            {
                return await task;
            }

            return (R) result;
        }
    }
}