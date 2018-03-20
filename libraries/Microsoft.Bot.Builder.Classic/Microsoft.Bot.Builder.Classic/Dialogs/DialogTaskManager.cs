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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    public interface IDialogTasks
    {
        /// <summary>
        /// The list of <see cref="IDialogTask"/>
        /// </summary>
        IReadOnlyList<IDialogTask> DialogTasks { get; }

        /// <summary>
        /// Creates a new <see cref="IDialogTask"/> and add it to <see cref="DialogTasks"/>
        /// </summary>
        IDialogTask CreateDialogTask();
    }

    public interface IDialogTaskManager : IDialogTasks
    {
        // TODO: move these to separate interface, remove IDialogTaskManager (interface segregation principle)
        // TODO: possibly share with IBotData LoadAsync and FlushAsync, 

        /// <summary>
        /// Loads the <see cref="IDialogTasks.DialogTasks"/> from <see cref="IBotDataBag"/>.
        /// </summary>
        /// <param name="token"> The cancellation token.</param>
        Task LoadDialogTasks(CancellationToken token);

        /// <summary>
        /// Flushes the <see cref="IDialogTask"/> in <see cref="IDialogTasks.DialogTasks"/>
        /// </summary>
        /// <param name="token"> The cancellation token.</param>
        Task FlushDialogTasks(CancellationToken token);
    }

    /// <summary>
    /// This class is responsible for managing the set of dialog tasks.
    /// </summary>
    public sealed class DialogTaskManager : IDialogTaskManager
    {
        private readonly string blobKeyPrefix;
        private readonly IBotData botData;
        private readonly IStackStoreFactory<DialogTask> stackStoreFactory;
        private readonly Func<IDialogStack, CancellationToken, IDialogContext> contextFactory;
        private readonly IEventProducer<IActivity> queue;

        private List<DialogTask> dialogTasks;

        public DialogTaskManager(string blobKeyPrefix, IBotData botData,
            IStackStoreFactory<DialogTask> stackStoreFactory,
            Func<IDialogStack, CancellationToken, IDialogContext> contextFactory,
            IEventProducer<IActivity> queue)
        {
            SetField.NotNull(out this.blobKeyPrefix, nameof(blobKeyPrefix), blobKeyPrefix);
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.contextFactory, nameof(contextFactory), contextFactory);
            SetField.NotNull(out this.stackStoreFactory, nameof(stackStoreFactory), stackStoreFactory);
            SetField.NotNull(out this.queue, nameof(queue), queue);
        }

        async Task IDialogTaskManager.LoadDialogTasks(CancellationToken token)
        {
            if (this.dialogTasks == null)
            {
                // load all dialog tasks. By default it loads/creates the default dialog task 
                // which will be used by ReactiveDialogTask
                this.dialogTasks = new List<DialogTask>();
                do
                {
                    IDialogTaskManager dialogTaskManager = this;
                    dialogTaskManager.CreateDialogTask();
                } while (
                    this.botData.PrivateConversationData.ContainsKey(this.GetCurrentTaskBlobKey(this.dialogTasks.Count)));
            }
        }

        async Task IDialogTaskManager.FlushDialogTasks(CancellationToken token)
        {
            foreach (var dialogTask in this.dialogTasks)
            {
                dialogTask.Store.Flush();
            }
        }


        IReadOnlyList<IDialogTask> IDialogTasks.DialogTasks
        {
            get { return this.dialogTasks; }
        }

        IDialogTask IDialogTasks.CreateDialogTask()
        {
            IDialogStack stack = default(IDialogStack);
            Func<CancellationToken, IDialogContext> makeContext = token => contextFactory(stack, token);
            var task = new DialogTask(makeContext, stackStoreFactory.StoreFrom(this.GetCurrentTaskBlobKey(this.dialogTasks.Count), botData.PrivateConversationData), this.queue);
            stack = task;
            dialogTasks.Add(task);
            return task;
        }

        private string GetCurrentTaskBlobKey(int idx)
        {
            return idx == 0 ? this.blobKeyPrefix : this.blobKeyPrefix + idx;
        }
    }
}
