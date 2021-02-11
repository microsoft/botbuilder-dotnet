// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1118 // Parameter should not span multiple lines
#pragma warning disable SA1210 // namespace order

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class ActionTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public ActionTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(ActionTests));
        }

        [Fact]
        public async Task Action_AttachmentInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_BeginDialog()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_BeginDialogWithActivity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_BeginDialogWithoutActivity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_BeginSkill()
        {
            var middleware = new List<IMiddleware>();
            middleware.Add(new SetSkillConversationIdFactoryBaseMiddleware());
            middleware.Add(new SetSkillBotFrameworkClientMiddleware());
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middlweare: middleware);
        }

        [Fact]
        public async Task Action_BeginSkillEndDialog()
        {
            var middleware = new List<IMiddleware>();
            middleware.Add(new SetSkillConversationIdFactoryBaseMiddleware());
            middleware.Add(new SetSkillBotFrameworkClientMiddleware());
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middlweare: middleware);
        }

        [Fact]
        public async Task Action_CancelDialog()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_CancelDialog_Processed()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_CancelAllDialogs()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_CancelAllDialogs_DoubleCancel()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ChoiceInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ChoiceInput_WithLocale()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ChoicesInMemory()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ChoiceStringInMemory()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ConfirmInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_DatetimeInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_DeleteActivity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_DoActions()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_DynamicBeginDialog()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_EditActionInsertActions()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_EditActionAppendActions()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_EditActionReplaceSequence()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_EmitEvent()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_EndDialog()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_Foreach()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_Foreach_Nested()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_Foreach_Empty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ForeachPage()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ForeachPage_Nested()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ForeachPage_Empty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ForeachPage_Partial()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_GetActivityMembers()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_GetConversationMembers()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_IfCondition()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_NumberInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_NumberInputWithDefaultValue()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_NumberInputWithVAlueExpression()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_NumberInputWithDefaultResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_RepeatDialog()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_RepeatDialogLoop()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ReplaceDialog()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ReplaceDialogRecursive()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SignOutUser()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_Switch()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_Switch_Bool()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_Switch_Default()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_Switch_Number()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_TextInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_TextInputWithInvalidPrompt()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_TextInputWithNonStringInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_TextInputWithValueExpression()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ThrowException()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_TraceActivity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_UpdateActivity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_WaitForInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task InputDialog_ActivityProcessed()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendActivity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SetProperty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SetProperties()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_DeleteProperty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_DeleteProperties()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ReplaceDialogDifferentLevel()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_ReplaceDialogRoot()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_HttpRequest()
        {
            var handler = new MockHttpMessageHandler();
            handler
                .When(HttpMethod.Post, "http://foo.com/")
                .WithContent("Joe is 52")
                .Respond("plain/text", "string");

            handler
                .When(HttpMethod.Post, "http://foo.com/")
                .WithContent("{\r\n  \"text\": \"Joe is 52\",\r\n  \"age\": 52\r\n}".Replace("\r\n", Environment.NewLine))
                .Respond("plain/text", "object");

            handler
                .When(HttpMethod.Post, "http://foo.com/")
                .WithHeaders(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("bound", "52"),
                    new KeyValuePair<string, string>("unbound", "dialog.age")
                })
                .WithContent("[\r\n  {\r\n    \"text\": \"Joe is 52\",\r\n    \"age\": 52\r\n  },\r\n  {\r\n    \"text\": \"text\",\r\n    \"age\": 11\r\n  }\r\n]".Replace("\r\n", Environment.NewLine))
                .Respond("plain/text", "array");

            // Reply with a bytes array and this bytes array would be base64encoded by the sdk
            handler
                .When(HttpMethod.Get, "http://foo.com/image")
                .Respond("image/jpeg", new MemoryStream(System.Text.Encoding.ASCII.GetBytes("TestImage")));

            handler
                .When(HttpMethod.Get, "http://foo.com/json")
                .Respond("application/json", "{\"test\": \"test\"}");

            var messageActivityWithText = Activity.CreateMessageActivity();
            messageActivityWithText.Text = "testtest";
            handler
                .When(HttpMethod.Get, "http://foo.com/activity")
                .Respond("application/vnd.microsoft.activity", JsonConvert.SerializeObject(messageActivityWithText));

            var message1 = Activity.CreateMessageActivity();
            message1.Text = "test1";

            var message2 = Activity.CreateMessageActivity();
            message2.Text = "test2";

            var message3 = Activity.CreateMessageActivity();
            message3.Text = "test3";

            var listOfActivites = new Activity[]
            {
                (Activity)message1,
                (Activity)message2,
                (Activity)message3
            };
            handler
                .When(HttpMethod.Get, "http://foo.com/activities")
                .Respond("application/vnd.microsoft.activities", JsonConvert.SerializeObject(listOfActivites));

            var testAdapter = new TestAdapter()
                .UseStorage(new MemoryStorage())
                .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new MemoryStorage()));

            var rootDialog = new AdaptiveDialog()
            {
                Triggers = new List<Conditions.OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperties()
                            {
                                Assignments = new List<PropertyAssignment>()
                                {
                                    new PropertyAssignment() { Property = "dialog.name", Value = "Joe" },
                                    new PropertyAssignment() { Property = "dialog.age", Value = 52 },
                                }
                            },
                            new HttpRequest()
                            {
                                Url = "http://foo.com/",
                                Method = HttpRequest.HttpMethod.POST,
                                ContentType = "plain/text",
                                Body = "${dialog.name} is ${dialog.age}"
                            },
                            new SendActivity("${turn.lastresult.content}"),
                            new HttpRequest()
                            {
                                Url = "http://foo.com/",
                                Method = HttpRequest.HttpMethod.POST,
                                ContentType = "application/json",
                                Body = JToken.FromObject(new
                                {
                                    text = "${dialog.name} is ${dialog.age}",
                                    age = "=dialog.age"
                                })
                            },
                            new SendActivity("${turn.lastresult.content}"),
                            new HttpRequest()
                            {
                                Url = "http://foo.com/",
                                Method = HttpRequest.HttpMethod.POST,
                                ContentType = "application/json",
                                Headers = new Dictionary<string, AdaptiveExpressions.Properties.StringExpression>()
                                {
                                    { "bound", "=dialog.age" },
                                    { "unbound", "dialog.age" }
                                },
                                Body = JToken.FromObject(new object[]
                                {
                                    new
                                    {
                                        text = "${dialog.name} is ${dialog.age}",
                                        age = "=dialog.age"
                                    },
                                    new
                                    {
                                        text = "text",
                                        age = 11
                                    }
                                })
                            },
                            new SendActivity("${turn.lastresult.content}"),
                            new HttpRequest()
                            {
                                Url = "http://foo.com/image",
                                Method = HttpRequest.HttpMethod.GET,
                                ResponseType = HttpRequest.ResponseTypes.Binary
                            },
                            new SendActivity("${turn.lastresult.content}"),
                            new HttpRequest()
                            {
                                Url = "http://foo.com/json",
                                Method = HttpRequest.HttpMethod.GET,
                                ResponseType = HttpRequest.ResponseTypes.Json
                            },
                            new SendActivity("${turn.lastresult.content.test}"),
                            new HttpRequest()
                            {
                                Url = "http://foo.com/activity",
                                Method = HttpRequest.HttpMethod.GET,
                                ResponseType = HttpRequest.ResponseTypes.Activity
                            },
                            new HttpRequest()
                            {
                                Url = "http://foo.com/activities",
                                Method = HttpRequest.HttpMethod.GET,
                                ResponseType = HttpRequest.ResponseTypes.Activities
                            },
                            new SendActivity("done")
                        }
                    }
                }
            };

            var dm = new DialogManager(rootDialog)
                .UseResourceExplorer(new ResourceExplorer())
                .UseLanguageGeneration();
            dm.InitialTurnState.Set<HttpClient>(handler.ToHttpClient());

            await new TestFlow((TestAdapter)testAdapter, dm.OnTurnAsync)
                .SendConversationUpdate()
                    .AssertReply("string")
                    .AssertReply("object")
                    .AssertReply("array")
                    .AssertReply("VGVzdEltYWdl")
                    .AssertReply("test")
                    .AssertReply("testtest")
                    .AssertReply("test1")
                    .AssertReply("test2")
                    .AssertReply("test3")
                    .AssertReply("done")
                .StartTestAsync();
        }

        [Fact]
        public async Task Action_TelemetryTrackEvent()
        {
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();

            var testAdapter = new TestAdapter()
                .UseStorage(new MemoryStorage())
                .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new MemoryStorage()));

            var rootDialog = new AdaptiveDialog
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new TelemetryTrackEventAction("testEvent")
                            {
                                Properties =
                                    new Dictionary<string, AdaptiveExpressions.Properties.
                                        StringExpression>()
                                    {
                                        { "prop1", "value1" },
                                        { "prop2", "value2" }
                                    }
                            },
                        }
                    }
                },
                TelemetryClient = mockTelemetryClient.Object
            };

            var dm = new DialogManager(rootDialog)
                .UseResourceExplorer(new ResourceExplorer())
                .UseLanguageGeneration();

            await new TestFlow((TestAdapter)testAdapter, dm.OnTurnAsync)
                .SendConversationUpdate()
                .StartTestAsync();

            var testEventInvocation = mockTelemetryClient.Invocations.FirstOrDefault(i => i.Arguments[0]?.ToString() == "testEvent");

            Assert.NotNull(testEventInvocation);
            Assert.Equal(2, ((Dictionary<string, string>)testEventInvocation.Arguments[1]).Count);
            Assert.Equal("value1", ((Dictionary<string, string>)testEventInvocation.Arguments[1])["prop1"]);
            Assert.Equal("value2", ((Dictionary<string, string>)testEventInvocation.Arguments[1])["prop2"]);
        }

        private class SetSkillConversationIdFactoryBaseMiddleware : IMiddleware
        {
            public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
            {
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    var mockSkillConversationIdFactoryBase = new Mock<SkillConversationIdFactoryBase>();
                    mockSkillConversationIdFactoryBase.SetupAllProperties();
                    turnContext.TurnState.Add(mockSkillConversationIdFactoryBase.Object);
                }

                await next(cancellationToken).ConfigureAwait(false);
            }
        }

        private class SetSkillBotFrameworkClientMiddleware : IMiddleware
        {
            /// <inheritdoc/>
            public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
            {
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    turnContext.TurnState.Add<BotFrameworkClient>(new MockSkillBotFrameworkClient());
                }

                await next(cancellationToken).ConfigureAwait(false);
            }
        }

        private class MockSkillBotFrameworkClient : BotFrameworkClient
        {
            public override Task<InvokeResponse<T>> PostActivityAsync<T>(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken = default)
            {
                var responseActivity = activity;

                if (activity.Text.Contains("skill"))
                {
                    responseActivity = new Activity()
                    {
                        Type = "message",
                        Text = "This is the skill talking: hello"
                    };
                }

                if (activity.Text.Contains("end"))
                {
                    responseActivity = new Activity()
                    {
                        Type = "endOfConversation"
                    };
                }

                var response = new InvokeResponse<ExpectedReplies>()
                {
                    Status = 200,
                    Body = new ExpectedReplies
                    {
                        Activities = new List<Activity>()
                    {
                        responseActivity
                    }
                    }
                };

                var casted = (InvokeResponse<T>)Convert.ChangeType(response, typeof(InvokeResponse<T>), new System.Globalization.CultureInfo("en-US"));
                var result = Task.FromResult(casted);
                return result;
            }

            /// <inheritdoc/>
            /// <remarks>Not implemented.</remarks>
            public override Task<InvokeResponse> PostActivityAsync(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
    }
}
