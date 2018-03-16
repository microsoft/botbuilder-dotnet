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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Classic.Scorables;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    /// <summary>
    /// This event loop dispatches incoming activities to a scorable action, and then if the
    /// scorable action does not match, some inner consumer of activities (usually the dialog system).
    /// </summary>
    public sealed class ScoringEventLoop<Score> : IEventLoop
    {
        private readonly IEventLoop innerLoop;
        private readonly IEventProducer<IActivity> innerProducer;
        private readonly IEventConsumer<IActivity> consumer;
        private readonly IScorable<IActivity, Score> scorable;
        public ScoringEventLoop(IEventLoop innerLoop, IEventProducer<IActivity> innerProducer, IEventConsumer<IActivity> consumer, IScorable<IActivity, Score> scorable)
        {
            SetField.NotNull(out this.innerLoop, nameof(innerLoop), innerLoop);
            SetField.NotNull(out this.innerProducer, nameof(innerProducer), innerProducer);
            SetField.NotNull(out this.consumer, nameof(consumer), consumer);
            SetField.NotNull(out this.scorable, nameof(scorable), scorable);
        }

        async Task IEventLoop.PollAsync(CancellationToken token)
        {
            // for proactive dialogs
            await this.innerLoop.PollAsync(token);

            IActivity activity;
            while (this.consumer.TryPull(out activity))
            {
                // for event wait completions
                await this.innerLoop.PollAsync(token);

                if (await this.scorable.TryPostAsync(activity, token))
                {
                }
                else
                {
                    this.innerProducer.Post(activity);
                }

                // for normal wait completions
                await this.innerLoop.PollAsync(token);
            }
        }
    }
}
