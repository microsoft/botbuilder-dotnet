// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.BotKit.Core;

namespace Microsoft.BotKit.CMS
{
    /// <summary>
    /// Data related to the BotkitCMSHelper.
    /// </summary>
    public class BotkitCMSHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotkitCMSHelper"/> class.
        /// </summary>
        /// <param name="botkit">botkit for the Dialog.</param>
        /// <param name="configuration">configuration id for the Dialog.</param>
        public BotkitCMSHelper(Botkit botkit, CMSConfiguration configuration)
        {
        }

        /// <summary>
        /// Load all script content from the configured CMS instance into a DialogSet and prepare them to be used.
        /// </summary>
        /// <param name="dialogSet">A DialogSet into which the dialogs should be loaded.  In most cases, this is `controller.dialogSet`, allowing Botkit to access these dialogs through `bot.beginDialog()`.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadAllScripts(DialogSet dialogSet)
        {
            await Task.FromException(new NotImplementedException());
        }

        /// <summary>
        /// Uses the Botkit CMS trigger API to test an incoming message against a list of predefined triggers.
        /// If a trigger is matched, the appropriate dialog will begin immediately.
        /// </summary>
        /// <param name="bot">The current bot worker instance.</param>
        /// <param name="message">An incoming message to be interpreted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task TestTrigger(BotWorker bot, IBotkitMessage message)
        {
            await Task.FromException(new NotImplementedException());
        }

        /// <summary>
        /// Bind a handler function that will fire before a given script and thread begin.
        /// Provides a way to use BotkitConversation.before() on dialogs loaded dynamically via the CMS api instead of being created in code.
        /// </summary>
        /// <param name="scriptName">The name of the script to bind to.</param>
        /// <param name="threadName">The name of a thread within the script to bind to.</param>
        /// <param name="handler">A handler function in the form async(convo, bot) => {}.</param>
        public void Before(string scriptName, string threadName, Action<BotkitDialogWrapper, BotWorker> handler)
        {
            // TO-DO: the method firm must match this:
            // public before(script_name: string, thread_name: string, handler: (convo: BotkitDialogWrapper, bot: BotWorker) => Promise<void>): void {
        }

        /// <summary>
        /// Bind a handler function that will fire when a given variable is set within a a given script.
        /// Provides a way to use BotkitConversation.onChange() on dialogs loaded dynamically via the CMS api instead of being created in code.
        /// </summary>
        /// <param name="scriptName">The name of the script to bind to.</param>
        /// <param name="variableName">The name of a variable within the script to bind to.</param>
        /// <param name="handler">A handler function in the form async(value, convo, bot) => {}.</param>
        public void OnChange(string scriptName, string variableName, Action<object, BotWorker> handler)
        {
            // TO-DO: Method name must match this:
            // public onChange(script_name: string, variable_name: string, handler: (value: any, convo: BotkitDialogWrapper, bot: BotWorker) => Promise<void>): void {
        }

        /// <summary>
        /// Bind a handler function that will fire after a given dialog ends.
        /// Provides a way to use BotkitConversation.after() on dialogs loaded dynamically via the CMS api instead of being created in code.
        /// </summary>
        /// <param name="scriptName">The name of the script to bind to.</param>
        /// <param name="handler">A handler function in the form async(results, bot) => {}.</param>
        public void After(string scriptName, Action<object, BotWorker> handler)
        {
            // TO-DO: method name must match this:
            // public after(script_name: string, handler: (results: any, bot: BotWorker) => Promise<void>): void {
        }
    }
}
