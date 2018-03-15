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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Base
{
    public interface IEventLoop
    {
        /// <summary>
        /// Poll the target for any work to be done.
        /// </summary>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing the poll operation.</returns>
        Task PollAsync(CancellationToken token);
    }

    public interface IEventProducer<in Event>
    {
        void Post(Event @event, Action onPull = null);
    }

    public interface IEventConsumer<Event>
    {
        bool TryPull(out Event @event);
    }

    public sealed class EventQueue<Event> : IEventProducer<Event>, IEventConsumer<Event>
    {
        private struct Item
        {
            public Event Event { get; set; }
            public Action OnPull { get; set; }
        }

        private readonly Queue<Item> queue = new Queue<Item>();
        void IEventProducer<Event>.Post(Event @event, Action onPull)
        {
            var item = new Item() { Event = @event, OnPull = onPull };
            this.queue.Enqueue(item);
        }

        bool IEventConsumer<Event>.TryPull(out Event @event)
        {
            if (queue.Count > 0)
            {
                var item = this.queue.Dequeue();
                @event = item.Event;
                var onPull = item.OnPull;
                onPull?.Invoke();

                return true;
            }
            else
            {
                @event = default(Event);
                return false;
            }
        }
    }
}
