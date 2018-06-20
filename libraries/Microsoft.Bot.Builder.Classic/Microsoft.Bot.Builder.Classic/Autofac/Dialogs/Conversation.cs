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
using Autofac;
using Microsoft.Bot.Builder.Classic.ConnectorEx;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    /// <summary>
    /// The top level composition root for the SDK.
    /// </summary>
    public static partial class Conversation
    {
        private static readonly object gate = new object();
        private static IContainer container;

        static Conversation()
        {
            UpdateContainer(builder =>
            {
            });
        }

        public static IContainer Container
        {
            get
            {
                lock (gate)
                {
                    return container;
                }
            }
        }

        /// <summary>
        /// Update the Autofac container.
        /// </summary>
        /// <param name="update">The delegate that represents the update to apply.</param>
        public static void UpdateContainer(Action<ContainerBuilder> update)
        {
            lock (gate)
            {
                var builder = new ContainerBuilder();
                builder.RegisterModule(new DialogModule_MakeRoot());
                update(builder);
                container = builder.Build();
            }
        }

        /// <summary>
        /// Process an incoming message within the conversation.
        /// </summary>
        /// <remarks>
        /// This method:
        /// 1. Instantiates and composes the required components.
        /// 2. Deserializes the dialog state (the dialog stack and each dialog's state) from the <paramref name="v4Context"/> <see cref="IMessageActivity"/>.
        /// 3. Resumes the conversation processes where the dialog suspended to wait for a <see cref="IMessageActivity"/>.
        /// 4. Queues <see cref="IMessageActivity"/>s to be sent to the user.
        /// 5. Serializes the updated dialog state in the messages to be sent to the user.
        /// 
        /// The <paramref name="MakeRoot"/> factory method is invoked for new conversations only,
        /// because existing conversations have the dialog stack and state serialized in the <see cref="IMessageActivity"/> data.
        /// </remarks>
        /// <param name="v4Context">The turn context containing the message sent to the bot.</param>
        /// <param name="MakeRoot">The factory method to make the root dialog.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents the message to send inline back to the user.</returns>
        public static async Task SendAsync(Microsoft.Bot.Builder.ITurnContext v4Context, Func<IDialog<object>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            using (var scope = DialogModule.BeginLifetimeScope(Container, v4Context))
            {
                DialogModule_MakeRoot.Register(scope, MakeRoot);
                await SendAsync(scope, v4Context, token);
            }
        }

        /// <summary>
        /// Resume a conversation and post the data to the dialog waiting.
        /// </summary>
        /// <param name="resumptionCookie"> The resumption cookie.</param>
        /// <param name="v4Context">The turn context containing the message sent to the bot.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns> A task that represent the message to send back to the user after resumption of the conversation.</returns>
        [Obsolete("Use the overload that uses ConversationReference instead of ResumptionCookie")]
        public static async Task ResumeAsync(ResumptionCookie resumptionCookie, Microsoft.Bot.Builder.ITurnContext v4Context, CancellationToken token = default(CancellationToken))
        {
            var conversationRef = resumptionCookie.ToConversationReference();
            await ResumeAsync(conversationRef, v4Context, token);
        }

        /// <summary>
        /// Resume a conversation and post the data to the dialog waiting.
        /// </summary>
        /// <param name="conversationReference"> The resumption cookie.</param>
        /// <param name="v4Context">The turn context containing the message sent to the bot.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns> A task that represent the message to send back to the user after resumption of the conversation.</returns>
        public static async Task ResumeAsync(ConversationReference conversationReference, Microsoft.Bot.Builder.ITurnContext v4Context, CancellationToken token = default(CancellationToken))
        {
            using (var scope = DialogModule.BeginLifetimeScope(Container, v4Context))
            {
                Func<IDialog<object>> MakeRoot = () => { throw new InvalidOperationException(); };
                DialogModule_MakeRoot.Register(scope, MakeRoot);

                await SendAsync(scope, v4Context, token);
            }
        }

        /// <summary>
        /// Disable a specific service type by replacing it with a pass through implementation.
        /// </summary>
        /// <param name="type">The service type.</param>
        /// <param name="builder">The container builder.</param>
        public static void Disable(Type type, ContainerBuilder builder)
        {
            if (typeof(IBotToUser).IsAssignableFrom(type))
            {
                builder
                .RegisterType<PassBotToUser>()
                .Keyed<IBotToUser>(type);
            }

            if (typeof(IPostToBot).IsAssignableFrom(type))
            {
                builder
                .RegisterType<PassPostToBot>()
                .Keyed<IPostToBot>(type);
            }
        }

        /// <summary>
        /// Disable a specific service type by replacing it with a pass through implementation.
        /// </summary>
        /// <param name="type">The service type.</param>
        public static void Disable(Type type)
        {
            UpdateContainer(builder =>
            {
                Disable(type, builder);
            });
        }

        public static async Task SendAsync(ILifetimeScope scope, Microsoft.Bot.Builder.ITurnContext v4Context, CancellationToken token = default(CancellationToken))
        {
            var task = scope.Resolve<IPostToBot>();
            await task.PostAsync(v4Context.Activity.AsMessageActivity(), token);
        }
    }
}