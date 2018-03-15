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
using System.Diagnostics;
using System.Net.Mime;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Builder.Classic.ConnectorEx;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    /// <summary>
    /// Methods to send a message from the user to the bot.
    /// </summary>
    public interface IPostToBot
    {
        /// <summary>
        /// Post an item (e.g. message or other external event) to the bot.
        /// </summary>
        /// <param name="activity">The item for the bot.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        Task PostAsync(IActivity activity, CancellationToken token);
    }

    public sealed class NullPostToBot : IPostToBot
    {
        Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }

    public sealed class PassPostToBot : IPostToBot
    {
        private readonly IPostToBot inner;

        public PassPostToBot(IPostToBot inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            await this.inner.PostAsync(activity, token);
        }
    }

    /// <summary>
    /// This IPostToBot service sets the ambient thread culture based on the <see cref="IMessageActivity.Locale"/>.
    /// </summary>
    public sealed class SetAmbientThreadCulture : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly ILocaleFinder localeFinder;

        public SetAmbientThreadCulture(IPostToBot inner, ILocaleFinder localeFinder)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.localeFinder, nameof(localeFinder), localeFinder);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            var locale = await this.localeFinder.FindLocale(activity, token);
            using (var localeScope = new LocalizedScope(locale))
            {
                await this.inner.PostAsync(activity, token);
            }
        }
    }

    /// <summary>
    /// This IPostToBot service serializes the execution of a particular conversation's code to avoid
    /// concurrency issues.
    /// </summary>
    public sealed class SerializeByConversation : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly IAddress address;
        private readonly IScope<IAddress> scopeForCookie;

        public SerializeByConversation(IPostToBot inner, IAddress address, IScope<IAddress> scopeForCookie)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.address, nameof(address), address);
            SetField.NotNull(out this.scopeForCookie, nameof(scopeForCookie), scopeForCookie);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            using (await this.scopeForCookie.WithScopeAsync(this.address, token))
            {
                await this.inner.PostAsync(activity, token);
            }
        }
    }

    /// <summary>
    /// This IPostToBot service converts any unhandled exceptions to a message sent to the user.
    /// </summary>
    public sealed class PostUnhandledExceptionToUser : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly IBotToUser botToUser;
        private readonly ResourceManager resources;
        private readonly TraceListener trace;

        public PostUnhandledExceptionToUser(IPostToBot inner, IBotToUser botToUser, ResourceManager resources, TraceListener trace)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.resources, nameof(resources), resources);
            SetField.NotNull(out this.trace, nameof(trace), trace);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            try
            {
                await this.inner.PostAsync(activity, token);
            }
            catch
            {
                try
                {
                    await this.botToUser.PostAsync(this.resources.GetString("UnhandledExceptionToUser"));
                }
                catch (Exception inner)
                {
                    this.trace.WriteLine(inner);
                }

                throw;
            }
        }
    }
}
