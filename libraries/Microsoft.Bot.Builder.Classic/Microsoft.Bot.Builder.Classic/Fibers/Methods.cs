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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    public static class Methods
    {
        public static Rest<C, T> Identity<C, T>()
        {
            return IdentityMethod<C, T>.Instance.IdentityAsync;
        }

        public static Rest<C, T> Loop<C, T>(Rest<C, T> rest, int count)
        {
            var loop = new LoopMethod<C, T>(rest, count);
            return loop.LoopAsync;
        }

        public static Rest<C, T> Void<C, T>(Rest<C, T> rest)
        {
            var root = new VoidMethod<C, T>(rest);
            return root.RootAsync;
        }

        [Serializable]
        private sealed class IdentityMethod<C, T>
        {
            public static readonly IdentityMethod<C, T> Instance = new IdentityMethod<C, T>();

            private IdentityMethod()
            {
            }

            public async Task<IWait<C>> IdentityAsync(IFiber<C> fiber, C context, IItem<T> item, CancellationToken token)
            {
                return fiber.Done(await item);
            }
        }

        [Serializable]
        private sealed class LoopMethod<C, T>
        {
            private readonly Rest<C, T> rest;
            private int count;
            private T item;

            public LoopMethod(Rest<C, T> rest, int count)
            {
                SetField.NotNull(out this.rest, nameof(rest), rest);
                this.count = count;
            }

            public async Task<IWait<C>> LoopAsync(IFiber<C> fiber, C context, IItem<T> item, CancellationToken token)
            {
                this.item = await item;

                --this.count;
                if (this.count >= 0)
                {
                    return fiber.Call<C, T, object>(this.rest, this.item, NextAsync);
                }
                else
                {
                    return fiber.Done(this.item);
                }
            }

            public async Task<IWait<C>> NextAsync(IFiber<C> fiber, C context, IItem<object> ignore, CancellationToken token)
            {
                --this.count;
                if (this.count >= 0)
                {
                    return fiber.Call<C, T, object>(this.rest, this.item, NextAsync);
                }
                else
                {
                    return fiber.Done(this.item);
                }
            }
        }

        [Serializable]
        private sealed class VoidMethod<C, T>
        {
            private readonly Rest<C, T> rest;

            public VoidMethod(Rest<C, T> rest)
            {
                SetField.NotNull(out this.rest, nameof(rest), rest);
            }

            public async Task<IWait<C>> RootAsync(IFiber<C> fiber, C context, IItem<T> item, CancellationToken token)
            {
                return fiber.Call<C, T, object>(this.rest, await item, DoneAsync);
            }

            public async Task<IWait<C>> DoneAsync(IFiber<C> fiber, C context, IItem<object> ignore, CancellationToken token)
            {
                return NullWait<C>.Instance;
            }
        }
    }
}
