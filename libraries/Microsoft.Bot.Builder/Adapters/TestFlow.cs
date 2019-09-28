// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters
{
    /// <summary>
    /// Represents a method the <see cref="TestFlow"/> can call to validate an activity.
    /// </summary>
    /// <param name="expected">The expected activity from the bot or adapter.</param>
    /// <param name="actual">The actual activity from the bot or adapter.</param>
    public delegate void ValidateReply(IActivity expected, IActivity actual);

    /// <summary>
    /// A mock channel that can be used for unit testing of bot logic.
    /// </summary>
    /// <remarks>You can use this class to mimic input from a a user or a channel to validate
    /// that the bot or adapter responds as expected.</remarks>
    /// <seealso cref="TestAdapter"/>
    public class TestFlow
    {
        private readonly TestAdapter _adapter;
        private readonly Task _testTask;
        private BotCallbackHandler _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFlow"/> class.
        /// </summary>
        /// <param name="adapter">The test adapter to use.</param>
        /// <param name="callback">The bot turn processing logic to test.</param>
        public TestFlow(TestAdapter adapter, BotCallbackHandler callback = null)
        {
            _adapter = adapter;
            _callback = callback;
            _testTask = _testTask ?? Task.CompletedTask;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFlow"/> class from an existing flow.
        /// </summary>
        /// <param name="testTask">The exchange to add to the exchanges in the existing flow.</param>
        /// <param name="flow">The flow to build up from. This provides the test adapter to use,
        /// the bot turn processing locig to test, and a set of exchanges to model and test.</param>
        public TestFlow(Task testTask, TestFlow flow)
        {
            _testTask = testTask ?? Task.CompletedTask;
            _callback = flow._callback;
            _adapter = flow._adapter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFlow"/> class.
        /// </summary>
        /// <param name="adapter">The test adapter to use.</param>
        /// <param name="bot">The bot containing the turn processing logic to test.</param>
        public TestFlow(TestAdapter adapter, IBot bot)
            : this(adapter, bot.OnTurnAsync)
        {
        }

        /// <summary>
        /// Starts the execution of the test flow.
        /// </summary>
        /// <returns>Runs the exchange between the user and the bot.</returns>
        /// <remarks>This methods sends the activities from the user to the bot and
        /// checks the responses from the bot based on the activities described in the
        /// current test flow.</remarks>
        public Task StartTestAsync() => _testTask;

        /// <summary>
        /// Adds a message activity from the user to the bot.
        /// </summary>
        /// <param name="userSays">The text of the message to send.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends a new message activity from the user to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        public TestFlow Send(string userSays)
        {
            if (userSays == null)
            {
                throw new ArgumentNullException("You have to pass a userSays parameter");
            }

            return new TestFlow(
                _testTask.ContinueWith((task) =>
                {
                    // NOTE: we need to .Wait() on the original Task to properly observe any exceptions that might have occurred
                    // and to have them propagate correctly up through the chain to whomever is waiting on the parent task
                    // The following StackOverflow answer provides some more details on why you want to do this:
                    // https://stackoverflow.com/questions/11904821/proper-way-to-use-continuewith-for-tasks/11906865#11906865
                    //
                    // From the Docs:
                    //  https://docs.microsoft.com/dotnet/standard/parallel-programming/exception-handling-task-parallel-library
                    //  Exceptions are propagated when you use one of the static or instance Task.Wait or Wait
                    //  methods, and you handle them by enclosing the call in a try/catch statement. If a task is the
                    //  parent of attached child tasks, or if you are waiting on multiple tasks, multiple exceptions
                    //  could be thrown.
                    task.Wait();

                    return _adapter.SendTextToBotAsync(userSays, _callback, default(CancellationToken));
                }).Unwrap(),
                this);
        }

        public TestFlow SendConversationUpdate()
        {
            return new TestFlow(
                _testTask.ContinueWith((task) =>
                {
                    // NOTE: we need to .Wait() on the original Task to properly observe any exceptions that might have occurred
                    // and to have them propagate correctly up through the chain to whomever is waiting on the parent task
                    // The following StackOverflow answer provides some more details on why you want to do this:
                    // https://stackoverflow.com/questions/11904821/proper-way-to-use-continuewith-for-tasks/11906865#11906865
                    //
                    // From the Docs:
                    //  https://docs.microsoft.com/dotnet/standard/parallel-programming/exception-handling-task-parallel-library
                    //  Exceptions are propagated when you use one of the static or instance Task.Wait or Wait
                    //  methods, and you handle them by enclosing the call in a try/catch statement. If a task is the
                    //  parent of attached child tasks, or if you are waiting on multiple tasks, multiple exceptions
                    //  could be thrown.
                    task.Wait();

                    var cu = Activity.CreateConversationUpdateActivity();
                    cu.MembersAdded.Add(this._adapter.Conversation.User);
                    return _adapter.ProcessActivityAsync((Activity)cu, _callback, default(CancellationToken));
                }).Unwrap(),
                this);
        }

        /// <summary>
        /// Adds an activity from the user to the bot.
        /// </summary>
        /// <param name="userActivity">The activity to send.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends a new activity from the user to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        public TestFlow Send(IActivity userActivity)
        {
            if (userActivity == null)
            {
                throw new ArgumentNullException("You have to pass an Activity");
            }

            return new TestFlow(
                _testTask.ContinueWith((task) =>
                {
                    // NOTE: See details code in above method.
                    task.Wait();

                    return _adapter.ProcessActivityAsync((Activity)userActivity, _callback, default(CancellationToken));
                }).Unwrap(),
                this);
        }

        /// <summary>
        /// Adds a delay in the conversation.
        /// </summary>
        /// <param name="ms">The delay length in milliseconds.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends a delay to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        public TestFlow Delay(uint ms)
        {
            return new TestFlow(
                _testTask.ContinueWith((task) =>
                {
                    // NOTE: See details code in above method.
                    task.Wait();

                    return Task.Delay((int)ms);
                }),
                this);
        }

        /// <summary>
        /// Adds an assertion that the turn processing logic responds as expected.
        /// </summary>
        /// <param name="expected">The expected text of a message from the bot.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends this assertion to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestFlow AssertReply(string expected, string description = null, uint timeout = 3000)
        {
            return AssertReply(_adapter.MakeActivity(expected), description ?? expected, timeout);
        }

        /// <summary>
        /// Adds an assertion that the turn processing logic responds as expected.
        /// </summary>
        /// <param name="expected">The expected activity from the bot.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <param name="equalityComparer">The equality parameter which compares two activities.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends this assertion to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestFlow AssertReply(IActivity expected, [CallerMemberName] string description = null, uint timeout = 3000, IEqualityComparer<IActivity> equalityComparer = null)
        {
            return AssertReply(
                (reply) =>
                {
                    description = description ?? expected.AsMessageActivity()?.Text.Trim();
                    if (expected.Type != reply.Type)
                    {
                        throw new Exception($"{description}: Type should match");
                    }

                    if (equalityComparer != null)
                    {
                        if (!equalityComparer.Equals(expected, reply))
                        {
                            throw new Exception($"Expected:{expected}\nReceived:{reply}");
                        }
                    }
                    else
                    {
                        if (expected.AsMessageActivity().Text.Trim() != reply.AsMessageActivity().Text.Trim())
                        {
                            if (description == null)
                            {
                                throw new Exception($"Expected:{expected.AsMessageActivity().Text}\nReceived:{reply.AsMessageActivity().Text}");
                            }
                            else
                            {
                                throw new Exception($"{description}:\nExpected:{expected.AsMessageActivity().Text}\nReceived:{reply.AsMessageActivity().Text}");
                            }
                        }
                    }
                },
                description,
                timeout);
        }

        /// <summary>
        /// Adds an assertion that the turn processing logic responds as expected.
        /// </summary>
        /// <param name="validateActivity">A validation method to apply to an activity from the bot.
        /// This activity should throw an exception if validation wfails.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends this assertion to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        public TestFlow AssertReply(Action<IActivity> validateActivity, [CallerMemberName] string description = null, uint timeout = 3000)
        {
            return new TestFlow(
                _testTask.ContinueWith((task) =>
                {
                    // NOTE: See details code in above method.
                    task.Wait();

                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        timeout = uint.MaxValue;
                    }

                    var start = DateTime.UtcNow;
                    while (true)
                    {
                        var current = DateTime.UtcNow;

                        if ((current - start).TotalMilliseconds > timeout)
                        {
                            throw new TimeoutException($"{timeout}ms Timed out waiting for:'{description}'");
                        }

                        IActivity replyActivity = _adapter.GetNextReply();
                        if (replyActivity != null)
                        {
                            // if we have a reply
                            validateActivity(replyActivity);
                            return;
                        }
                    }
                }),
                this);
        }

        /// <summary>
        /// Shortcut for calling <see cref="Send(string)"/> followed by <see cref="AssertReply(string, string, uint)"/>.
        /// </summary>
        /// <param name="userSays">The text of the message to send.</param>
        /// <param name="expected">The expected text of a message from the bot.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends this exchange to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestFlow Test(string userSays, string expected, string description = null, uint timeout = 3000)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            return Send(userSays)
                .AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Shortcut for calling <see cref="Send(string)"/> followed by <see cref="AssertReply(IActivity, string, uint)"/>.
        /// </summary>
        /// <param name="userSays">The text of the message to send.</param>
        /// <param name="expected">The expected activity from the bot.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends this exchange to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestFlow Test(string userSays, Activity expected, string description = null, uint timeout = 3000)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            return Send(userSays)
                .AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Shortcut for calling <see cref="Send(string)"/> followed by <see cref="AssertReply(Action{IActivity}, string, uint)"/>.
        /// </summary>
        /// <param name="userSays">The text of the message to send.</param>
        /// <param name="validateActivity">A validation method to apply to an activity from the bot.
        /// This activity should throw an exception if validation fails.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends this exchange to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestFlow Test(string userSays, Action<IActivity> validateActivity, string description = null, uint timeout = 3000)
        {
            if (validateActivity == null)
            {
                throw new ArgumentNullException(nameof(validateActivity));
            }

            return Send(userSays)
                .AssertReply(validateActivity, description, timeout);
        }

        /// <summary>
        /// Shortcut for adding an arbitrary exchange between the user and bot.
        /// Each activity with a <see cref="IActivity.From"/>.<see cref="ChannelAccount.Role"/> equals to "bot"
        /// will be processed with the <see cref="AssertReply(IActivity, string, uint)"/> method.
        /// Every other activity will be processed as user's message via the <see cref="Send(IActivity)"/> method.
        /// </summary>
        /// <param name="activities">The list of activities to test.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends this exchange to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestFlow Test(IEnumerable<IActivity> activities, [CallerMemberName] string description = null, uint timeout = 3000)
        {
            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            // Chain all activities in a TestFlow, check if its a user message (send) or a bot reply (assert)
            return activities.Aggregate(this, (flow, activity) =>
            {
                return IsReply(activity)
                    ? flow.AssertReply(activity, description, timeout)
                    : flow.Send(activity);
            });
        }

        /// <summary>
        /// Shortcut for adding an arbitrary exchange between the user and bot.
        /// Each activity with a <see cref="IActivity.From"/>.<see cref="ChannelAccount.Role"/> equals to "bot"
        /// will be processed with the <see cref="AssertReply(IActivity, string, uint)"/> method.
        /// Every other activity will be processed as user's message via the <see cref="Send(IActivity)"/> method.
        /// </summary>
        /// <param name="activities">The list of activities to test.</param>
        /// <param name="validateReply">The delegate to call to validate responses from the bot.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends this exchange to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestFlow Test(IEnumerable<IActivity> activities, ValidateReply validateReply, [CallerMemberName] string description = null, uint timeout = 3000)
        {
            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            // Chain all activities in a TestFlow, check if its a user message (send) or a bot reply (assert)
            return activities.Aggregate(this, (flow, activity) =>
            {
                if (IsReply(activity))
                {
                    return flow.AssertReply((actual) => validateReply(activity, actual), description, timeout);
                }
                else
                {
                    return flow.Send(activity);
                }
            });
        }

        /// <summary>
        /// Adds an assertion that the bot's response is contained within a set of acceptable responses.
        /// </summary>
        /// <param name="candidates">The set of acceptable messages.</param>
        /// <param name="description">A message to send if the actual response is not as expected.</param>
        /// <param name="timeout">The amount of time in milliseconds within which a response is expected.</param>
        /// <returns>A new <see cref="TestFlow"/> object that appends this assertion to the modeled exchange.</returns>
        /// <remarks>This method does not modify the original <see cref="TestFlow"/> object.</remarks>
        /// <exception cref="Exception">The bot did not respond as expected.</exception>
        public TestFlow AssertReplyOneOf(string[] candidates, string description = null, uint timeout = 3000)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            return AssertReply(
                (reply) =>
                {
                    var text = reply.AsMessageActivity().Text;

                    foreach (var candidate in candidates)
                    {
                        if (reply.AsMessageActivity().Text == candidate)
                        {
                            return;
                        }
                    }
                    
                    throw new Exception(description ?? $"Text \"{text}\" does not match one of candidates: {string.Join("\n", candidates)}");
                },
                description,
                timeout);
        }

        private bool IsReply(IActivity activity)
        {
            return string.Equals("bot", activity.From?.Role, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
