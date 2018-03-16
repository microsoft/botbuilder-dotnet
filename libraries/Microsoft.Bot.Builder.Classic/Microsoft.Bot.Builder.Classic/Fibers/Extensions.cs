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
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Dialogs;

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    public static partial class Extensions
    {
        /// <summary>
        /// Without pushing or popping the stack, schedule a wait to be satisfied later.
        /// </summary>
        public static IWait<C> Wait<C, T>(this IFiber<C> fiber, Rest<C, T> resumeHandler)
        {
            var wait = fiber.Waits.Make<T>();
            wait.Wait(resumeHandler);
            fiber.Wait = wait;
            return wait;
        }

        /// <summary>
        /// Scheduled a wait for the return value, then invoke the <see cref="Call{C, T}(IFiber{C}, Rest{C, T}, T)"/> method.
        /// </summary>
        public static IWait<C> Call<C, T, R>(this IFiber<C> fiber, Rest<C, T> invokeHandler, T item, Rest<C, R> returnHandler)
        {
            // tell the leaf frame of the stack to wait for the return value
            var wait = fiber.Wait(returnHandler);
            
            // call the child
            return fiber.Call<C, T>(invokeHandler, item);
        }

        /// <summary>
        /// Push a frame on the stack, schedule a wait, and immediately satisfy that wait.
        /// </summary>
        /// <remarks>
        /// This overload is used to allow a child to later call <see cref="Done{C, T}(IFiber{C}, T)"/>
        /// to satisfy an existing wait without scheduling a new wait for the child's return value.
        /// </remarks>
        public static IWait<C> Call<C, T>(this IFiber<C> fiber, Rest<C, T> invokeHandler, T item)
        {
            // make a frame on the stack for calling the method
            fiber.Push();

            // initiate and immediately complete a wait for calling the child
            var wait = fiber.Wait(invokeHandler);
            wait.Post(item);
            return wait;
        }

        /// <summary>
        /// Remove the frame from the stack, and satisfy the existing wait with the return value.
        /// </summary>
        public static IWait<C> Done<C, T>(this IFiber<C> fiber, T item)
        {
            // pop the stack
            fiber.Done();

            // complete the caller's wait for the return value
            fiber.Wait.Post(item);
            return fiber.Wait;
        }

        public static void Reset<C>(this IFiber<C> fiber)
        {
            while (fiber.Frames.Count > 0)
            {
                fiber.Done();
            }
        }

        public static IWait<C> Post<C, T>(this IFiber<C> fiber, T item)
        {
            fiber.Wait.Post(item);
            return fiber.Wait;
        }

        public static IWait<C> Fail<C>(this IFiber<C> fiber, Exception error)
        {
            // pop the stack
            fiber.Done();

            // complete the caller's wait with an exception
            fiber.Wait.Fail(error);
            return fiber.Wait;
        }

        public static void ValidateNeed(this IWait wait, Need need)
        {
            if (need != wait.Need)
            {
                throw new InvalidNeedException(wait, need);
            }
        }

        public static IWait<C> CloneTyped<C>(this IWait<C> wait)
        {
            return (IWait<C>)wait.Clone();
        }

        public static Task<T> ToTask<T>(this IAwaitable<T> item)
        {
            var source = new TaskCompletionSource<T>();
            try
            {
                var result = item.GetAwaiter().GetResult();
                source.SetResult(result);
            }
            catch (Exception error)
            {
                source.SetException(error);
            }

            return source.Task;
        }
    }
}
