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

using Microsoft.Bot.Builder.Classic.History;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.History
{
    /// <summary>
    /// Log message activities between bots and users.
    /// </summary>
    public interface IActivityLogger
    {
        Task LogAsync(IActivity activity);
    }

    /// <summary>
    /// Activity logger that traces to the console.
    /// </summary>
    /// <remarks>
    /// To use this, you need to register the class like this:
    /// <code>           
    /// var builder = new ContainerBuilder();
    /// builder.RegisterModule(Dialog_Manager.MakeRoot());
    /// builder.RegisterType&lt;TraceActivityLogger&gt;()
    ///        .AsImplementedInterfaces()
    ///        .InstancePerLifetimeScope();
    /// </code>
    /// </remarks>
    public sealed class TraceActivityLogger : IActivityLogger
    {
        /// <summary>
        /// Log activity to trace stream.
        /// </summary>
        /// <param name="activity">Activity to log.</param>
        /// <returns></returns>
        async Task IActivityLogger.LogAsync(IActivity activity)
        {
            Trace.TraceInformation(JsonConvert.SerializeObject(activity));
        }
    }

    /// <summary>
    /// Activity logger that doesn't log.
    /// </summary>
    public sealed class NullActivityLogger : IActivityLogger
    {
        /// <summary>
        /// Swallow activity.
        /// </summary>
        /// <param name="activity">Activity to be logged.</param>
        /// <returns></returns>
        async Task IActivityLogger.LogAsync(IActivity activity)
        {
        }
    }
}

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    public sealed class LogPostToBot : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly IActivityLogger logger;
        public LogPostToBot(IPostToBot inner, IActivityLogger logger)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.logger, nameof(logger), logger);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            await this.logger.LogAsync(activity);
            await inner.PostAsync(activity, token);
        }
    }

    public sealed class LogBotToUser : IBotToUser
    {
        private readonly IBotToUser inner;
        private readonly IActivityLogger logger;
        public LogBotToUser(IBotToUser inner, IActivityLogger logger)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.logger, nameof(logger), logger);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            return this.inner.MakeMessage();
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.logger.LogAsync(message);
            await this.inner.PostAsync(message, cancellationToken);
        }
    }
}
