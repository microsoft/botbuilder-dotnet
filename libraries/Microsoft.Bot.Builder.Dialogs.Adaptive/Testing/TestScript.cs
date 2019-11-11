// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// A mock Test Script that can be used for unit testing of bot logic.
    /// </summary>
    /// <remarks>You can use this class to mimic input from a a user or a channel to validate
    /// that the bot or adapter responds as expected.</remarks>
    /// <seealso cref="TestAdapter"/>
    public class TestScript
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestScript"/> class.
        /// </summary>
        /// <param name="callback">The bot turn processing logic to test.</param>
        /// <param name="adapter">The optional test adapter to use.</param>
        /// <remarks>If adapter is not provided a standard test adapter with all services will be registered.</remarks>
        public TestScript()
        {
        }

        /// <summary>
        /// Gets or sets the description property.
        /// </summary>
        /// <value>
        /// A description of the test sequence.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the RootDialog.
        /// </summary>
        /// <value>
        /// The dialog to use for the root dialog.
        /// </value>
        public Dialog Dialog { get; set; }

        /// <summary>
        /// Gets or sets the test script actions.
        /// </summary>
        /// <value>
        /// The sequence of test actions to perform to validate the dialog behavior.
        /// </value>
        public List<TestAction> Script { get; set; } = new List<TestAction>();

        /// <summary>
        /// Starts the execution of the test sequence.
        /// </summary>
        /// <remarks>This methods sends the activities from the user to the bot and
        /// checks the responses from the bot based on the TestActions.</remarks>
        /// <param name="resourceExplorer">resource explorer.</param>
        /// <param name="callback">bot logic.</param>
        /// <param name="adapter">optional test adapter.</param>
        /// <returns>Runs the exchange between the user and the bot.</returns>
        public async Task ExecuteAsync(ResourceExplorer resourceExplorer, BotCallbackHandler callback = null, TestAdapter adapter = null)
        {
            if (adapter == null)
            {
                TypeFactory.Configuration = new ConfigurationBuilder().Build();
                var storage = new MemoryStorage();
                var convoState = new ConversationState(storage);
                var userState = new UserState(storage);
                adapter = (TestAdapter)new TestAdapter(sendTraceActivity: false)
                    .UseStorage(storage)
                    .UseState(userState, convoState)
                    .Use(new AutoSaveStateMiddleware())
                    .UseResourceExplorer(resourceExplorer)
                    .UseAdaptiveDialogs()
                    .UseLanguageGeneration(resourceExplorer)
                    .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));
            }

            DialogManager dm = new DialogManager(this.Dialog);
            foreach (var testAction in this.Script)
            {
                if (callback != null)
                {
                    await testAction.ExecuteAsync(adapter, callback).ConfigureAwait(false);
                }
                else
                {
                    await testAction.ExecuteAsync(adapter, dm.OnTurnAsync).ConfigureAwait(false);
                }
            }
        }
    }
}
