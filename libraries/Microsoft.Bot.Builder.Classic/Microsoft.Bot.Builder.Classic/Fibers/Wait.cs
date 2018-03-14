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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Builder.Classic.Dialogs;

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    public interface IItem<out T> : IAwaitable<T>
    {
    }

    public delegate Task<IWait<C>> Rest<C, in T>(IFiber<C> fiber, C context, IItem<T> item, CancellationToken token);

    /// <summary>
    /// This is the stage of the wait, showing what the wait needs during its lifecycle.
    /// </summary>
    public enum Need
    {
        /// <summary>
        /// The wait does not need anything.
        /// </summary>
        None,

        /// <summary>
        /// The wait needs an item to be posted.
        /// </summary>
        Wait,

        /// <summary>
        /// The wait needs to be polled for execution after an item has been posted.
        /// </summary>
        Poll,

        /// <summary>
        /// The wait is in the middle of executing the rest delegate.
        /// </summary>
        Call,

        /// <summary>
        /// The wait has completed executing the rest delegate.
        /// </summary>
        Done
    };

    public interface IWait
    {
        /// <summary>
        /// The stage of the wait.
        /// </summary>
        Need Need { get; }

        /// <summary>
        /// The type of the item parameter for the rest delegate.
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// The static type of the wait item.
        /// </summary>
        Type NeedType { get; }

        /// <summary>
        /// The rest delegate method.
        /// </summary>
        Delegate Rest { get; }

        /// <summary>
        /// Mark this wait as satisfied with this item.
        /// </summary>
        void Post<T>(T item);

        /// <summary>
        /// Mark this wait as satisfied with this fail exception.
        /// </summary>
        void Fail(Exception error);
    }

    public interface IWait<C> : IWait, ICloneable
    {
        Task<IWait<C>> PollAsync(IFiber<C> fiber, C context, CancellationToken token);
    }

    /// <summary>
    /// Null object pattern implementation of wait interface.
    /// </summary>
    public sealed class NullWait<C> : IWait<C>
    {
        public static readonly IWait<C> Instance = new NullWait<C>();
        private NullWait()
        {
        }

        Need IWait.Need => Need.None;

        Type IWait.NeedType => typeof(object);

        Delegate IWait.Rest
        {
            get
            {
                throw new InvalidNeedException(this, Need.None);
            }
        }

        Type IWait.ItemType => typeof(object);

        void IWait.Post<T>(T item)
        {
            throw new InvalidNeedException(this, Need.Wait);
        }

        void IWait.Fail(Exception error)
        {
            throw new InvalidNeedException(this, Need.Wait);
        }

        Task<IWait<C>> IWait<C>.PollAsync(IFiber<C> fiber, C context, CancellationToken token)
        {
            throw new InvalidNeedException(this, Need.Poll);
        }

        object ICloneable.Clone()
        {
            return NullWait<C>.Instance;
        }
    }

    public interface IWait<C, out T> : IWait<C>
    {
        void Wait(Rest<C, T> rest);
    }

    public interface IPost<in T>
    {
        void Post(T item);
    }

    public sealed class PostStruct<T> : IPost<T>
    {
        private readonly IPost<object> postBoxed;
        public PostStruct(IPost<object> postBoxed)
        {
            SetField.NotNull(out this.postBoxed, nameof(postBoxed), postBoxed);
        }
        void IPost<T>.Post(T item)
        {
            this.postBoxed.Post((object)item);
        }
    }

    [Serializable]
    public sealed class Wait<C, T> : IItem<T>, IWait<C, T>, IPost<T>, IAwaiter<T>, IEquatable<Wait<C, T>>, ISerializable
    {
        private Rest<C, T> rest;
        private Need need;
        private T item;
        private Exception fail;

        public Wait()
        {
        }

        private Wait(SerializationInfo info, StreamingContext context)
        {
            SetField.NotNullFrom(out this.rest, nameof(rest), info);
            SetField.From(out this.need, nameof(need), info);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.rest), this.rest);
            info.AddValue(nameof(this.need), this.need);
        }

        public override string ToString()
        {
            IWait wait = this;
            return $"Wait: {wait.Need} {wait.NeedType?.Name} for {this.rest?.Target}.{this.rest?.Method.Name} have {wait.ItemType?.Name} {this.item}";
        }

        public override int GetHashCode()
        {
            return this.rest.GetHashCode();
        }

        public override bool Equals(object other)
        {
            IEquatable<Wait<C, T>> wait = this;
            return wait.Equals(other as Wait<C, T>);
        }

        bool IEquatable<Wait<C, T>>.Equals(Wait<C, T> other)
        {
            return other != null
                && object.Equals(this.rest, other.rest)
                && object.Equals(this.need, other.need)
                && object.Equals(this.item, other.item)
                && object.Equals(this.fail, other.fail)
                ;
        }

        Need IWait.Need => this.need;

        Type IWait.NeedType
        {
            get
            {
                if (this.rest != null)
                {
                    var method = this.rest.Method;
                    var parameters = method.GetParameters();
                    var itemType = parameters[2].ParameterType;
                    var type = itemType.GenericTypeArguments.Single();
                    return type;
                }
                else
                {
                    return null;
                }
            }
        }

        Delegate IWait.Rest => this.rest;

        Type IWait.ItemType => typeof(T);

        async Task<IWait<C>> IWait<C>.PollAsync(IFiber<C> fiber, C context, CancellationToken token)
        {
            this.ValidateNeed(Need.Poll);

            this.need = Need.Call;
            try
            {
                return await this.rest(fiber, context, this, token);
            }
            finally
            {
                this.need = Need.Done;
            }
        }

        private static readonly MethodInfo MethodPost = Types.MethodOf(() => ((IWait)null).Post(0)).GetGenericMethodDefinition();

        void IWait.Post<D>(D item)
        {
            this.ValidateNeed(Need.Wait);

            // try generic type variance first
            var post = this as IPost<D>;
            if (post == null)
            {
                // then work around lack of generic type variant for value types
                if (typeof(D).IsValueType)
                {
                    var postBoxed = this as IPost<object>;
                    if (postBoxed != null)
                    {
                        post = new PostStruct<D>(postBoxed);
                    }
                }
            }

            if (post != null)
            {
                post.Post(item);
            }
            else
            {
                // if we have runtime type information, use reflection and recurse
                var type = item?.GetType();
                bool reflection = type != null && !type.IsAssignableFrom(typeof(D));
                if (reflection)
                {
                    var generic = MethodPost.MakeGenericMethod(type);
                    generic.Invoke(this, new object[] { item });
                }
                else
                {
                    // otherwise, we cannot satisfy this wait with this item
                    IWait wait = this;
                    wait.Fail(new InvalidTypeException(this, typeof(D)));
                }
            }
        }

        void IWait.Fail(Exception fail)
        {
            this.ValidateNeed(Need.Wait);

            this.item = default(T);
            this.fail = fail;
            this.need = Need.Poll;
        }

        void IPost<T>.Post(T item)
        {
            this.ValidateNeed(Need.Wait);

            this.item = item;
            this.fail = null;
            this.need = Need.Poll;
        }

        void IWait<C, T>.Wait(Rest<C, T> rest)
        {
            this.ValidateNeed(Need.None);

            SetField.NotNull(out this.rest, nameof(rest), rest);
            this.need = Need.Wait;
        }

        IAwaiter<T> IAwaitable<T>.GetAwaiter()
        {
            return this;
        }

        bool IAwaiter<T>.IsCompleted
        {
            get
            {
                switch (this.need)
                {
                    case Need.Call:
                    case Need.Done:
                        return true;
                    default:
                        return false;
                }
            }
        }

        T IAwaiter<T>.GetResult()
        {
            if (this.fail != null)
            {
                // http://stackoverflow.com/a/17091351
                ExceptionDispatchInfo.Capture(this.fail).Throw();

                // just to satisfy compiler - should not reach this line
                throw new InvalidOperationException();
            }
            else
            {
                return this.item;
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        object ICloneable.Clone()
        {
            var clone = new Wait<C, T>();
            clone.rest = this.rest;
            clone.need = Need.Wait;
            clone.item = default(T);
            clone.fail = null;
            return clone;
        }
    }

    public interface IWaitFactory<C>
    {
        IWait<C, T> Make<T>();
    }

    [Serializable]
    public sealed class WaitFactory<C> : IWaitFactory<C>
    {
        IWait<C, T> IWaitFactory<C>.Make<T>()
        {
            return new Wait<C, T>();
        }
    }
}
