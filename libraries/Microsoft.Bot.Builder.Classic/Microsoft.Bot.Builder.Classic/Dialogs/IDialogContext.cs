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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    /// <summary>
    /// Encapsulates a method that represents the code to execute after a result is available.
    /// </summary>
    /// <remarks>
    /// The result is often a message from the user.
    /// </remarks>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="context">The dialog context.</param>
    /// <param name="result">The result.</param>
    /// <returns>A task that represents the code that will resume after the result is available.</returns>
    public delegate Task ResumeAfter<in T>(IDialogContext context, IAwaitable<T> result);

    /// <summary>
    /// Encapsulate a method that represents the code to start a dialog.
    /// </summary>
    /// <param name="context">The dialog context.</param>
    /// <returns>A task that represents the start code for a dialog.</returns>
    public delegate Task StartAsync(IDialogContext context);

    /// <summary>
    /// The context for the bot.
    /// </summary>
    public interface IBotContext : IBotData, IBotToUser
    {
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// The activity posted to bot.
        /// </summary>
        /// <remarks> This is the incoming activity in reactive cases.
        /// for proactive case, i.e. Conversation.ResumeAsync code path,
        /// it will be the <see cref="IMessageActivity"/> returned by <see cref="ConversationReference.GetContinuationActivity"/>.
        /// </remarks>
        IActivity Activity { get; }
    }

    /// <summary>
    /// The context for the execution of a dialog's conversational process.
    /// </summary>
    public interface IDialogContext : IDialogStack, IBotContext
    {
    }

    /// <summary>
    /// Optional message properties that can be sent <see cref="Extensions.SayAsync(IBotToUser, string, string, MessageOptions, string, CancellationToken)"/>
    /// </summary>
    public class MessageOptions
    {
        public MessageOptions()
        {
            this.TextFormat = TextFormatTypes.Markdown;
            this.AttachmentLayout = AttachmentLayoutTypes.List;
            this.Attachments = new List<Attachment>();
            this.Entities = new List<Microsoft.Bot.Schema.Entity>();
        }

        /// <summary>
        /// Indicates whether the bot is accepting, expecting, or ignoring input
        /// </summary>
        public string InputHint { get; set; }

        /// <summary>
        /// Format of text fields [plain|markdown] Default:markdown
        /// </summary>
        public string TextFormat { get; set; }

        /// <summary>
        /// Hint for how to deal with multiple attachments: [list|carousel] Default:list
        /// </summary>
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Attachments
        /// </summary>
        public IList<Attachment> Attachments { get; set; }

        /// <summary>
        /// Collection of Entity objects, each of which contains metadata about this activity. Each Entity object is typed.
        /// </summary>
        public IList<Microsoft.Bot.Schema.Entity> Entities { get; set; }
    }

    /// <summary>
    /// Helper methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Post a message to be sent to the user, using previous messages to establish a conversation context.
        /// </summary>
        /// <remarks>
        /// If the locale parameter is not set, locale of the incoming message will be used for reply.
        /// </remarks>
        /// <param name="botToUser">Communication channel to use.</param>
        /// <param name="text">The message text.</param>
        /// <param name="locale">The locale of the text.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        public static async Task PostAsync(this IBotToUser botToUser, string text, string locale = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = botToUser.MakeMessage();
            message.Text = text;

            if (!string.IsNullOrEmpty(locale))
            {
                message.Locale = locale;
            }

            await botToUser.PostAsync(message, cancellationToken);
        }


        /// <summary>
        /// Post a message and optional SSML to be sent to the user, using previous messages to establish a conversation context.
        /// </summary>
        /// <remarks>
        /// If the locale parameter is not set, locale of the incoming message will be used for reply.
        /// </remarks>
        /// <param name="botToUser">Communication channel to use.</param>
        /// <param name="text">The message text.</param>
        /// <param name="speak">The SSML markup for text to speech.</param>
        /// <param name="options">The options for the message.</param>
        /// <param name="locale">The locale of the text.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        public static async Task SayAsync(this IBotToUser botToUser, string text, string speak = null, MessageOptions options = null, string locale = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = botToUser.MakeMessage();

            message.Text = text;
            message.Speak = speak;

            if (!string.IsNullOrEmpty(locale))
            {
                message.Locale = locale;
            }

            if (options != null)
            {
                message.InputHint = options.InputHint;
                message.TextFormat = options.TextFormat;
                message.AttachmentLayout = options.AttachmentLayout;
                message.Attachments = options.Attachments;
                message.Entities = options.Entities;
            }

            await botToUser.PostAsync(message, cancellationToken);
        }

        /// <summary>
        /// Suspend the current dialog until the user has sent a message to the bot.
        /// </summary>
        /// <param name="stack">The dialog stack.</param>
        /// <param name="resume">The method to resume when the message has been received.</param>
        public static void Wait(this IDialogStack stack, ResumeAfter<IMessageActivity> resume)
        {
            stack.Wait<IMessageActivity>(resume);
        }

        /// <summary>
        /// Call a child dialog, add it to the top of the stack and post the message to the child dialog.
        /// </summary>
        /// <typeparam name="R">The type of result expected from the child dialog.</typeparam>
        /// <param name="stack">The dialog stack.</param>
        /// <param name="child">The child dialog.</param>
        /// <param name="resume">The method to resume when the child dialog has completed.</param>
        /// <param name="message">The message that will be posted to child dialog.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing the Forward operation.</returns>
        public static async Task Forward<R>(this IDialogStack stack, IDialog<R> child, ResumeAfter<R> resume, IMessageActivity message, CancellationToken token = default(CancellationToken))
        {
            await stack.Forward<R, IMessageActivity>(child, resume, message, token);
        }
    }
}