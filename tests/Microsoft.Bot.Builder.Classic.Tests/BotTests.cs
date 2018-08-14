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
using System.Collections.Generic;
using Diag = System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.History;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Luis;
using Microsoft.Bot.Builder.Classic.Luis.Models;
using Microsoft.Bot.Builder.Classic.Scorables;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    public interface IBot
    {
        Task PostAsync(Activity activity, CancellationToken token);
    }

    public sealed class Bot : IBot
    {
        // TODO: Microsoft.Extensions.DependencyInjection
        public ILifetimeScope Container;
        private TestAdapter adapter;

        public Bot()
        {
            var builder = new ContainerBuilder();

            // TODO: minimal DialogModule, without DeleteProfileScorable or PostUnhandledExceptionToUserTask
            builder.RegisterModule(new DialogModule());

            // TODO: some trivial dialog?
            Func<IDialog<object>> make = () => Chain.PostToChain();
            //Func<IDialog<object>> make = () => { throw new NotImplementedException(); };
            builder.RegisterInstance(make).AsSelf();

            // for proactive trigger post-to-self
            builder.RegisterInstance(this).As<IBot>();
            this.Container = builder.Build();
            this.adapter = new TestAdapter();
        }

        async Task IBot.PostAsync(Activity activity, CancellationToken token)
        {
            await this.adapter.ProcessActivityAsync(activity, async (context, cancellationToken) =>
            {
                using (var scope = DialogModule.BeginLifetimeScope(this.Container, context))
                {
                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(activity, token);
                }
            });
        }
    }


    public static partial class Extensions
    {
        /// <summary>
        /// Make a trigger activity based on the same conversation referenced by another activity.
        /// </summary>
        public static Activity MakeTrigger(this IActivity activity, object value)
        {
            var trigger = new Activity(ActivityTypes.Event)
            {
                ChannelId = activity.ChannelId,
                Conversation = activity.Conversation,
                From = activity.From,
                Id = Guid.NewGuid().ToString(),
                Recipient = activity.Recipient,
                ServiceUrl = activity.ServiceUrl,
                Value = value,
            };

            return trigger;
        }

        /// <summary>
        /// Exception used to signal resetting a dialog.
        /// </summary>
        public sealed class ResetException : Exception
        {
        }

        /// <summary>
        /// Decorate a dialog with a scorable action to allow resetting state.
        /// </summary>
        public static IDialog<T> WithReset<T>(this IDialog<T> dialog, Func<IDialog<T>, ResetException, IDialog<T>> block, Regex regex)
        {
            // add the ResetException catch block that will execute the dialog reset
            dialog = dialog.Catch<T, ResetException>(block);

            // create a scorable that will fire the ResetException when the RegularExpression is matched
            var scorable =
                Actions
                .Bind(async (IDialogStack stack, CancellationToken token) =>
                {
                    stack.Fail(new ResetException());
                })
                .When(regex)
                .Normalize();

            // decorate the dialog with the scorable
            return dialog.WithScorable(scorable);
        }

        /// <summary>
        /// Decorate a dialog with a scorable event handler to stop event propagation up the stack.
        /// </summary>
        public static IDialog<T> WithEventIgnore<T>(this IDialog<T> dialog)
        {
            var scorable =
                Actions
                .Bind(async (TriggerEvent @event) =>
                {
                    // do nothing on purpose
                })
                .Normalize();

            return dialog.WithScorable(scorable);
        }

        /// <summary>
        /// Decorate a dialog with an scorable event handler to cancel that dialog when interrupted.
        /// </summary>
        public static IDialog<T> WithCancelOnInterrupt<T>(this IDialog<T> dialog)
        {
            var scorable =
                Actions
                .Bind(async (InterruptDialogEvent @event, IDialogStack stack, CancellationToken token) =>
                {
                    stack.Fail(new OperationCanceledException());
                })
                .Normalize();

            return dialog.WithScorable(scorable);
        }

        /// <summary>
        /// A dialog to confirm whether to resume an interrupted dialog when the interrupting dialog is done.
        /// </summary>
        [Serializable]
        public sealed class ConfirmResumeDialog : IDialog<bool>
        {
            private readonly PromptOptions<string> options;
            public ConfirmResumeDialog(PromptOptions<string> options)
            {
                SetField.NotNull(out this.options, nameof(options), options);
            }

            /// <summary>
            /// Call this dialog by interrupting the stack.
            /// </summary>
            /// <remarks>
            /// This method helps us avoid a closure with environment capture in <see cref="WithConfirmOnResume"/>.
            /// </remarks>
            public async Task Action(ResumeDialogEvent @event, IDialogStack stack, CancellationToken token)
            {
                stack.Call(this.Void(stack), null);
            }
            async Task IDialog<bool>.StartAsync(IDialogContext context)
            {
                var confirm = new PromptDialog.PromptConfirm(options);
                context.Call(confirm, AfterConfirm);
            }
            private async Task AfterConfirm(IDialogContext context, IAwaitable<bool> confirm)
            {
                if (await confirm)
                {
                    context.Done(true);
                }
                else
                {
                    context.Fail(new OperationCanceledException());
                }
            }
        }

        /// <summary>
        /// Decorate a dialog with a scorable event handlers to confirm resumption once the interrupting dialog is done.
        /// </summary>
        public static IDialog<T> WithConfirmOnResume<T>(this IDialog<T> dialog, PromptOptions<string> options)
        {
            var confirm = new ConfirmResumeDialog(options);

            var scorable =
                Actions
                .Bind<ResumeDialogEvent, IDialogStack, CancellationToken, Task>(confirm.Action)
                .Normalize();

            return dialog.WithScorable(scorable);
        }

        public static IEventActivity MakeEvent(object @event)
        {
            return new Activity(ActivityTypes.Event) { Value = @event };
        }

        /// <summary>
        /// Interrupt a dialog stack with a new dialog, following rules for sending interrupt and resume events.
        /// </summary>
        public static async Task InterruptAsync<T>(this IDialogSystem system, IDialogStack stack, IDialog<T> dialog, CancellationToken token)
        {
            if (!system.DialogTasks.Contains(stack))
            {
                throw new ArgumentOutOfRangeException(nameof(stack));
            }

            // send the interruption event
            system.Post(MakeEvent(new InterruptDialogEvent()));
            await system.PollAsync(token);

            var interrupter =
                // when the dialog is done
                dialog
                // task boundary: catch all exceptions
                .DefaultIfException()
                // task boundary: catch all events
                .WithEventIgnore();

            // Chain.PostToChain is two frames - yuck?
            if (stack.Frames.Count > 2)
            {
                // post the resume dialog event to the queue
                interrupter = interrupter.PostEvent(new ResumeDialogEvent());
            }

            // ignore/void the result, and then resume the interrupted wait
            var voided = interrupter.Void(stack);

            // start the interrupting dialog
            stack.Call(voided, null);
            // run the task until the next wait
            await system.PollAsync(token);
        }
    }

    [Serializable]
    public partial class TriggerEvent
    {
        public override string ToString()
        {
            return this.GetType().Name;
        }
    }

    public partial class TimerTriggerEvent : TriggerEvent
    {
        public readonly string Text;
        public TimerTriggerEvent(string text)
        {
            SetField.NotNull(out this.Text, nameof(text), text);
        }
    }

    [Serializable]
    public partial class InterruptDialogEvent : TriggerEvent
    {
    }

    [Serializable]
    public partial class ResumeDialogEvent : TriggerEvent
    {
    }

    public static partial class ExampleBot
    {
        public static string ToString(object instance)
        {
            if (instance != null)
            {
                var lambda = instance as Delegate;
                if (lambda != null)
                {
                    return $"{ToString(lambda.Target)}.{lambda.Method.Name}";
                }
                else
                {
                    var type = instance.GetType();
                    var method = type.GetMethod("ToString");
                    if (typeof(object).Equals(method.DeclaringType))
                    {
                        return type.Name;
                    }
                }

                return instance.ToString();
            }

            return null;
        }

        public static async Task RenderAsync(IBotToUser botToUser, IDialogStack stack, string message, CancellationToken token)
        {
            var frames = stack.Frames;

            var builder = new StringBuilder();
            if (message != null)
            {
                builder.Append($"{message}").AppendLine().AppendLine();
            }
            builder.Append($"Stack Frames ({frames.Count}): ").AppendLine().AppendLine();

            builder.Append("~~~~").AppendLine().AppendLine();
            for (int index = 0; index < frames.Count; ++index)
            {
                var frame = frames[index];
                builder.Append($"* {index}: {ToString(frame)}").AppendLine().AppendLine();
            }
            builder.Append("~~~~").AppendLine().AppendLine();

            await botToUser.PostAsync(builder.ToString(), null, token);
        }

        public interface IDataService<T>
        {
            bool TryParse(IActivity activity, out T parsed);
        }

        public sealed class DataService : IDataService<string>
        {
            private int count;
            bool IDataService<string>.TryParse(IActivity activity, out string parsed)
            {
                var message = activity as IMessageActivity;
                string text = message != null ? message.Text : activity.Type;
                bool success = !string.IsNullOrWhiteSpace(text);
                if (success)
                {
                    parsed = $"{count}:{text}";
                    ++count;
                }
                else
                {
                    parsed = null;
                }
                return success;
            }
        }

        public sealed class BotActivityLogger : IActivityLogger
        {
            async Task IActivityLogger.LogAsync(IActivity activity)
            {
                Diag.Trace.TraceInformation($"{activity.Type}: {activity.From.Id} -> {activity.Recipient.Id}");
            }
        }

        [Serializable]
        public sealed class ListBuilderDialog<T> : IDialog<IReadOnlyList<T>> where T : class
        {
            private readonly List<T> items = new List<T>();
            private readonly IDataService<T> service;
            public ListBuilderDialog(IDataService<T> service)
            {
                SetField.NotNull(out this.service, nameof(service), service);
            }
            public void Reset()
            {
                this.items.Clear();
            }
            async Task IDialog<IReadOnlyList<T>>.StartAsync(IDialogContext context)
            {
                await context.PostAsync("Let's starting building a list!", null, context.CancellationToken);
                NextItem(context);
            }
            private void NextItem(IDialogContext context)
            {
                var dialog = new ItemBuilderDialog<T>(this.service);
                context.Call(dialog, AfterItemAsync);
            }
            private async Task AfterItemAsync(IDialogContext context, IAwaitable<T> awaitable)
            {
                var item = await awaitable;
                if (item != null)
                {
                    Func<T, string> Render = i => string.Concat("'", i, "'");
                    var list = string.Join(",", this.items.Select(Render));
                    await context.PostAsync($"Adding item '{Render(item)}' to list [{list}].", null, context.CancellationToken);
                    this.items.Add(item);
                    NextItem(context);
                }
                else
                {
                    context.Done(this.items);
                }
            }
        }

        [Serializable]
        public sealed class ItemBuilderDialog<T> : IDialog<T>
        {
            private readonly IDataService<T> service;
            public ItemBuilderDialog(IDataService<T> service)
            {
                SetField.NotNull(out this.service, nameof(service), service);
            }
            public async Task StartAsync(IDialogContext context)
            {
                await context.PostAsync("Please enter an item.");
                context.Wait(ActivityReceivedAsync);
            }
            private async Task ActivityReceivedAsync(IDialogContext context, IAwaitable<IActivity> item)
            {
                var activity = await item;
                T parsed;
                if (this.service.TryParse(activity, out parsed))
                {
                    context.Done(parsed);
                }
                else
                {
                    await context.PostAsync("Sorry, I did not understand that.");
                    await StartAsync(context);
                }
            }
        }

        [Serializable]
        public sealed class ChitChatDialog : DispatchDialog<object>
        {
            public const string PatternHello = "(?i)(hello|hi|greeting|salutation)";

            [RegexPattern(PatternHello)]
            [ScorableGroup(0)]
            private async Task HelloAsync(IDialogContext context)
            {
                await context.PostAsync("hello!");
                context.Wait(ActivityReceivedAsync);
            }
            [RegexPattern("(?i)(goodbye|bye|done|quit|stop)")]
            [ScorableGroup(0)]
            private async Task Stop(IDialogContext context)
            {
                await context.PostAsync("bye!");
                context.Done<object>(null);
            }
            [RegexPattern(".*")]
            [ScorableGroup(1)]
            private async Task Fallback(IDialogContext context)
            {
                await context.PostAsync("I'm just talking here.");
                context.Wait(ActivityReceivedAsync);
            }
        }

        public static readonly LuisIntentAttribute IntentSetAlarm = new LuisIntentAttribute("builtin.intent.alarm.set_alarm");

        public static IScorable<IResolver, double> MakeActions(Func<ILuisModel, ILuisService> MakeService)
        {
            ILuisModel model = new LuisModelAttribute("c413b2ef-382c-45bd-8ff0-f76d60e2a821", "6d0966209c6e4f6b835ce34492f3e6d9");

            var actions = new[]
            {
                // echo text back to the user

                Actions
                .Bind(async (IBotToUser toUser, System.Text.RegularExpressions.Capture text, CancellationToken token) =>
                {
                    await toUser.PostAsync($"echo: {text.Value}");
                })
                // when recognizing this regular expression, with a named "text" capture
                .When(new Regex(@"echo\s*(?<text>(?:.*)?)"))
                .Normalize(),

                // display the current stack 

                Actions
                .Bind(async (IBotToUser toUser, IDialogStack stack, CancellationToken token) =>
                {
                    await RenderAsync(toUser, stack, null, token);
                })
                .When(new Regex("stack"))
                .Normalize(),

                // reset the stack

                Actions
                .Bind(async (IBotToUser toUser, IDialogTaskManager manager, CancellationToken token) =>
                {
                    await toUser.PostAsync($"Resetting the stack.");

                    foreach (var task in manager.DialogTasks)
                    {
                        task.Reset();
                    }
                })
                .When(new Regex("reset"))
                .Normalize(),

                // cancel dialog

                Actions
                .Bind(async (IBotToUser toUser, IDialogTaskManager manager, CancellationToken token) =>
                {
                    await toUser.PostAsync($"Injecting an OperationCanceledException into the stack.");

                    foreach (var stack in manager.DialogTasks)
                    {
                        if (stack.Frames.Count > 0)
                        {
                            stack.Fail(new OperationCanceledException());
                        }
                    }
                })
                .When(new Regex("cancel"))
                .Normalize(),

                // start a modal chit chat dialog

                Actions
                .Bind(async (IBotToUser toUser, IDialogSystem system, IActivity activity, CancellationToken token) =>
                {
                    var dialog = new ChitChatDialog().WithCancelOnInterrupt();

                    var task = system.DialogTasks[0];

                    // interrupt the current stack
                    await system.InterruptAsync(task, dialog, token);

                    // forward the incoming activity
                    system.Post(activity);
                })
                // prevent re-entrancy of the chit chat dialog
                .Where(async (IDialogStack stack) => ! stack.Frames.Any(f => f.Target is ChitChatDialog))
                // when recognizing this regular expression
                .When(new Regex(ChitChatDialog.PatternHello))
                .Normalize(),

                // start a modal list building dialog

                Actions
                .Bind(async (IBotToUser toUser, IDialogSystem system, System.Text.RegularExpressions.Capture forward, IMessageActivity activity, IDataService<string> service, CancellationToken token) =>
                {
                    var task = system.DialogTasks[0];
                    if (task.Frames.Any(f => f.Target is ListBuilderDialog<string>))
                    {
                        await toUser.PostAsync($"You don't want to re-enter the {typeof(ListBuilderDialog<string>).Name}!");
                    }
                    else
                    {
                        IDialog<IReadOnlyList<string>> dialog = new ListBuilderDialog<string>(service);

                        // add the reset scorable decorator
                        dialog = dialog.WithReset((d, e) =>
                        {
                            var list = (ListBuilderDialog<string>)d;
                            list.Reset();
                            return list;
                        }, new Regex("reset"));

                        // add the resumption scorable decorator
                        var options = new PromptOptions<string>(prompt: "Do you want to continue building the list?");
                        dialog = dialog.WithConfirmOnResume(options);

                        // interrupt the current stack
                        await system.InterruptAsync(task, dialog, token);

                        // optionally forwarding the incoming activity
                        if (forward.Value.Length > 0)
                        {
                            activity.Text = forward.Value;
                            system.Post(activity);
                        }
                    }
                })
                // when recognizing this regular expression, with an optional "forward" capture
                .When(new Regex(@"list\s*(?<forward>(?:.*)?)"))
                .Normalize(),

                // use a trigger activity to "poke the bot" with a proactive event after some time has passed

                Actions
                .Bind(async (IBotToUser toUser, IBot bot, IMessageActivity activity, CancellationToken token) =>
                {
                    // make a trigger activity with the same address information
                    var @event = new TimerTriggerEvent(activity.Text);
                    var trigger = activity.MakeTrigger(@event);

                    // spawn a task to run "later", after this activity is handled
                    var span = TimeSpan.FromSeconds(5);
                    var task = Task.Run(async () =>
                    {
                        await Task.Delay(span);
                        // emulate an external, proactive event
                        await bot.PostAsync(trigger, CancellationToken.None);
                    });

                    await toUser.PostAsync($"Sending a {trigger.Type} activity with text '{@event.Text}' in {span}.", null, token);
                })
                .When(model, IntentSetAlarm, MakeService(model))
                .Normalize(),

                // received the proactive "poke the bot" trigger activity

                Actions
                .Bind(async (IBotToUser toUser, IEventActivity trigger, TimerTriggerEvent evt, CancellationToken token) =>
                {
                    await toUser.PostAsync($"Received a {trigger.Type} activity initiated after timer from text '{evt.Text}'.");
                })
                .Normalize(),

                // handle a top-level conversation update activity

                Actions
                .Bind(async (IBotToUser toUser, IConversationUpdateActivity update, CancellationToken token) =>
                {
                    if (update.MembersAdded != null)
                    {
                        foreach (var member in update.MembersAdded)
                        {
                            await toUser.PostAsync($"Welcome {member.Name}!");
                        }
                    }

                    if (update.MembersRemoved != null)
                    {
                        foreach (var member in update.MembersRemoved)
                        {
                            await toUser.PostAsync($"Farewell {member.Name}!");
                        }
                    }
                })
                .Normalize(),

            };

            // create a composed scorable action that folds these scorable actions together
            // to select the action with the highest double score
            return actions.Fold(Comparer<double>.Default);
        }

        public static IBot MakeBot(Func<ILuisModel, ILuisService> MakeService)
        {
            // a bot is composed of a list of scorable actions
            var actions = MakeActions(MakeService);

            var bot = new Bot();

            // TODO: Microsoft.Extensions.DependencyInjection
            bot.Container = bot.Container.BeginLifetimeScope(
                builder =>
                {
                    // configure the brain dispatcher of the bot with the top-level scorable actions
                    builder
                        .RegisterInstance(actions)
                        .AsImplementedInterfaces()
                        .SingleInstance();

                    // add a singleton data service, as an example
                    builder
                        .RegisterInstance(new DataService())
                        .Keyed<IDataService<string>>(FiberModule.Key_DoNotSerialize)
                        .AsImplementedInterfaces()
                        .SingleInstance();

                    // add an activity logger, as an example
                    builder
                        .RegisterType<BotActivityLogger>()
                        .AsImplementedInterfaces()
                        .SingleInstance();
                });

            return bot;
        }
    }

    [TestClass]
    public sealed class BotTests : DialogTestBase
    {
        /// <summary>
        /// Activity logging for scripted unit tests
        /// </summary>
        public sealed class TestActivityLogger : IActivityLogger
        {
            private readonly StreamWriter writer;
            public TestActivityLogger(StreamWriter writer)
            {
                SetField.NotNull(out this.writer, nameof(writer), writer);
            }

            private readonly Dictionary<string, string> ordinalById = new Dictionary<string, string>();

            /// <summary>
            /// standardize GUID-ish strings into a compact integer space for easier readability
            /// </summary>
            private string OrdinalFor(string id)
            {
                if (id == null)
                {
                    return null;
                }

                string ordinal;
                if (!ordinalById.TryGetValue(id, out ordinal))
                {
                    ordinal = ordinalById.Count.ToString();
                    ordinalById.Add(id, ordinal);
                }

                return ordinal;
            }
            async Task IActivityLogger.LogAsync(IActivity activity)
            {
                var clone = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                clone.Timestamp = null;
                clone.LocalTimestamp = null;
                clone.Id = OrdinalFor(clone.Id);
                clone.ReplyToId = OrdinalFor(clone.ReplyToId);
                clone.Conversation.Id = OrdinalFor(clone.Conversation.Id);

                var settings = new JsonSerializerSettings()
                {
                    ContractResolver = new OrderedContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                };

                var json = JsonConvert.SerializeObject(clone, settings);
                await this.writer.WriteLineAsync(json);
            }
        }

        public TestContext TestContext { get; set; }

        public const string UtteranceSetAlarm = "alarm";

        public static ILuisService MakeMockedLuisService(ILuisModel model)
        {
            var mock = new Mock<ILuisService>(MockBehavior.Strict);
            var request = new LuisRequest(query: UtteranceSetAlarm);

            var uri = request.BuildUri(model);

            mock
                .Setup(l => l.BuildUri(It.IsAny<LuisRequest>()))
                .Returns<LuisRequest>(r => ((ILuisService)new LuisService(model)).BuildUri(r));

            mock
                .Setup(l => l.ModifyRequest(It.IsAny<LuisRequest>()))
                .Returns<LuisRequest>(r => r);

            mock
                .Setup(l => l.QueryAsync(It.Is<Uri>(u => u == uri), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LuisResult()
                {
                    Intents = new[]
                    {
                        new IntentRecommendation()
                        {
                            Intent = ExampleBot.IntentSetAlarm.IntentName,
                            Score = 1.0
                        }
                    },
                    Entities = Array.Empty<EntityModel>(),
                });

            mock
                .Setup(l => l.QueryAsync(It.Is<Uri>(u => u != uri), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LuisResult()
                {
                    Intents = Array.Empty<IntentRecommendation>(),
                    Entities = Array.Empty<EntityModel>(),
                });

            return mock.Object;
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Scripts\BotDispatcher.script")]
        public async Task BotDispatcher()
        {
            var token = new CancellationTokenSource().Token;
            var pathOld = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            var pathNew = TestFiles.TestResultPathFor(TestContext, Path.GetFileName(pathOld));

            using (var writer = File.CreateText(pathNew))
            {
                var message = (Activity)MakeTestMessage();

                var bot = ExampleBot.MakeBot(MakeMockedLuisService);
                // TODO: Microsoft.Extensions.DependencyInjection
                ((Bot)bot).Container = ((Bot)bot).Container.BeginLifetimeScope(
                    builder =>
                    {
                        // register singleton StreamWriter
                        builder
                            .RegisterInstance(writer)
                            .AsSelf();

                        // log all activities to and from bot
                        builder
                            .RegisterType<TestActivityLogger>()
                            .AsImplementedInterfaces()
                            .SingleInstance();

                        // truncate AlwaysSendDirect_BotToUser/IConnectorClient with null implementation
                        builder
                            .RegisterType<NullBotToUser>()
                            .Keyed<IBotToUser>(typeof(AlwaysSendDirect_BotToUser))
                            .InstancePerLifetimeScope();
                    });

                var texts = new[]
                {
                    "echo reset an empty stack",
                    "reset",

                    "echo start chit chat dialog with normal quit",
                    "hello",
                    "lol wut",
                    "goodbye",

                    "echo start chit chat dialog with cancel quit",
                    "hello",
                    "lol wut",
                    "cancel",

                    "echo start list builder dialog with cancel quit",
                    "list",
                    "itemA",
                    "itemB",
                    "cancel",

                    "echo start list builder dialog with message forward with cancel quit",
                    "list itemC",
                    "itemD",
                    "cancel",

                    "echo start list builder dialog with reset",
                    "list",
                    "itemE",
                    "reset",
                    "itemF",
                    "cancel",

                    "echo chit chat dialog is not re-entrant",
                    "hello",
                    "hello",
                    "cancel",

                    "echo list dialog is not re-entrant",
                    "list",
                    "list",
                    "cancel",

                    "echo list builder dialog interrupts chit chat dialog",
                    "hello",
                    "list itemG",
                    "cancel",

                    "echo chit chat dialog interrupts list builder dialog then resume list builder dialog",
                    "list itemH",
                    "hello",
                    "cancel",
                    "yes",
                    "itemI",
                    "cancel",

                    "echo chit chat dialog interrupts list builder dialog then cancel list builder dialog",
                    "list itemH",
                    "hello",
                    "cancel",
                    "no",

                    // proactive (NEED TO MOCK TIMER)
                    //"echo proactive",
                    //UtteranceSetAlarm,

                    // clean up
                    "reset",
                };

                for (int index = 0; index < texts.Length; ++index)
                {
                    var text = texts[index];

                    message.Text = "stack";
                    await bot.PostAsync(message, token);

                    message.Text = text;
                    await bot.PostAsync(message, token);
                }

                message.Text = "stack";
                await bot.PostAsync(message, token);
            }

            CollectionAssert.AreEqual(File.ReadAllLines(pathOld), File.ReadAllLines(pathNew));
        }

        [TestMethod]
        public async Task SayAsync_ShouldSendText()
        {
            var dialog = Chain.PostToChain().Do(async (context, activity) =>
            {
                await context.SayAsync("some text");
            });

            using (var container = Build(Options.None))
            {
                var toBot = MakeTestMessage();
                toBot.Text = "hi";

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, () => dialog);

                        var task = scope.Resolve<IPostToBot>();

                        await task.PostAsync(toBot, CancellationToken.None);

                        await AssertOutgoingActivity(scope, (toUser) =>
                        {
                            Assert.AreEqual("some text", toUser.Text);
                            Assert.IsNull(toUser.Speak);
                            Assert.AreEqual(0, toUser.Attachments.Count());
                        });
                    }
                });
            };
        }

        [TestMethod]
        public async Task SayAsync_ShouldSendTextAndSSML()
        {
            var dialog = Chain.PostToChain().Do(async (context, activity) =>
            {
                await context.SayAsync("some text", "some ssml");
            });

            using (var container = Build(Options.None))
            {
                var toBot = MakeTestMessage();
                toBot.Text = "hi";
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, () => dialog);

                        var task = scope.Resolve<IPostToBot>();

                        await task.PostAsync(toBot, CancellationToken.None);

                        await AssertOutgoingActivity(scope, (toUser) =>
                        {
                            Assert.AreEqual("some text", toUser.Text);
                            Assert.AreEqual("some ssml", toUser.Speak);
                            Assert.AreEqual(0, toUser.Attachments.Count());
                        });
                    }
                });
            };
        }

        [TestMethod]
        public async Task SayAsync_ShouldSendTextAndAttachments()
        {
            var dialog = Chain.PostToChain().Do(async (context, activity) =>
            {
                var messageOptions = new MessageOptions
                {
                    Attachments =
                    {
                            new Attachment
                            {
                                ContentType = "foo",
                                Content = "bar"
                            }
                    }
                };
                await context.SayAsync("some text", options: messageOptions);
            });

            using (var container = Build(Options.None))
            {
                var toBot = MakeTestMessage();
                toBot.Text = "hi";
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, () => dialog);

                        var task = scope.Resolve<IPostToBot>();

                        await task.PostAsync(toBot, CancellationToken.None);

                        await AssertOutgoingActivity(scope, (toUser) =>
                        {
                            Assert.AreEqual("some text", toUser.Text);
                            Assert.IsNull(toUser.Speak);
                            Assert.AreEqual(1, toUser.Attachments.Count());
                            Assert.AreEqual("foo", toUser.Attachments[0].ContentType);
                        });
                    }
                });
            };
        }

        [TestMethod]
        public async Task SayAsync_ShouldSendTextAndSSMLAndAttachments()
        {
            var dialog = Chain.PostToChain().Do(async (context, activity) =>
            {
                var messageOptions = new MessageOptions
                {
                    Attachments =
                    {
                            new Attachment
                            {
                                ContentType = "foo",
                                Content = "bar"
                            }
                    }
                };
                await context.SayAsync("some text", "some ssml", messageOptions);
            });

            using (var container = Build(Options.None))
            {
                var toBot = MakeTestMessage();
                toBot.Text = "hi";
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, () => dialog);

                        var task = scope.Resolve<IPostToBot>();
                        await task.PostAsync(toBot, CancellationToken.None);

                        await AssertOutgoingActivity(scope, (toUser) =>
                        {
                            Assert.AreEqual("some text", toUser.Text);
                            Assert.AreEqual("some ssml", toUser.Speak);
                            Assert.AreEqual(1, toUser.Attachments.Count());
                            Assert.AreEqual("foo", toUser.Attachments[0].ContentType);
                        });
                    }
                });
            };
        }

        [TestMethod]
        public async Task SayAsync_ShouldApplyMessageOptions()
        {
            var dialog = Chain.PostToChain().Do(async (context, activity) =>
            {
                var messageOptions = new MessageOptions
                {
                    AttachmentLayout = AttachmentLayoutTypes.Carousel,
                    TextFormat = TextFormatTypes.Plain,
                    InputHint = InputHints.ExpectingInput,
                    Entities =
                    {
                            new Mention
                            {
                                 Type = "mention",
                                 Text = "foo"
                            }
                    }
                };
                await context.SayAsync("some text", options: messageOptions);
            });

            using (var container = Build(Options.None))
            {
                var toBot = MakeTestMessage();
                toBot.Text = "hi";

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, () => dialog);

                        var task = scope.Resolve<IPostToBot>();

                        await task.PostAsync(toBot, CancellationToken.None);

                        await AssertOutgoingActivity(scope, (toUser) =>
                        {
                            Assert.AreEqual("some text", toUser.Text);
                            Assert.AreEqual(0, toUser.Attachments.Count());
                            Assert.AreEqual(AttachmentLayoutTypes.Carousel, toUser.AttachmentLayout);
                            Assert.AreEqual(TextFormatTypes.Plain, toUser.TextFormat);
                            Assert.AreEqual(InputHints.ExpectingInput, toUser.InputHint);
                            Assert.AreEqual(1, toUser.Entities.Count());
                            Assert.IsInstanceOfType(toUser.Entities[0], typeof(Mention));
                            Assert.AreEqual("foo", ((Mention)toUser.Entities[0]).Text);
                        });
                    }
                });
            };
        }
    }
}
