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

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    /// <summary>
    /// A <see cref="IDialog{TResult}"/> is a suspendable conversational process that produces a result of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <remarks>
    /// Dialogs can call child dialogs or send messages to a user.
    /// Dialogs are suspended when waiting for a message from the user to the bot.
    /// Dialogs are resumed when the bot receives a message from the user.
    /// </remarks>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IDialog<out TResult>
    {
        /// <summary>
        /// The start of the code that represents the conversational dialog.
        /// </summary>
        /// <param name="context">The dialog context.</param>
        /// <returns>A task that represents the dialog start.</returns>
        Task StartAsync(IDialogContext context);
    }

    /// <summary>
    /// A <see cref="IDialog"/> is a suspendable conversational process that produces an ignored result.
    /// </summary>
    public interface IDialog : IDialog<object>
    {
    }
}
