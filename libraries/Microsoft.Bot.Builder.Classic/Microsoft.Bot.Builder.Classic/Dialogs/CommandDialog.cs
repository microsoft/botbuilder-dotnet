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

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    #region Documentation
    /// <summary> Dialog that dispatches based on a regex matching input. </summary>
    #endregion
    [Serializable]
    public class CommandDialog<T> : IDialog<T>
    {
        #region Documentation
        /// <summary>   A single command. </summary>
        #endregion
        [Serializable]
        public class Command
        {
            #region Documentation
            /// <summary>   Gets or sets the command ID used for persisting currently running command handler. </summary>
            /// <value> Command ID. </value>
            #endregion
            public string CommandId { set; get; }

            #region Documentation
            /// <summary>   Gets or sets the regular expression for matching command. </summary>
            /// <value> The regular expression. </value>
            #endregion
            public Regex Expression { set; get; }

            #region Documentation
            /// <summary>   Gets or sets the command handler. </summary>
            /// <value> The command handler. </value>
            #endregion
            public ResumeAfter<IMessageActivity> CommandHandler { set; get; }
        }

        private Command defaultCommand;
        private readonly List<Command> commands = new List<Command>();
        private readonly Dictionary<string, Delegate> resultHandlers = new Dictionary<string, Delegate>();

        async Task IDialog<T>.StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceived);
        }

        public virtual async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var text = (await message).Text;
            Command matched = null;
            for (int idx = 0; idx < commands.Count; idx++)
            {
                var handler = commands[idx];
                if (handler.Expression.Match(text).Success)
                {
                    matched = handler;
                    break;
                }
            }

            if (matched == null && this.defaultCommand != null)
            {
                matched = this.defaultCommand;
            }

            if (matched != null)
            {
                context.PrivateConversationData.SetValue("ActiveCommandId", matched.CommandId);
                await matched.CommandHandler(context, message);
            }
            else
            {
                string error = $"CommandDialog doesn't have a registered command handler for this message: {text}";
                throw new InvalidOperationException(error);
            }
        }

        #region Documentation
        /// <summary>
        /// The result handler of the command dialog passed to the child dialogs. 
        /// </summary>
        /// <typeparam name="U"> The type of the result returned by the child dialog. </typeparam>
        /// <param name="context"> Dialog context. </param>
        /// <param name="result"> The result retured by the child dialog. </param>
        #endregion
        public virtual async Task ResultHandler<U>(IDialogContext context, IAwaitable<U> result)
        {
            Delegate handler;
            string commandId;
            if (context.PrivateConversationData.TryGetValue("ActiveCommandId", out commandId) && resultHandlers.TryGetValue(commandId, out handler))
            {
                await ((ResumeAfter<U>)handler).Invoke(context, result);
                context.Wait(MessageReceived);
            }
            else
            {
                string error = $"CommandDialog doesn't have a registered result handler for this type: {typeof(U)}";
                throw new InvalidOperationException(error);
            }
        }

        #region Documentation
        /// <summary> Define a handler that is fired on a regular expression match of a message. </summary>
        /// <typeparam name="U"> Type of input to result handler. </typeparam>
        /// <param name="expression"> Regular expression to match. </param>
        /// <param name="handler"> Handler to call on match. </param>
        /// <param name="resultHandler"> Optional result handler to be called if handler is creating a chaild dialog. </param>
        /// <returns> A commandDialog. </returns>
        #endregion
        public CommandDialog<T> On<U>(Regex expression, ResumeAfter<IMessageActivity> handler, ResumeAfter<U> resultHandler = null)
        {
            var command = new Command
            {
                CommandId = ComputeHash(expression.ToString()),
                Expression = expression,
                CommandHandler = handler,
            };
            commands.Add(command);
            RegisterResultHandler(command, resultHandler);

            return this;
        }

        #region Documentation
        /// <summary> Define the default action if no match. </summary>
        /// <typeparam name="U"> Type of input to result handler. </typeparam>
        /// <param name="handler"> Handler to call if no match. </param>
        /// <param name="resultHandler"> Optional result handler to be called if handler is creating a chaild dialog. </param>
        /// <returns> A CommandDialog. </returns>
        #endregion
        public CommandDialog<T> OnDefault<U>(ResumeAfter<IMessageActivity> handler, ResumeAfter<U> resultHandler = null)
        {
            var command = new Command { CommandId = "defaultResultHandler", CommandHandler = handler };
            this.defaultCommand = command;
            RegisterResultHandler(command, resultHandler);

            return this;
        }

        private void RegisterResultHandler<U>(Command command, ResumeAfter<U> resultHandler)
        {
            if (resultHandler != null)
            {
                resultHandlers.Add(command.CommandId, resultHandler);
            }
        }

        private string ComputeHash(string str)
        {
            var algorithm = SHA1.Create();
            return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(str)));
        }
    }
}
