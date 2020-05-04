// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.Script";

        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        private static IConfiguration defaultConfiguration = new ConfigurationBuilder().Build();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestScript"/> class.
        /// </summary>
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
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the RootDialog.
        /// </summary>
        /// <value>
        /// The dialog to use for the root dialog.
        /// </value>
        [JsonProperty("dialog")]
        public Dialog Dialog { get; set; }

        /// <summary>
        /// Gets or sets the locale.
        /// </summary>
        /// <value>the locale (Default:en-us).</value>
        [JsonProperty("locale")]
        public string Locale { get; set; } = "en-us";

        /// <summary>
        /// Gets or sets the test script actions.
        /// </summary>
        /// <value>
        /// The sequence of test actions to perform to validate the dialog behavior.
        /// </value>
        [JsonProperty("script")]
        public List<TestAction> Script { get; set; } = new List<TestAction>();

        /// <summary>
        /// Gets or sets a value indicating whether trace activities should be passed to the test script.
        /// </summary>
        /// <value>If true then trace activities will be sent to the test script.</value>
        [JsonProperty("enableTrace")]
        public bool EnableTrace { get; set; } = false;

        /// <summary>
        /// Build default test adapter.
        /// </summary>
        /// <param name="resourceExplorer">Resource explorer to use.</param>
        /// <param name="testName">Name of test.</param>
        /// <returns>Test adapter.</returns>
        public TestAdapter DefaultTestAdapter(ResourceExplorer resourceExplorer, [CallerMemberName] string testName = null)
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
           
            var adapter = (TestAdapter)new TestAdapter(TestAdapter.CreateConversation(testName))
                .UseStorage(storage)
                .UseBotState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));
            
            adapter.OnTurnError += (context, err) => context.SendActivityAsync(err.Message);
            return adapter;
        }

        /// <summary>
        /// Starts the execution of the test sequence.
        /// </summary>
        /// <remarks>This methods sends the activities from the user to the bot and
        /// checks the responses from the bot based on the TestActions.</remarks>
        /// <param name="resourceExplorer">The resource explorer to use.</param>
        /// <param name="testName">Name of the test.</param>
        /// <param name="callback">The bot logic.</param>
        /// <param name="adapter">optional test adapter.</param>
        /// <returns>Runs the exchange between the user and the bot.</returns>
        public async Task ExecuteAsync(ResourceExplorer resourceExplorer, [CallerMemberName] string testName = null, BotCallbackHandler callback = null, TestAdapter adapter = null)
        {
            if (adapter == null)
            {
                adapter = DefaultTestAdapter(resourceExplorer, testName);
            }

            adapter.EnableTrace = this.EnableTrace;
            adapter.Locale = this.Locale;

            if (callback != null)
            {
                foreach (var testAction in this.Script)
                {
                    await testAction.ExecuteAsync(adapter, callback).ConfigureAwait(false);
                }
            }
            else
            {
                var dm = new DialogManager(this.Dialog)
                    .UseResourceExplorer(resourceExplorer)
                    .UseLanguageGeneration();

                foreach (var testAction in this.Script)
                {
                    await testAction.ExecuteAsync(adapter, dm.OnTurnAsync).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Adds a message activity from the user to the bot.
        /// </summary>
        /// <param name="userSays">The text of the message to send.</param>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        /// <returns>A new <see cref="TestScript"/> object that appends a new message activity from the user to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestScript"/> object.</remarks>
        public TestScript Send(string userSays, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            this.Script.Add(new UserSays(path: path, line: line) { Text = userSays });
            return this;
        }

        /// <summary>
        /// Adds an activity from the user to the bot.
        /// </summary>
        /// <param name="userActivity">The activity to send.</param>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        /// <returns>A new <see cref="TestScript"/> object that appends a new activity from the user to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestScript"/> object.</remarks>
        public TestScript Send(IActivity userActivity, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            this.Script.Add(new UserActivity(path: path, line: line) { Activity = (Activity)userActivity });
            return this;
        }

        public TestScript SendConversationUpdate([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            this.Script.Add(new UserConversationUpdate(path: path, line: line));
            return this;
        }

        /// <summary>
        /// Adds a delay in the conversation.
        /// </summary>
        /// <param name="ms">The delay length in milliseconds.</param>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        /// <returns>A new <see cref="TestScript"/> object that appends a delay to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestScript"/> object.</remarks>
        public TestScript Delay(uint ms, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            this.Script.Add(new UserDelay(path: path, line: line) { Timespan = ms });
            return this;
        }

        /// <summary>
        /// Adds a delay in the conversation.
        /// </summary>
        /// <param name="timespan">The delay length in TimeSpan.</param>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        /// <returns>A new <see cref="TestScript"/> object that appends a delay to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestScript"/> object.</remarks>
        public TestScript Delay(TimeSpan timespan, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            this.Script.Add(new UserDelay(path: path, line: line) { Timespan = (uint)timespan.TotalMilliseconds });
            return this;
        }

        /// <summary>
        /// Adds an assertion that the turn processing logic responds as expected.
        /// </summary>
        /// <param name="expected">The expected text of a message from the bot.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <param name="assertions">assertions.</param>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        /// <returns>A new <see cref="TestScript"/> object that appends this assertion to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestScript"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestScript AssertReply(string expected, string description = null, uint timeout = 3000, string[] assertions = null, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            var action = new AssertReply(path: path, line: line) { Text = expected, Description = description, Timeout = timeout, Exact = true };
            if (assertions != null)
            {
                action.Assertions.AddRange(assertions);
            }

            this.Script.Add(action);
            return this;
        }

        /// <summary>
        /// Adds an assertion that the turn processing logic responds as expected.
        /// </summary>
        /// <param name="assertions">assertions.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        /// <returns>A new <see cref="TestScript"/> object that appends this assertion to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestScript"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestScript AssertReplyActivity(string[] assertions, string description = null, uint timeout = 3000, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            var action = new AssertReplyActivity(path: path, line: line) { Description = description, Timeout = timeout };
            if (assertions != null)
            {
                action.Assertions.AddRange(assertions);
            }

            this.Script.Add(action);
            return this;
        }

        /// <summary>
        /// Adds an assertion that the turn processing logic responds as expected.
        /// </summary>
        /// <param name="expected">The part of the expected text of a message from the bot.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        /// <returns>A new <see cref="TestScript"/> object that appends this assertion to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestScript"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestScript AssertReplyContains(string expected, string description = null, uint timeout = 3000, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            this.Script.Add(new AssertReply(path: path, line: line) { Text = expected, Description = description, Timeout = timeout, Exact = false });
            return this;
        }

        /// <summary>
        /// Shortcut for calling <see cref="Send(string, string, int)"/> followed by <see cref="AssertReply(string, string, uint, string[], string, int)"/>.
        /// </summary>
        /// <param name="userSays">The text of the message to send.</param>
        /// <param name="expected">The expected text of a message from the bot.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        /// <returns>A new <see cref="TestScript"/> object that appends this exchange to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestScript"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestScript Test(string userSays, string expected, string description = null, uint timeout = 3000, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            this.Script.Add(new UserSays(path: path, line: line) { Text = userSays });
            this.Script.Add(new AssertReply(path: path, line: line) { Text = expected, Description = description, Timeout = timeout, Exact = true });
            return this;
        }

        /// <summary>
        /// Adds an assertion that the bot's response is contained within a set of acceptable responses.
        /// </summary>
        /// <param name="candidates">The set of acceptable messages.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        /// <returns>A new <see cref="TestScript"/> object that appends this assertion to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestScript"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestScript AssertReplyOneOf(string[] candidates, string description = null, uint timeout = 3000, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            this.Script.Add(new AssertReplyOneOf(path: path, line: line) { Text = candidates.ToList<string>(), Description = description, Timeout = timeout, Exact = true });
            return this;
        }

#if SAVESCRIPT
        public Task SaveScriptAsync(string folder, [CallerMemberName] string testName = null)
        {
            folder = Path.Combine(GetProjectPath(), PathUtils.NormalizePath($"tests\\{folder}"));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (this.Dialog is DialogContainer container && container.Dialogs.GetDialogs().Any())
            {
                folder = Path.Combine(folder, testName);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                SaveContainerDialogs(container, folder);
            }

            File.WriteAllText(Path.Combine(folder, $"{testName}.test.dialog"), JsonConvert.SerializeObject(this, serializerSettings));
            return Task.CompletedTask;
        }

        private static string GetProjectPath()
        {
            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }
                else
                {
                    parent = Path.GetDirectoryName(parent);
                }
            }

            return parent;
        }

        private void SaveContainerDialogs(DialogContainer container, string folder)
        {
            foreach (var dialog in container.Dialogs.GetDialogs())
            {
                var filePath = Path.GetFullPath(Path.Combine(folder, $"{dialog.Id}.dialog"));
                File.WriteAllText(filePath, JsonConvert.SerializeObject(dialog, serializerSettings));

                if (dialog is DialogContainer container2)
                {
                    SaveContainerDialogs(container2, folder);
                }
            }
        }
#endif

        internal class IgnoreEmptyEnumerablesResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                {
                    property.ShouldSerialize = instance =>
                    {
                        var enumer = instance
                            .GetType()
                            .GetProperty(member.Name)
                            .GetValue(instance, null) as IEnumerable;

                        if (enumer != null)
                        {
                            // check to see if there is at least one item in the Enumerable
                            return enumer.GetEnumerator().MoveNext();
                        }
                        else
                        {
                            // if the enumerable is null, we defer the decision to NullValueHandling
                            return true;
                        }
                    };
                }

                return property;
            }
        }
    }
}
