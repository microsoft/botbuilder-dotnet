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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.V3Bridge.Dialogs;
using Microsoft.Bot.Builder.V3Bridge.Internals.Fibers;
using Microsoft.Bot.Builder.V3Bridge.Luis;
using Microsoft.Bot.Builder.V3Bridge.Luis.Models;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Action = Microsoft.Bot.Builder.V3Bridge.Luis.Models.Action;

namespace Microsoft.Bot.Builder.V3Bridge.Tests
{
    public abstract class LuisTestBase : DialogTestBase
    {
        public static IntentRecommendation[] IntentsFor<D>(Expression<Func<D, Task>> expression, double? score)
        {
            var body = (MethodCallExpression)expression.Body;
            var attributes = body.Method.GetCustomAttributes<LuisIntentAttribute>();
            var intents = attributes
                .Select(attribute => new IntentRecommendation(attribute.IntentName, score))
                .ToArray();
            return intents;
        }

        public static EntityRecommendation EntityFor(string type, string entity, IDictionary<string, object> resolution = null)
        {
            return new EntityRecommendation(type: type) { Entity = entity, Resolution = resolution };
        }

        public static EntityRecommendation EntityForDate(string type, DateTime date)
        {
            return EntityFor(type,
                date.ToString("d", DateTimeFormatInfo.InvariantInfo),
                new Dictionary<string, object>()
                {
                    { "resolution_type", "builtin.datetime.date" },
                    { "date", date.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo) }
                });
        }

        public static EntityRecommendation EntityForTime(string type, DateTime time)
        {
            return EntityFor(type,
                time.ToString("t", DateTimeFormatInfo.InvariantInfo),
                new Dictionary<string, object>()
                {
                    { "resolution_type", "builtin.datetime.time" },
                    { "time", time.ToString("THH:mm:ss", DateTimeFormatInfo.InvariantInfo) }
                });
        }

        public static void SetupLuis<D>(
            Mock<ILuisService> luis,
            Expression<Func<D, Task>> expression,
            double? score,
            params EntityRecommendation[] entities
            )
        {
            luis
                .Setup(l => l.QueryAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LuisResult()
                {
                    Intents = IntentsFor(expression, score),
                    Entities = entities
                });
        }

        public static void SetupLuis<D>(
            Mock<ILuisService> luis,
            string utterance,
            Expression<Func<D, Task>> expression,
            double? score,
            params EntityRecommendation[] entities
            )
        {
            var uri = new UriBuilder() { Query = utterance }.Uri;
            luis
                .Setup(l => l.BuildUri(It.Is<LuisRequest>(r => r.Query == utterance)))
                .Returns(uri);

            luis.Setup(l => l.ModifyRequest(It.IsAny<LuisRequest>()))
                .Returns<LuisRequest>(r => r);

            luis
                .Setup(l => l.QueryAsync(uri, It.IsAny<CancellationToken>()))
                .Returns<Uri, CancellationToken>(async (_, token) =>
                {
                    return new LuisResult()
                    {
                        Intents = IntentsFor(expression, score),
                        Entities = entities
                    };
                });
        }
    }

    [TestClass]
    public sealed class LuisTests : LuisTestBase
    {
        public sealed class DerivedLuisDialog : LuisDialog<object>
        {
            public DerivedLuisDialog(params ILuisService[] services)
                : base(services)
            {
            }

            [LuisIntent("PublicHandlerWithAttribute")]
            public Task PublicHandlerWithAttribute(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            [LuisIntent("PublicHandlerWithAttribute")]
            public Task PublicHandlerWithAttribute(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            [LuisIntent("PrivateHandlerWithAttribute")]
            public Task PrivateHandlerWithAttribute(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            [LuisIntent("PrivateHandlerWithAttribute")]
            public Task PrivateHandlerWithAttribute(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            [LuisIntent("PublicHandlerWithAttributeOne")]
            [LuisIntent("PublicHandlerWithAttributeTwo")]
            public Task PublicHandlerWithTwoAttributes(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            [LuisIntent("PublicHandlerWithAttributeOne")]
            [LuisIntent("PublicHandlerWithAttributeTwo")]
            public Task PublicHandlerWithTwoAttributes(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            private Task PublicHandlerWithNoAttribute(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            private Task PublicHandlerWithNoAttribute(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            private Task PrivateHandlerWithNoAttribute(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            private Task PrivateHandlerWithNoAttribute(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            public Task PublicHandlerWithCovariance(IDialogContext context, object luisResult)
            {
                throw new NotImplementedException();
            }

            public Task PublicHandlerWithCovariance(IDialogContext context, IAwaitable<IMessageActivity> activity, object luisResult)
            {
                throw new NotImplementedException();
            }

            public void DoesNotMatchReturnType(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            public void DoesNotMatchArgumentType(IDialogContext context, int notLuisResult)
            {
                throw new NotImplementedException();
            }

            private Task ThrowsEvenOnFalseThrowOnBindFailure<T>(T x) where T : IDialog
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void All_Handlers_Are_Found()
        {
            var service = new Mock<ILuisService>();
            var dialog = new DerivedLuisDialog(service.Object);
            var handlers = LuisDialog.EnumerateHandlers(dialog).ToArray();
            Assert.AreEqual(14, handlers.Length);
        }

        [Serializable]
        public sealed class MultiServiceLuisDialog : LuisDialog<object>
        {
            public MultiServiceLuisDialog(params ILuisService[] services)
                : base(services)
            {
            }

            [LuisIntent("ServiceOne")]
            public async Task ServiceOne(IDialogContext context, LuisResult luisResult)
            {
                await context.PostAsync(luisResult.Entities.Single().Type);
                context.Wait(MessageReceived);
            }

            [LuisIntent("ServiceTwo")]
            public async Task ServiceTwo(IDialogContext context, LuisResult luisResult)
            {
                await context.PostAsync(luisResult.Entities.Single().Type);
                context.Wait(MessageReceived);
            }

            [LuisIntent("IntentOne")]
            public async Task IntentOne(IDialogContext context, LuisResult luisResult)
            {
                await context.PostAsync(luisResult.Intents.Single().Actions.Single().Name);
                context.Wait(MessageReceived);
            }

        }

        [TestMethod]
        public async Task All_Services_Are_Called()
        {
            var service1 = new Mock<ILuisService>();
            var service2 = new Mock<ILuisService>();

            var dialog = new MultiServiceLuisDialog(service1.Object, service2.Object);

            using (new FiberTestBase.ResolveMoqAssembly(service1.Object, service2.Object))
            using (var container = Build(Options.ResolveDialogFromContainer, service1.Object, service2.Object))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(dialog)
                    .As<IDialog<object>>();
                builder.Update(container);

                const string EntityOne = "one";
                const string EntityTwo = "two";

                SetupLuis<MultiServiceLuisDialog>(service1, d => d.ServiceOne(null, null), 1.0, new EntityRecommendation(type: EntityOne));
                SetupLuis<MultiServiceLuisDialog>(service2, d => d.ServiceTwo(null, null), 0.0, new EntityRecommendation(type: EntityTwo));

                await AssertScriptAsync(container, "hello", EntityOne);

                SetupLuis<MultiServiceLuisDialog>(service1, d => d.ServiceOne(null, null), 0.0, new EntityRecommendation(type: EntityOne));
                SetupLuis<MultiServiceLuisDialog>(service2, d => d.ServiceTwo(null, null), 1.0, new EntityRecommendation(type: EntityTwo));

                await AssertScriptAsync(container, "hello", EntityTwo);
            }
        }

        [Serializable]
        public sealed class NullMessageTextLuisDialog : LuisDialog<object>
        {
            public NullMessageTextLuisDialog(params ILuisService[] services)
                : base(services)
            {
            }

            [LuisIntent("")]
            public async Task NullHandler(IDialogContext context, LuisResult luisResult)
            {
                await context.PostAsync("I see null");
                context.Wait(MessageReceived);
            }
        }

        [TestMethod]
        public async Task NullMessageText_Is_EmptyIntent()
        {
            var service = new Mock<ILuisService>();

            var dialog = new NullMessageTextLuisDialog(service.Object);

            using (new FiberTestBase.ResolveMoqAssembly(service.Object))
            using (var container = Build(Options.ResolveDialogFromContainer, service.Object))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(dialog)
                    .As<IDialog<object>>();
                builder.Update(container);

                await AssertScriptAsync(container, null, "I see null");
            }
        }

        [TestMethod]
        public void NullOrEmptyIntents_DefaultsTo_TopScoringIntent()
        {
            var intent = new IntentRecommendation();
            var result = new LuisResult()
            {
                TopScoringIntent = intent
            };

            LuisService.Fix(result);

            Assert.AreEqual(1, result.Intents.Count);
            Assert.AreEqual(intent, result.Intents[0]);
        }

        [TestMethod]
        public async Task Service_With_LuisActionDialog()
        {
            var service = new Mock<ILuisService>(MockBehavior.Strict);
            var contextId = "test";
            var intent = "IntentOne";
            var prompt = "ParamOne?";
            var action = "IntentOneAction";

            service
                .Setup(l => l.BuildUri(It.IsAny<LuisRequest>()))
                .Returns<LuisRequest>(request =>
                        request.BuildUri(new LuisModelAttribute("model", "subs", LuisApiVersion.V2))
                );

            service
                .Setup(l => l.ModifyRequest(It.IsAny<LuisRequest>()))
                .Returns<LuisRequest>(r => r);

            service
                .Setup(l => l.QueryAsync(It.Is<Uri>(t => t.AbsoluteUri.Contains($"&contextId={contextId}")), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LuisResult()
                {
                    Intents = new List<IntentRecommendation>()
                    {
                        new IntentRecommendation()
                        {
                            Intent = intent,
                            Score =  1.0,
                            Actions =  new List<Action>
                            {
                                new Action
                                {
                                    Triggered = true,
                                    Name = action,
                                    Parameters = new List<ActionParameter>()
                                    {
                                        new ActionParameter()
                                        {
                                            Name = "ParamOne",
                                            Required = true,
                                            Value = new List<EntityRecommendation>()
                                            {
                                                new EntityRecommendation
                                                {
                                                    Type = "EntityOne",
                                                    Score = 1.0
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Entities = new List<EntityRecommendation>()
                    {
                        new EntityRecommendation
                        {
                            Type = "EntityOne",
                            Score = 1.0
                        }
                    },
                    Dialog = new DialogResponse()
                    {
                        ContextId = contextId,
                        Status = DialogResponse.DialogStatus.Finished
                    }
                });

            service
                .Setup(
                    l => l.QueryAsync(It.Is<Uri>(t => t.AbsoluteUri.Contains("q=start")), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LuisResult
                {
                    TopScoringIntent = new IntentRecommendation
                    {
                        Intent = intent,
                        Score = 1.0
                    },
                    Dialog = new DialogResponse
                    {
                        ContextId = contextId,
                        Status = DialogResponse.DialogStatus.Question,
                        Prompt = prompt
                    }
                });


            var dialog = new MultiServiceLuisDialog(service.Object);
            using (new FiberTestBase.ResolveMoqAssembly(service.Object))
            using (var container = Build(Options.ResolveDialogFromContainer, service.Object))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(dialog)
                    .As<IDialog<object>>();
                builder.Update(container);

                await AssertScriptAsync(container, "start", prompt, "EntityOne", action);
            }
        }

        public sealed class InvalidLuisDialog : LuisDialog<object>
        {
            public InvalidLuisDialog(ILuisService service)
                : base(service)
            {
            }

            [LuisIntent("HasAttributeButDoesNotMatchReturnType")]
            public void HasAttributeButDoesNotMatchReturnType(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidIntentHandlerException))]
        public void Invalid_Handle_Throws_Error()
        {
            var service = new Mock<ILuisService>();
            var dialog = new InvalidLuisDialog(service.Object);
            var handlers = LuisDialog.EnumerateHandlers(dialog).ToArray();
        }

        [TestMethod]
        public void UrlEncoding_UTF8_Then_Hex()
        {
            ILuisService service = new LuisService(new LuisModelAttribute("modelID", "subscriptionID"));

            var uri = service.BuildUri("Français");

            // https://github.com/Microsoft/BotBuilder/issues/247
            // https://github.com/Microsoft/BotBuilder/pull/76
            Assert.AreNotEqual("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/modelID?subscription-key=subscriptionID&q=Fran%25u00e7ais&log=True", uri.AbsoluteUri);
            Assert.AreEqual("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/modelID?subscription-key=subscriptionID&q=Fran%C3%A7ais&log=True", uri.AbsoluteUri);
        }

        [TestMethod]
        public void Uri_Building()
        {
            const string Model = "model";
            const string Subscription = "subscription";
            const string Domain = "domain";
            const string Text = "text";

            // TODO: xunit theory
            var tests = new[]
            {
#pragma warning disable CS0612
                new { m = new LuisModelAttribute(Model, Subscription, LuisApiVersion.V1, null) { }, u = new Uri("https://api.projectoxford.ai/luis/v1/application?subscription-key=subscription&q=text&id=model&log=True") },
                new { m = new LuisModelAttribute(Model, Subscription, LuisApiVersion.V1, null) { Log = false, SpellCheck = false, Staging = false, TimezoneOffset = 1, Verbose = false }, u = new Uri("https://api.projectoxford.ai/luis/v1/application?subscription-key=subscription&q=text&id=model&log=False&spellCheck=False&staging=False&timezoneOffset=1&verbose=False") },
                new { m = new LuisModelAttribute(Model, Subscription, LuisApiVersion.V1, Domain) { Log = true, SpellCheck = true, Staging = true, TimezoneOffset = 2, Verbose = true }, u = new Uri("https://api.projectoxford.ai/luis/v1/application?subscription-key=subscription&q=text&id=model&log=True&spellCheck=True&staging=True&timezoneOffset=2&verbose=True") },
#pragma warning restore CS0612
                new { m = new LuisModelAttribute(Model, Subscription, LuisApiVersion.V2, null) { }, u = new Uri("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/model?subscription-key=subscription&q=text&log=True") },
                new { m = new LuisModelAttribute(Model, Subscription, LuisApiVersion.V2, null) { Log = false, SpellCheck = false, Staging = false, TimezoneOffset = 1, Verbose = false }, u = new Uri("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/model?subscription-key=subscription&q=text&log=False&spellCheck=False&staging=False&timezoneOffset=1&verbose=False") },
                new { m = new LuisModelAttribute(Model, Subscription, LuisApiVersion.V2, Domain) { Log = true, SpellCheck = true, Staging = true, TimezoneOffset = 2, Verbose = true }, u = new Uri("https://domain/luis/v2.0/apps/model?subscription-key=subscription&q=text&log=True&spellCheck=True&staging=True&timezoneOffset=2&verbose=True") },
            };

            foreach (var test in tests)
            {
                ILuisService service = new LuisService(test.m);
                var actual = service.BuildUri(Text);
                Assert.AreEqual(test.u, actual);
            }
        }
    }
}
