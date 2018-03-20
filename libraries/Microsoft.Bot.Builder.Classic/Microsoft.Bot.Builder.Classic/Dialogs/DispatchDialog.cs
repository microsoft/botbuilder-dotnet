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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Classic.Scorables;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    /// <summary>
    /// A dialog specialized to dispatch an IScorable.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    [Serializable]
    public class DispatchDialog<TResult> : Dispatcher, IDialog<TResult>
    {
        public virtual async Task StartAsync(IDialogContext context)
        {
            context.Wait(ActivityReceivedAsync);
        }

        [NonSerialized]
        private IReadOnlyList<object> services;
        protected override IReadOnlyList<object> MakeServices()
        {
            return this.services;
        }

        protected virtual async Task ActivityReceivedAsync(IDialogContext context, IAwaitable<IActivity> item)
        {
            var activity = await item;
            this.services = new object[] { this, context, activity };
            try
            {
                IDispatcher dispatcher = this;
                await dispatcher.TryPostAsync(context.CancellationToken);
            }
            finally
            {
                this.services = null;
            }
        }
    }

    /// <summary>
    /// A dialog specialized to dispatch an IScorable.
    /// </summary>
    /// <remarks>
    /// This non-generic dialog is intended for use as a top-level dialog that will not
    /// return to any calling parent dialog (and therefore the result type is object).
    /// </remarks>
    [Serializable]
    public class DispatchDialog : DispatchDialog<object>
    {
    }
}