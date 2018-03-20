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
using System.Runtime.CompilerServices;

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    public interface IAwaiter<out T> : INotifyCompletion
    {
        bool IsCompleted { get; }

        T GetResult();
    }
}

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    using Microsoft.Bot.Builder.Classic.Internals.Fibers;

    /// <summary>
    /// Explicit interface to support the compiling of async/await.
    /// </summary>
    /// <typeparam name="T">The type of the contained value.</typeparam>
    public interface IAwaitable<out T>
    {
        /// <summary>
        /// Get the awaiter for this awaitable item.
        /// </summary>
        /// <returns>The awaiter.</returns>
        Microsoft.Bot.Builder.Classic.Internals.Fibers.IAwaiter<T> GetAwaiter();
    }

    /// <summary>
    /// Creates a <see cref="IAwaitable{T}"/> from item passed to constructor.
    /// </summary>
    /// <typeparam name="T"> The type of the item.</typeparam>
    public sealed class AwaitableFromItem<T> : IAwaitable<T>, IAwaiter<T>
    {
        private readonly T item;

        public AwaitableFromItem(T item)
        {
            this.item = item;
        }
        
        bool IAwaiter<T>.IsCompleted
        {
            get { return true; }
        }

        T IAwaiter<T>.GetResult()
        {
            return item;
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        IAwaiter<T> IAwaitable<T>.GetAwaiter()
        {
            return this;
        }
    }

    public partial class Awaitable
    {
        /// <summary>
        /// Wraps item in a <see cref="IAwaitable{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the item.</typeparam>
        /// <param name="item">The item that will be wrapped.</param>
        public static IAwaitable<T> FromItem<T>(T item)
        {
            return new AwaitableFromItem<T>(item);
        }
    }
}
