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

using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    /// <summary>
    /// The dialog system represents the top-level interface for the dialog tasks and their event loop.
    /// </summary>
    public interface IDialogSystem : IDialogTasks, IEventLoop, IEventProducer<IActivity>
    {
    }

    public sealed class DialogSystem : IDialogSystem
    {
        private readonly IDialogTasks tasks;
        private readonly IEventLoop loop;
        private readonly IEventProducer<IActivity> queue;
        public DialogSystem(IDialogTasks tasks, IEventLoop loop, IEventProducer<IActivity> queue)
        {
            SetField.NotNull(out this.tasks, nameof(tasks), tasks);
            SetField.NotNull(out this.loop, nameof(loop), loop);
            SetField.NotNull(out this.queue, nameof(queue), queue);
        }

        IReadOnlyList<IDialogTask> IDialogTasks.DialogTasks => this.tasks.DialogTasks;

        IDialogTask IDialogTasks.CreateDialogTask()
        {
            return this.tasks.CreateDialogTask();
        }

        async Task IEventLoop.PollAsync(CancellationToken token)
        {
            await this.loop.PollAsync(token);
        }

        void IEventProducer<IActivity>.Post(IActivity activity, Action onPull)
        {
            this.queue.Post(activity, onPull);
        }
    }
}
