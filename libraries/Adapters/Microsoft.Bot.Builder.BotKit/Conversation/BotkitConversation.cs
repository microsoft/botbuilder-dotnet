// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.BotKit.Core;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.BotKit.Conversation
{
    /// <summary>
    /// Data related to the BotkitConversation.
    /// </summary>
    public class BotkitConversation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotkitConversation"/> class.
        /// </summary>
        /// <param name="dialogId">A unique identifier for this dialog, used to later trigger this dialog.</param>
        /// <param name="botkit">A pointer to the main Botkit controller.</param>
        public BotkitConversation(string dialogId, Botkit botkit)
        {
        }

        /// <summary>
        /// Add a non-interactive message to the default thread.
        /// Messages added with `say()` and `addMessage()` will _not_ wait for a response, will be sent one after another without a pause.
        /// </summary>
        /// <param name="messageTemplate">Message template to be sent.</param>
        /// <returns><see cref="BotkitConversation"/> object.</returns>
        public BotkitConversation Say(IBotkitMessageTemplate messageTemplate)
        {
            return this;
        }

        /// <summary>
        /// An action to the conversation timeline. This can be used to go to switch threads or end the dialog.
        /// When provided the name of another thread in the conversation, this will cause the bot to go immediately
        /// to that thread.
        /// Otherwise, use one of the following keywords: stop, repeat, complete, timeout.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="threadName">The thread name.</param>
        /// <returns><see cref="BotkitConversation"/> object.</returns>
        public BotkitConversation AddAction(string action, string threadName)
        {
            return this;
        }

        /// <summary>
        /// Cause the dialog to call a child dialog, wait for it to complete,
        /// then store the results in a variable and resume the parent dialog.
        /// Use this to combine multiple dialogs into bigger interactions.
        /// </summary>
        /// <param name="dialogId">The id of another dialog.</param>
        /// <param name="keyName">The variable name in which to store the results of the child dialog. if not provided, defaults to dialog_id.</param>
        /// <param name="threadName">The name of a thread to which this call should be added. defaults to 'default'.</param>
        /// <returns><see cref="BotkitConversation"/> object.</returns>
        public BotkitConversation AddChildDialog(string dialogId, string keyName, string threadName)
        {
            return this;
        }

        /// <summary>
        /// Cause the current dialog to handoff to another dialog.
        /// The parent dialog will not resume when the child dialog completes.
        /// However, the afterDialog event will not fire for the parent dialog until all child dialogs complete.
        /// Use this to combine multiple dialogs into bigger interactions.
        /// </summary>
        /// <param name="dialogId">Dialog Id.</param>
        /// <param name="threadName">Thread Name.</param>
        /// <returns><see cref="BotkitConversation"/> object.</returns>
        public BotkitConversation AddGotoDialog(string dialogId, string threadName)
        {
            return this;
        }

        /// <summary>
        /// Add a message template to a specific thread.
        /// Messages added with say() and addMessage() will be sent one after another without a pause.
        /// </summary>
        /// <param name="message">Message template to be sent.</param>
        /// <param name="threadName">Name of thread to which message will be added.</param>
        /// <returns><see cref="BotkitConversation"/> object.</returns>
        public BotkitConversation AddMessage(IBotkitMessageTemplate message, string threadName)
        {
            return this;
        }

        /// <summary>
        /// Add a question to the default thread.
        /// In addition to a message template, receives either a single handler function to call when an answer is provided,
        /// or an array of handlers paired with trigger patterns.When providing multiple conditions to test, developers may also provide a
        /// handler marked as the default choice.
        /// </summary>
        /// <param name="message">A message that will be used as the prompt.</param>
        /// <param name="handlers">One or more handler functions defining possible conditional actions based on the response to the question.</param>
        /// <param name="key">Name of variable to store response in.</param>
        /// <returns><see cref="BotkitConversation"/> object.</returns>
        public BotkitConversation Ask(IBotkitMessageTemplate message, IBotkitConvoTrigger handlers, string key)
        {
            return this;
        }

        /// <summary>
        /// Identical to ask(), but accepts the name of a thread to which the question is added.
        /// </summary>
        /// <param name="message">A message that will be used as the prompt.</param>
        /// <param name="handlers">One or more handler functions defining possible conditional actions based on the response to the question.</param>
        /// <param name="key">Name of variable to store response in.</param>
        /// <param name="threadName">Name of thread to which message will be added.</param>
        /// <returns><see cref="BotkitConversation"/> object.</returns>
        public BotkitConversation AddQuestion(IBotkitMessageTemplate message, IBotkitConvoTrigger handlers, string key, string threadName)
        {
            return this;
        }

        /// <summary>
        /// Register a handler function that will fire before a given thread begins.
        /// Use this hook to set variables, call APIs, or change the flow of the conversation using `convo.gotoThread`.
        /// </summary>
        /// <param name="threadName">A valid thread defined in this conversation.</param>
        /// <param name="obj">A handler function in the form async(convo, bot) => { ... }.</param>
        public void Before(string threadName, object obj)
        {
            // TO-DO: review method firm for matching this:
            // public void Before(string threadName, Func<BotkitDialogWrapper, BotWorker, object>)
        }

        /// <summary>
        /// This private method is called before a thread begins, and causes any bound handler functions to be executed.
        /// </summary>
        /// <param name="threadName">The thread about to begin.</param>
        /// <param name="dialogContext">The current DialogContext.</param>
        /// <param name="step">The current step object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunBefore(string threadName, DialogContext dialogContext, IBotkitConversationStep step)
        {
            await Task.FromException(new NotImplementedException());
        }

        /// <summary>
        /// Bind a function to run after the dialog has completed.
        /// The first parameter to the handler will include a hash of all variables set and values collected from the user during the conversation.
        /// The second parameter to the handler is a BotWorker object that can be used to start new dialogs or take other actions.
        /// </summary>
        /// <param name="handler">In the form async(results, bot) { ... }.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task After(Action<object, BotWorker> handler)
        {
            await Task.FromException(new NotImplementedException());
        }

        /// <summary>
        /// This private method is called at the end of the conversation, and causes any bound handler functions to be executed.
        /// </summary>
        /// <param name="context">The current dialog context.</param>
        /// <param name="any">An object containing the final results of the dialog.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunAfter(DialogContext context, object any)
        {
            await Task.FromException(new NotImplementedException());
        }

        /// <summary>
        /// Bind a function to run whenever a user answers a specific question.  Can be used to validate input and take conditional actions.
        /// </summary>
        /// <param name="variable">Name of the variable to watch for changes.</param>
        /// <param name="handler">A handler function that will fire whenever a user's response is used to change the value of the watched variable.</param>
        public void OnChange(string variable, Func<object, BotkitConversation, BotWorker, object> handler)
        {
        }

        /// <summary>
        /// Called automatically when a dialog begins. Do not call this directly!.
        /// </summary>
        /// <param name="dialogContext">The current DialogContext.</param>
        /// <param name="options">An object containing initialization parameters passed to the dialog. may include `thread` which will cause the dialog to begin with that thread instead of the `default` thread.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> BeginDialog(DialogContext dialogContext, object options)
        {
             return await Task.FromException<object>(new NotImplementedException());
        }

        /// <summary>
        /// Called automatically when an already active dialog is continued. Do not call this directly!.
        /// </summary>
        /// <param name="dialogContext">The current DialogContext.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> ContinueDialog(DialogContext dialogContext)
        {
            return await Task.FromException<object>(new NotImplementedException());
        }

        /// <summary>
        /// Called automatically when a dialog moves forward a step. Do not call this directly!.
        /// </summary>
        /// <param name="dialogContext">The current DialogContext.</param>
        /// <param name="reason">Reason for resuming the dialog.</param>
        /// <param name="result">Result of previous step.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> ResumeDialog(DialogContext dialogContext, string reason, object result)
        {
            return await Task.FromException<object>(new NotImplementedException());
        }

        /// <summary>
        /// Automatically called when the dialog ends and causes any handlers bound using `after()` to fire. Do not call this directly!.
        /// </summary>
        /// <param name="dialogContext">The current DialogContext.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnStatus> End(DialogContext dialogContext)
        {
            await Task.Yield();
            return DialogTurnStatus.Cancelled;
        }
    }
}
