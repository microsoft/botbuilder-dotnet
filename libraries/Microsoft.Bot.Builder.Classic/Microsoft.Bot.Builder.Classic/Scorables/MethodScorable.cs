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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;

namespace Microsoft.Bot.Builder.Classic.Scorables
{
    /// <summary>
    /// This attribute is used to specify that a method will participate in the
    /// scoring process for overload resolution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Serializable]
    public sealed class MethodBindAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is used to specify that a method parameter is bound to an entity
    /// that can be resolved by an implementation of <see cref="IResolver"/>. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    [Serializable]
    public sealed class EntityAttribute : Attribute
    {
        /// <summary>
        /// The entity name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Construct the <see cref="EntityAttribute"/>. 
        /// </summary>
        /// <param name="name">The entity name.</param>
        public EntityAttribute(string name)
        {
            SetField.NotNull(out this.Name, nameof(name), name);
        }
    }
}

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    public sealed class MethodScorableFactory : IScorableFactory<IResolver, IBinding>
    {
        IScorable<IResolver, IBinding> IScorableFactory<IResolver, IBinding>.ScorableFor(IEnumerable<MethodInfo> methods)
        {
            var specs =
                from method in methods
                from bind in InheritedAttributes.For<MethodBindAttribute>(method)
                select new { method, bind };

            var scorables = from spec in specs
                            select new MethodScorable(spec.method);

            var all = scorables.ToArray().Fold(BindingComparer.Instance);
            return all;
        }
    }

    [Serializable]
    public abstract class MethodScorableBase : ScorableBase<IResolver, IBinding, IBinding>
    {
        public abstract MethodBase Method { get; }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Method.Name})";
        }

        protected override bool HasScore(IResolver resolver, IBinding state)
        {
            return state != null;
        }

        protected override IBinding GetScore(IResolver resolver, IBinding state)
        {
            return state;
        }

        protected override Task PostAsync(IResolver item, IBinding state, CancellationToken token)
        {
            try
            {
                return state.InvokeAsync(token);
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

        protected override Task DoneAsync(IResolver item, IBinding state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Scorable to represent binding arguments to a method's parameters.
    /// </summary>
    [Serializable]
    public sealed class MethodScorable : MethodScorableBase
    {
        private readonly MethodBase method;
        public MethodScorable(MethodInfo method)
        {
            SetField.NotNull(out this.method, nameof(method), method);
        }

        public override MethodBase Method => this.method;

        protected override Task<IBinding> PrepareAsync(IResolver item, CancellationToken token)
        {
            try
            {
                IBinding binding;
                if (Binder.Instance.TryBind(this.method, item, out binding))
                {
                    return Task.FromResult(binding);
                }
                else
                {
                    return Tasks<IBinding>.Null;
                }
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled<IBinding>(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException<IBinding>(error);
            }
        }
    }

    [Serializable]
    public sealed class DelegateScorable : MethodScorableBase
    {
        private readonly Delegate lambda;
        public DelegateScorable(Delegate lambda)
        {
            SetField.NotNull(out this.lambda, nameof(lambda), lambda);
        }

        public override MethodBase Method => this.lambda.Method;

        protected override Task<IBinding> PrepareAsync(IResolver item, CancellationToken token)
        {
            try
            {
                IBinding binding;
                if (Binder.Instance.TryBind(this.lambda, item, out binding))
                {
                    return Task.FromResult(binding);
                }
                else
                {
                    return Tasks<IBinding>.Null;
                }
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled<IBinding>(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException<IBinding>(error);
            }
        }
    }
}
