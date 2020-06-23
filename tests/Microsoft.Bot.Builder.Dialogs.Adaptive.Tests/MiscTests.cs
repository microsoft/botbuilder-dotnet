// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class MiscTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(MiscTests)), monitorChanges: false);
        }

        [TestMethod]
        public async Task IfCondition_EndDialog()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Rule_Reprompt()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task DialogManager_InitDialogsEnsureDependencies()
        {
            Dialog CreateDialog()
            {
                return new AdaptiveDialog()
                {
                    Recognizer = new CustomRecognizer(),
                    Triggers = new List<Adaptive.Conditions.OnCondition>()
                    {
                        new OnUnknownIntent()
                        {
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("You said '${turn.activity.text}'"),
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Enter age"),
                                    Property = "$age"
                                },
                                new SendActivity("You said ${$age}")
                            }
                        }
                    }
                };
            }

            await new TestScript()
            {
                Dialog = CreateDialog()
            }
                .Send("hi")
                    .AssertReply("You said 'hi'")
                    .AssertReply("Enter age")
                .Send("10")
                    .AssertReply("You said 10")
                .ExecuteAsync(ResourceExplorer);

            // create new dialog manager and new dialog each turn should be the same as when it's static
            await new TestScript()
                .Send("hi")
                    .AssertReply("You said 'hi'")
                    .AssertReply("Enter age")
                .Send("10")
                    .AssertReply("You said 10")
                .ExecuteAsync(ResourceExplorer, callback: (context, ct) => new DialogManager(CreateDialog())
                    .UseResourceExplorer(ResourceExplorer)
                    .UseLanguageGeneration()
                    .OnTurnAsync(context, ct));
        }

        [TestMethod]
        public async Task TestMultipleEditActions()
        {
            var jokeDialog = new AdaptiveDialog("jokeDialog")
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Sure.. Here's the joke .. ha ha ha :) ")
                        }
                    }
                }
            };

            var userProfileDialog = new AdaptiveDialog("userProfileDialog")
            {
                AutoEndDialog = true,
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern()
                        {
                            Intent = "why",
                            Pattern = "why"
                        },
                        new IntentPattern()
                        {
                            Intent = "no",
                            Pattern = "no"
                        }
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("In profile dialog..."),
                            new TextInput()
                            {
                                Id = "askForName",
                                Prompt = new ActivityTemplate("What is your name?"),
                                Property = "user.name"
                            },
                            new SendActivity("I have ${user.name}"),
                            new TextInput()
                            {
                                Id = "askForAge",
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age"
                            },
                            new SendActivity("I have ${user.age}")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "why",
                        Condition = "isDialogActive('askForName')",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("I need your name to address you correctly"),
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "why",
                        Condition = "isDialogActive('askForAge')",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("I need your age to provide relevant product recommendations")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "why",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("I need your information to complete the sample..")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "no",
                        Actions = new List<Dialog>()
                        {
                            new SetProperties()
                            {
                                Assignments = new List<PropertyAssignment>()
                                {
                                    new PropertyAssignment()
                                    {
                                        Property = "user.name",
                                        Value = "Human"
                                    },
                                    new PropertyAssignment()
                                    {
                                        Property = "user.age",
                                        Value = "30"
                                    }
                                }
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "no",
                        Condition = "isDialogActive('askForName')",
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "user.name",
                                Value = "Human"
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "no",
                        Condition = "isDialogActive('askForAge')",
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "user.age",
                                Value = "30"
                            }
                        }
                    }
                }
            };

            var rootDialog = new AdaptiveDialog("root")
            {
                AutoEndDialog = false,
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern()
                        {
                            Intent = "profile",
                            Pattern = "profile"
                        },
                        new IntentPattern()
                        {
                            Intent = "joke",
                            Pattern = "joke"
                        },
                        new IntentPattern()
                        {
                            Intent = "start",
                            Pattern = "start"
                        }
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Say profile | joke | start to get started")
                        }
                    },

                    // joke is always available if it is an interruption.
                    new OnIntent()
                    {
                        Intent = "joke",
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog()
                            {
                                Dialog = "jokeDialog"
                            }
                        }
                    },

                    // queue up joke if we are in the middle of prompting for name or age. You can also do this via 
                    // contains(dialogContext.state, 'askForName') || contains(dialogContext.state, 'askForAge')
                    new OnIntent()
                    {
                        Intent = "joke",
                        Condition = "isDialogActive('askForName', 'askForAge')",
                        Actions = new List<Dialog>()
                        {
                            new EditActions()
                            {
                                ChangeType = ActionChangeType.InsertActions,
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("Sure. I will get you a joke after I get your name..")
                                }
                            },
                            new EditActions()
                            {
                                ChangeType = ActionChangeType.AppendActions,
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("Here is the joke you asked for.."),
                                    new BeginDialog()
                                    {
                                        Dialog = jokeDialog
                                    }
                                }
                            }
                        }
                    },

                    // Only possible if in turn.0. Not possible if this is an interruption
                    new OnIntent()
                    {
                        Intent = "start",
                        Condition = "!hasPendingActions()",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("In start .. this action is not possible as an interruption...")
                        }
                    },

                    // Do not do this if childDialog is already in the stack => child cannot self interrupt itself
                    new OnIntent()
                    {
                        Intent = "profile",
                        Condition = "!isDialogActive('userProfileDialog')",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("In profile .. this is always possible and I handle it immediately."),
                            new BeginDialog()
                            {
                                Dialog = userProfileDialog
                            }
                        }
                    }
                }
            };

            await new TestScript() { Dialog = rootDialog }
                .SendConversationUpdate()
                    .AssertReply("Say profile | joke | start to get started")
                .Send("start")
                    .AssertReply("In start .. this action is not possible as an interruption...")
                .Send("profile")
                    .AssertReply("In profile .. this is always possible and I handle it immediately.")
                    .AssertReply("In profile dialog...")
                    .AssertReply("What is your name?")
                .Send("joke")
                    .AssertReply("Sure. I will get you a joke after I get your name..")
                    .AssertReply("What is your name?")
                .Send("Fred")
                    .AssertReply("I have Fred")
                    .AssertReply("What is your age?")
                .Send("52")
                    .AssertReply("I have 52")
                    .AssertReply("Here is the joke you asked for..") 
                .ExecuteAsync(ResourceExplorer);
        }
    }

    public class CustomRecognizer : Recognizer, IRecognizer
    {
        public override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            return this.RecognizeAsync(new TurnContext(dialogContext.Context.Adapter, activity), cancellationToken);
        }

        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(new RecognizerResult());
        }

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            throw new NotImplementedException();
        }
    }
}
