// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters
{
    /// <summary>
    /// Called by TestFlow to validate and activity
    /// </summary>
    /// <param name="expected">Activity from trnascript file</param>
    /// <param name="actual">Activity from bot</param>
    public delegate void ValidateReply(IActivity expected, IActivity actual);

    public class TestFlow
    {
        private readonly TestAdapter adapter;
        private readonly Task testTask;
        private Func<ITurnContext, Task> callback;

        public TestFlow(TestAdapter adapter, Func<ITurnContext, Task> callback = null)
        {
            this.adapter = adapter;
            this.callback = callback;
            this.testTask = testTask ?? Task.CompletedTask;
        }

        public TestFlow(Task testTask, TestFlow flow)
        {
            this.testTask = testTask ?? Task.CompletedTask;
            this.callback = flow.callback;
            this.adapter = flow.adapter;
        }

        public TestFlow(TestAdapter adapter, IBot bot) : this(adapter, (ctx) => bot.OnTurn(ctx))
        { }

        /// <summary>
        /// Start the execution of the test flow
        /// </summary>
        /// <returns></returns>
        public Task StartTest()
        {
            return this.testTask;
        }

        /// <summary>
        /// Send a message from the user to the bot
        /// </summary>
        /// <param name="userSays"></param>
        /// <returns></returns>
        public TestFlow Send(string userSays)
        {
            if (userSays == null)
                throw new ArgumentNullException("You have to pass a userSays parameter");

            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: we need to .Wait() on the original Task to properly observe any exceptions that might have occurred
                // and to have them propagate correctly up through the chain to whomever is waiting on the parent task
                // The following StackOverflow answer provides some more details on why you want to do this:
                // https://stackoverflow.com/questions/11904821/proper-way-to-use-continuewith-for-tasks/11906865#11906865
                //
                // From the Docs:
                //  https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library
                //  Exceptions are propagated when you use one of the static or instance Task.Wait or Wait
                //  methods, and you handle them by enclosing the call in a try/catch statement. If a task is the
                //  parent of attached child tasks, or if you are waiting on multiple tasks, multiple exceptions
                //  could be thrown.
                task.Wait();

                return this.adapter.SendTextToBot(userSays, this.callback);
            }).Unwrap(), this);
        }

        /// <summary>
        /// Send an activity from the user to the bot
        /// </summary>
        /// <param name="userActivity"></param>
        /// <returns></returns>
        public TestFlow Send(IActivity userActivity)
        {
            if (userActivity == null)
                throw new ArgumentNullException("You have to pass an Activity");

            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: See details code in above method.
                task.Wait();

                return this.adapter.ProcessActivity((Activity)userActivity, this.callback);
            }).Unwrap(), this);
        }

        /// <summary>
        /// Delay for time period
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public TestFlow Delay(UInt32 ms)
        {
            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: See details code in above method.
                task.Wait();

                return Task.Delay((int)ms);
            }), this);
        }

        /// <summary>
        /// Assert that reply is expected text
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(string expected, string description = null, UInt32 timeout = 3000)
        {
            return this.AssertReply(this.adapter.MakeActivity(expected), description, timeout);
        }

        /// <summary>
        /// Assert that the reply is expected activity
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(IActivity expected, [CallerMemberName] string description = null, UInt32 timeout = 3000)
        {
            return this.AssertReply((reply) =>
            {
                if (expected.Type != reply.Type)
                    throw new Exception($"{description}: Type should match");
                if (expected.AsMessageActivity().Text != reply.AsMessageActivity().Text)
                {
                    if (description == null)
                        throw new Exception($"Expected:{expected.AsMessageActivity().Text}\nReceived:{reply.AsMessageActivity().Text}");
                    else
                        throw new Exception($"{description}: Text should match");
                }
                // TODO, expand this to do all properties set on expected
            }, description, timeout);
        }

        /// <summary>
        /// Assert that the reply matches a custom validation routine
        /// </summary>
        /// <param name="validateActivity"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(Action<IActivity> validateActivity, [CallerMemberName] string description = null, UInt32 timeout = 3000)
        {
            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: See details code in above method.
                task.Wait();

                if (System.Diagnostics.Debugger.IsAttached)
                    timeout = UInt32.MaxValue;

                var start = DateTime.UtcNow;
                while (true)
                {
                    var current = DateTime.UtcNow;

                    if ((current - start).TotalMilliseconds > timeout)
                    {
                        throw new TimeoutException($"{timeout}ms Timed out waiting for:'{description}'");
                    }

                    IActivity replyActivity = this.adapter.GetNextReply();
                    if (replyActivity != null)
                    {
                        // if we have a reply
                        validateActivity(replyActivity);
                        return;
                    }
                }
            }), this);
        }

        /// <summary>
        /// Say() -> shortcut for .Send(user).AssertReply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow Test(string userSays, string expected, string description = null, UInt32 timeout = 3000)
        {
            if (expected == null)
                throw new ArgumentNullException(nameof(expected));

            return this.Send(userSays)
                .AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Test() -> shortcut for .Send(user).AssertReply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow Test(string userSays, Activity expected, string description = null, UInt32 timeout = 3000)
        {
            if (expected == null)
                throw new ArgumentNullException(nameof(expected));

            return this.Send(userSays)
                .AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Say() -> shortcut for .Send(user).AssertReply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow Test(string userSays, Action<IActivity> expected, string description = null, UInt32 timeout = 3000)
        {
            if (expected == null)
                throw new ArgumentNullException(nameof(expected));

            return this.Send(userSays)
                .AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Shorcut to test a conversation for .Send(user).AssertReply(bot)
        /// Each activity with From.Role equals to "bot" will be processed with AssertReply method
        /// Every other activity will be processed as User's message with Send method
        /// </summary>
        /// <param name="activities">List of activities to test</param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns>TestFlow to test</returns>
        public TestFlow Test(IEnumerable<IActivity> activities, [CallerMemberName] string description = null, UInt32 timeout = 3000)
        {
            if (activities == null)
                throw new ArgumentNullException(nameof(activities));

            // Chain all activities in a TestFlow, check if its a user message (send) or a bot reply (assert)
            return activities.Aggregate(this, (flow, activity) =>
            {
                return IsReply(activity)
                    ? flow.AssertReply(activity, description, timeout)
                    : flow.Send(activity);
            });
        }

        /// <summary>
        /// Shorcut to test a conversation for .Send(user).AssertReply(bot)
        /// Each activity with From.Role equals to "bot" will be processed with AssertReply method
        /// Every other activity will be processed as User's message with Send method
        /// </summary>
        /// <param name="activities">List of activities to test</param>
        /// <param name="validateReply">Custom validation between an expected response and the actual response</param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns>TestFlow to test</returns>
        public TestFlow Test(IEnumerable<IActivity> activities, ValidateReply validateReply, [CallerMemberName] string description = null, UInt32 timeout = 3000)
        {
            if (activities == null)
                throw new ArgumentNullException(nameof(activities));

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
                };
            });
        }

        private bool IsReply(IActivity activity)
        {
            return string.Equals("bot", activity.From?.Role, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Assert that reply is one of the candidate responses
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReplyOneOf(string[] candidates, string description = null, UInt32 timeout = 3000)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            return this.AssertReply((reply) =>
            {
                foreach (var candidate in candidates)
                {
                    if (reply.AsMessageActivity().Text == candidate)
                        return;
                }
                throw new Exception(description ?? $"Not one of candidates: {String.Join("\n", candidates)}");
            }, description, timeout);
        }
    }
}