// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class ActionTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task Action_WaitForInput()
        {
            var testDialog = new AdaptiveDialog("planningTest");

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new TextInput() { Prompt = new ActivityTemplate("Hello, what is your name?"), Property = "user.name" },
                        new SendActivity("Hello {user.name}, nice to meet you!"),
                    })
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

#if needsmoq
        [TestMethod]
        public async Task Action_HttpRequest()
        {
            var testDialog = new AdaptiveDialog("planningTest");

            testDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<Dialog>()
                    {
                        new HttpRequest()
                        {
                            Url = "https://translatorbotfn.azurewebsites.net/api/TranslateMessage?code=DFgQcuVqFkvP0nL27sibggKfMGYM8/fjNCZWywCwR1mbXclw0uSk3A==",
                            Method = HttpRequest.HttpMethod.POST,
                            ResponseType = HttpRequest.ResponseTypes.Activity,
                            Body = JObject.FromObject(new
                                {
                                    text = "Azure",
                                    fromLocale ="english",
                                    toLocale = "italian"
                                })
                        },
                    })
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply(reply =>
                {
                    var message = reply.AsMessageActivity();
                    Assert.AreEqual(1, message.Attachments.Count);
                })
            .StartTestAsync();
        }
#endif
        [TestMethod]
        public async Task Action_TraceActivity()
        {
            var dialog = new AdaptiveDialog("traceActivity");

            dialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new SetProperty()
                        {
                             Property = "user.name",
                             Value = "'frank'"
                        },
                        new TraceActivity()
                        {
                            Name = "test",
                            ValueType = "user",
                            Value = "user"
                        },
                        new TraceActivity()
                        {
                            Name = "test",
                            ValueType = "memory"
                        }
                    })
            });

            await CreateFlow(dialog, sendTrace: true)
            .Send("hi")
                .AssertReply((activity) =>
                {
                    var traceActivity = (ITraceActivity)activity;
                    Assert.AreEqual(ActivityTypes.Trace, traceActivity.Type, "type doesn't match");
                    Assert.AreEqual("user", traceActivity.ValueType, "ValueType doesn't match");
                    dynamic value = traceActivity.Value;
                    Assert.AreEqual("frank", (string)value["name"], "Value doesn't match");
                })
                .AssertReply((activity) =>
                {
                    var traceActivity = (ITraceActivity)activity;
                    Assert.AreEqual(ActivityTypes.Trace, traceActivity.Type, "type doesn't match");
                    Assert.AreEqual("memory", traceActivity.ValueType, "ValueType doesn't match");
                    dynamic value = traceActivity.Value;
                    Assert.AreEqual("frank", (string)value["user"]["name"], "Value doesn't match");
                })
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_IfCondition()
        {
            var testDialog = new AdaptiveDialog("planningTest");
            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new IfCondition()
                        {
                            Condition = "!dialog.foo && user.name == null",
                            Actions = new List<Dialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    Property = "user.name"
                                },
                                new SendActivity("Hello {user.name}, nice to meet you!")
                            },
                            ElseActions = new List<Dialog>()
                            {
                                new SendActivity("Hello {user.name}, nice to see you again!")
                            }
                        },
                    })
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("hi")
                .AssertReply("Hello Carlos, nice to see you again!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_Switch()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                            {
                                new SetProperty()
                                {
                                    Property = "user.name",
                                    Value = "'frank'"
                                },
                                new SwitchCondition()
                                {
                                    Condition = "user.name",
                                    Cases = new List<Case>()
                                    {
                                        new Case("susan", new List<Dialog>() { new SendActivity("hi susan") }),
                                        new Case("bob", new List<Dialog>() { new SendActivity("hi bob") }),
                                        new Case("frank", new List<Dialog>() { new SendActivity("hi frank") })
                                    },
                                    Default = new List<Dialog>() { new SendActivity("Who are you?") }
                                },
                            }
                    }
                }
            };

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("hi frank")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_Switch_Default()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "user.name",
                                Value = "'Zoidberg'"
                            },
                            new SwitchCondition()
                            {
                                Condition = "user.name",
                                Cases = new List<Case>()
                                {
                                    new Case("susan", new List<Dialog>() { new SendActivity("hi susan") }),
                                    new Case("bob", new List<Dialog>() { new SendActivity("hi bob") }),
                                    new Case("frank", new List<Dialog>() { new SendActivity("hi frank") })
                                },
                                Default = new List<Dialog>() { new SendActivity("Who are you?") }
                            },
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Who are you?")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_Switch_Number()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "user.age",
                                Value = "22"
                            },
                            new SwitchCondition()
                            {
                                Condition = "user.age",
                                Cases = new List<Case>()
                                {
                                    new Case("21", new List<Dialog>() { new SendActivity("Age is 21") }),
                                    new Case("22", new List<Dialog>() { new SendActivity("Age is 22") }),
                                    new Case("23", new List<Dialog>() { new SendActivity("Age is 23") })
                                },
                                Default = new List<Dialog>() { new SendActivity("Who are you?") }
                            },
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Age is 22")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_Switch_Bool()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "user.isVip",
                                Value = "true"
                            },
                            new SwitchCondition()
                            {
                                Condition = "user.isVip",
                                Cases = new List<Case>()
                                {
                                    new Case("True", new List<Dialog>() { new SendActivity("User is VIP") }),
                                    new Case("False", new List<Dialog>() { new SendActivity("User is NOT VIP") })
                                },
                                Default = new List<Dialog>() { new SendActivity("Who are you?") }
                            },
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("User is VIP")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_TextInput()
        {
            var testDialog = new AdaptiveDialog("planningTest");

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new IfCondition()
                        {
                            Condition = "user.name == null",
                            Actions = new List<Dialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    UnrecognizedPrompt = new ActivityTemplate("How should I call you?"),
                                    Property = "user.name",
                                    Validations = new List<string>()
                                    {
                                        "this.value.Length > 3"
                                    }
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("c")
                .AssertReply("How should I call you?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_NumberInputWithDefaultValue()
        {
            var testDialog = new AdaptiveDialog("planningTest");

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent()
                {
                    Actions = new List<Dialog>()
                    {
                        new NumberInput()
                        {
                            MaxTurnCount = 1,
                            DefaultValue = "10",
                            Prompt = new ActivityTemplate("What is your age?"),
                            Property = "turn.age"
                        },
                        new SendActivity("You said {turn.age}")
                    }
                }
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("What is your age?")
            .Send("hi")
                .AssertReply("You said 10")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_ConfirmInput()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new ConfirmInput()
                            {
                                Prompt = new ActivityTemplate("yes or no"),
                                UnrecognizedPrompt = new ActivityTemplate("I need a yes or no."),
                                Property = "user.confirmed"
                            },
                            new SendActivity("confirmation: {user.confirmed}"),
                            new ConfirmInput()
                            {
                                Prompt = new ActivityTemplate("yes or no"),
                                UnrecognizedPrompt = new ActivityTemplate("I need a yes or no."),
                                Property = "user.confirmed",
                                AlwaysPrompt = true
                            },
                            new SendActivity("confirmation: {user.confirmed}"),
                            new ConfirmInput()
                            {
                                Prompt = new ActivityTemplate("yes or no"),
                                UnrecognizedPrompt = new ActivityTemplate("I need a yes or no."),
                                Property = "user.confirmed",
                                AlwaysPrompt = true
                            },
                            new SendActivity("confirmation: {user.confirmed}"),
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("yes or no (1) Yes or (2) No")
            .Send("asdasd")
                .AssertReply("I need a yes or no. (1) Yes or (2) No")
            .Send("yes")
                .AssertReply("confirmation: True")
                .AssertReply("yes or no (1) Yes or (2) No")
            .Send("nope")
                .AssertReply("confirmation: False")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_ChoiceInput()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Not a color. Please select a color:"),
                                Choices = new List<Choice>() { new Choice("red"), new Choice("green"), new Choice("blue") },
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Please select a color:"),
                                Choices = new List<Choice>() { new Choice("red"), new Choice("green"), new Choice("blue") },
                                AlwaysPrompt = true,
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Please select a color:"),
                                Choices = new List<Choice>() { new Choice("red"), new Choice("green"), new Choice("blue") },
                                AlwaysPrompt = true,
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send("asdasd")
                .AssertReply("Not a color. Please select a color: (1) red, (2) green, or (3) blue")
            .Send("blue")
                .AssertReply("blue")
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send("red")
                .AssertReply("red")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_ChoiceInput_WithLocale()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Not a color. Please select a color:"),
                                Choices = new List<Choice>() { new Choice("red"), new Choice("green"), new Choice("blue") },
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Please select a color:"),
                                Choices = new List<Choice>() { new Choice("red"), new Choice("green"), new Choice("blue") },
                                AlwaysPrompt = true,
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Please select a color:"),
                                Choices = new List<Choice>() { new Choice("red"), new Choice("green"), new Choice("blue") },
                                AlwaysPrompt = true,
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
            .Send(BuildMessageActivityWithLocale("hi", "en-US"))
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send(BuildMessageActivityWithLocale("asdasd", "EN-US"))
                .AssertReply("Not a color. Please select a color: (1) red, (2) green, or (3) blue")
            .Send(BuildMessageActivityWithLocale("blue", "en-us"))
                .AssertReply("blue")
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send("red")
                .AssertReply("red")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_ChoiceStringInMemory()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Value = "json('[\"red\", \"green\", \"blue\"]')",
                                Property = "user.choices"
                            },
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Not a color. Please select a color:"),
                                ChoicesProperty = "user.choices",
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Please select a color:"),
                                ChoicesProperty = "user.choices",
                                AlwaysPrompt = true,
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Please select a color:"),
                                ChoicesProperty = "user.choices",
                                AlwaysPrompt = true,
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send("asdasd")
                .AssertReply("Not a color. Please select a color: (1) red, (2) green, or (3) blue")
            .Send("3")
                .AssertReply("blue")
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send("red")
                .AssertReply("red")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_ChoicesInMemory()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Value = "json('[{\"value\": \"red\"}, {\"value\": \"green\"}, {\"value\": \"blue\"}]')",
                                Property = "user.choices"
                            },
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Not a color. Please select a color:"),
                                ChoicesProperty = "user.choices",
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Please select a color:"),
                                ChoicesProperty = "user.choices",
                                AlwaysPrompt = true,
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                            new ChoiceInput()
                            {
                                Property = "user.color",
                                Prompt = new ActivityTemplate("Please select a color:"),
                                UnrecognizedPrompt = new ActivityTemplate("Please select a color:"),
                                ChoicesProperty = "user.choices",
                                AlwaysPrompt = true,
                                Style = ListStyle.Inline
                            },
                            new SendActivity("{user.color}"),
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send("asdasd")
                .AssertReply("Not a color. Please select a color: (1) red, (2) green, or (3) blue")
            .Send("3")
                .AssertReply("blue")
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send("red")
                .AssertReply("red")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_NumberInput()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false
            };

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new NumberInput()
                        {
                            Prompt = new ActivityTemplate("Please enter your age."),
                            UnrecognizedPrompt = new ActivityTemplate("The value entered must be greater than 0 and less than 150."),
                            Property = "user.userProfile.Age",
                            OutputFormat = NumberOutputFormat.Integer,
                            Validations = new List<string>()
                            {
                                "this.value > 0 && this.value < 150"
                            }
                        },
                        new SendActivity("I have your age as {user.userProfile.Age}."),
                    })
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Please enter your age.")
            .Send("1000")
                .AssertReply("The value entered must be greater than 0 and less than 150.")
            .Send("15.3")
                .AssertReply("I have your age as 15.")
            .Send("hi")
                .AssertReply("I have your age as 15.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_DatetimeInput()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false
            };

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new DateTimeInput()
                        {
                            Prompt = new ActivityTemplate("Please enter a date."),
                            Property = "user.date",
                        },
                        new SendActivity("You entered {user.date[0].Value}"),
                    })
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Please enter a date.")
            .Send("June 1st 2019")
                .AssertReply("You entered 2019-06-01")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_TextInputWithInvalidPrompt()
        {
            var testDialog = new AdaptiveDialog("planningTest");

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new IfCondition()
                        {
                            Condition = "user.name == null",
                            Actions = new List<Dialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    UnrecognizedPrompt = new ActivityTemplate("How should I call you?"),
                                    InvalidPrompt = new ActivityTemplate("That does not soud like a name"),
                                    Property = "user.name",
                                    Validations = new List<string>()
                                    {
                                        "this.value.Length > 3"
                                    }
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("c")
                .AssertReply("That does not soud like a name")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_EditActionReplaceSequence()
        {
            var testDialog = new AdaptiveDialog("planningTest")
            {
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Replace", "(?i)replace"),
                    }
                }
            };

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent()
                {
                    Actions = new List<Dialog>()
                    {
                        new TextInput()
                        {
                            Prompt = new ActivityTemplate("Say replace to replace these actions"),
                            Property = "turn.tempInput"
                        },
                        new SendActivity("You should not see this step if you said replace"),
                        new RepeatDialog()
                    }
                },
                new OnIntent()
                {
                    Intent = "Replace",
                    Actions = new List<Dialog>()
                    {
                        new SendActivity("I'm going to replace the original actions via EditActions"),
                        new EditActions()
                        {
                            ChangeType = ActionChangeType.ReplaceSequence,
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("New actions..."),
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("What's your name?"),
                                    Property = "turn.tempInput"
                                }
                            }
                        }
                    }
                }
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Say replace to replace these actions")
            .Send("replace")
                .AssertReply("I'm going to replace the original actions via EditActions")
                .AssertReply("New actions...")
                .AssertReply("What's your name?")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_DoActions()
        {
            var testDialog = new AdaptiveDialog("planningTest");

            testDialog.Recognizer = new RegexRecognizer() { Intents = new List<IntentPattern>() { new IntentPattern("JokeIntent", "joke") } };

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnIntent(
                    "JokeIntent",
                    actions: new List<Dialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }),
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new IfCondition()
                        {
                            Condition = "user.name == null",
                            Actions = new List<Dialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    Property = "user.name"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })
            });

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("Do you know a joke?")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_BeginDialog()
        {
            var tellJokeDialog = new AdaptiveDialog("TellJokeDialog")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Why did the chicken cross the road?"),
                            new EndTurn(),
                            new SendActivity("To get to the other side")
                        },
                    }
                }
            };

            var askNameDialog = new AdaptiveDialog("AskNameDialog")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "user.name == null",
                                Actions = new List<Dialog>()
                                {
                                    new TextInput()
                                    {
                                        Prompt = new ActivityTemplate("Hello, what is your name?"),
                                        Property = "user.name"
                                    }
                                }
                            },
                            new SendActivity("Hello {user.name}, nice to meet you!")
                        }
                    }
                }
            };

            var testDialog = new AdaptiveDialog("planningTest");
            testDialog.AutoEndDialog = false;

            testDialog.Recognizer = new RegexRecognizer() { Intents = new List<IntentPattern>() { new IntentPattern("JokeIntent", "joke") } };

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnBeginDialog()
                {
                    Actions = new List<Dialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'"),
                        new BeginDialog(askNameDialog.Id)
                    }
                },
                new OnIntent(
                    "JokeIntent",
                    actions: new List<Dialog>()
                    {
                        new BeginDialog() { Dialog = tellJokeDialog }
                    }),
                new OnUnknownIntent(
                    actions: new List<Dialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
            });
            testDialog.Dialogs.Add(askNameDialog);

            await CreateFlow(testDialog)
            .SendConversationUpdate()
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("Cool")
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_ReplaceDialog()
        {
            var tellJokeDialog = new AdaptiveDialog("TellJokeDialog");
            tellJokeDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    })
            });

            var askNameDialog = new AdaptiveDialog("AskNameDialog")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "user.name == null",
                                Actions = new List<Dialog>()
                                {
                                    new TextInput()
                                    {
                                        Prompt = new ActivityTemplate("Hello, what is your name?"),
                                        UnrecognizedPrompt = new ActivityTemplate("How should I call you?"),
                                        InvalidPrompt = new ActivityTemplate("That does not soud like a name"),
                                        Property = "user.name",
                                    }
                                }
                            },
                            new SendActivity("Hello {user.name}, nice to meet you!")
                        }
                    }
                }
            };

            var testDialog = new AdaptiveDialog("planningTest");
            testDialog.AutoEndDialog = false;
            testDialog.Recognizer = new RegexRecognizer() { Intents = new List<IntentPattern>() { new IntentPattern("JokeIntent", "joke") } };

            testDialog.Triggers.Add(new OnBeginDialog()
            {
                Actions = new List<Dialog>()
                {
                    new SendActivity("I'm a joke bot. To get started say 'tell me a joke'"),
                    new ReplaceDialog("AskNameDialog")
                }
            });

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnIntent(
                    "JokeIntent",
                    actions: new List<Dialog>()
                    {
                        new ReplaceDialog("TellJokeDialog")
                    }),
            });

            testDialog.Dialogs.Add(tellJokeDialog);
            testDialog.Dialogs.Add(askNameDialog);

            await CreateFlow(testDialog)
            .SendConversationUpdate()
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("Do you know a joke?")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_EndDialog()
        {
            var testDialog = new AdaptiveDialog("planningTest");

            testDialog.Recognizer = new RegexRecognizer() { Intents = new List<IntentPattern>() { new IntentPattern("EndIntent", "end") } };

            var tellJokeDialog = new AdaptiveDialog("TellJokeDialog");
            tellJokeDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnIntent(
                    "EndIntent",
                    actions: new List<Dialog>()
                    {
                        new EndDialog()
                    }),
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    })
            });
            tellJokeDialog.Recognizer = new RegexRecognizer() { Intents = new List<IntentPattern>() { new IntentPattern("EndIntent", "end") } };

            testDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new BeginDialog(tellJokeDialog.Id),
                        new SendActivity("You went out from ask name dialog.")
                    })
            });

            testDialog.Dialogs.Add(tellJokeDialog);

            await CreateFlow(testDialog)
            .Send("hi")
                .AssertReply("Why did the chicken cross the road?")
            .Send("end")
                .AssertReply("You went out from ask name dialog.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_RepeatDialog()
        {
            var testDialog = new AdaptiveDialog("testDialog")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new TextInput() { Prompt = new ActivityTemplate("Hello, what is your name?"), Property = "user.name" },
                            new SendActivity("Hello {user.name}, nice to meet you!"),
                            new EndTurn(),
                            new RepeatDialog()
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
                .Send("hi")
                    .AssertReply("Hello, what is your name?")
                .Send("Carlos")
                    .AssertReply("Hello Carlos, nice to meet you!")
                .Send("hi")
                    .AssertReply("Hello Carlos, nice to meet you!")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_EmitEvent()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var outer = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("EmitIntent", "emit"),
                        new IntentPattern("CowboyIntent", "moo")
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent(intent: "CowboyIntent")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Yippee ki-yay!")
                        }
                    },
                    new OnIntent(intent: "EmitIntent")
                    {
                        Actions = new List<Dialog>()
                        {
                            new EmitEvent()
                            {
                                EventName = "CustomEvent",
                                BubbleEvent = true,
                            }
                        }
                    }
                }
            };

            var rootDialog = new AdaptiveDialog("root")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog(outer.Id)
                        }
                    },

                    new OnCustomEvent()
                    {
                        Event = "CustomEvent",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("CustomEventFired")
                        }
                    }
                }
            };
            rootDialog.Dialogs.Add(outer);

            await CreateFlow(rootDialog)
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .Send("emit")
                .AssertReply("CustomEventFired")
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_Foreach()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var rootDialog = new AdaptiveDialog("root")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new InitProperty()
                            {
                                Property = "dialog.todo",
                                Type = "Array"
                            },

                            new EditArray()
                            {
                                ItemsProperty = "dialog.todo",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "1"
                            },

                            new EditArray()
                            {
                                ItemsProperty = "dialog.todo",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "2"
                            },

                            new EditArray()
                            {
                                ItemsProperty = "dialog.todo",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "3"
                            },

                            new Foreach()
                            {
                                ItemsProperty = "dialog.todo",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("index is: {dialog.foreach.index} and value is: {dialog.foreach.value}")
                                }
                            }
                        }
                    }
                }
            };

            await CreateFlow(rootDialog)
            .Send("hi")
                .AssertReply("index is: 0 and value is: 1")
                .AssertReply("index is: 1 and value is: 2")
                .AssertReply("index is: 2 and value is: 3")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Action_ForeachPage()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var rootDialog = new AdaptiveDialog("root")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new InitProperty()
                            {
                                Property = "dialog.todo",
                                Type = "Array"
                            },

                            new EditArray()
                            {
                                ItemsProperty = "dialog.todo",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "1"
                            },

                            new EditArray()
                            {
                                ItemsProperty = "dialog.todo",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "2"
                            },

                            new EditArray()
                            {
                                ItemsProperty = "dialog.todo",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "3"
                            },

                            new EditArray()
                            {
                                ItemsProperty = "dialog.todo",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "4"
                            },

                            new EditArray()
                            {
                                ItemsProperty = "dialog.todo",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "5"
                            },

                            new EditArray()
                            {
                                ItemsProperty = "dialog.todo",
                                ChangeType = EditArray.ArrayChangeType.Push,
                                Value = "6"
                            },

                            new ForeachPage()
                            {
                                ItemsProperty = "dialog.todo",
                                PageSize = 3,
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("This page have 3 items"),
                                    new Foreach()
                                    {
                                        ItemsProperty = "dialog.foreach.page",
                                        Actions = new List<Dialog>()
                                        {
                                            new SendActivity("index is: {dialog.foreach.index} and value is: {dialog.foreach.value}")
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            await CreateFlow(rootDialog)
            .Send("hi")
                .AssertReply("This page have 3 items")
                .AssertReply("index is: 0 and value is: 1")
                .AssertReply("index is: 1 and value is: 2")
                .AssertReply("index is: 2 and value is: 3")
                .AssertReply("This page have 3 items")
                .AssertReply("index is: 0 and value is: 4")
                .AssertReply("index is: 1 and value is: 5")
                .AssertReply("index is: 2 and value is: 6")
            .StartTestAsync();
        }

        private static IActivity BuildMessageActivityWithLocale(string text, string locale)
        {
            return new Activity()
            {
                Type = ActivityTypes.Message,
                Text = text,
                Locale = locale
            };
        }

        private TestFlow CreateFlow(AdaptiveDialog testDialog, bool sendTrace = false)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
            var resourceExplorer = new ResourceExplorer();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName), sendTrace);
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseResourceExplorer(resourceExplorer)
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));
            DialogManager dm = new DialogManager(testDialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }
}
