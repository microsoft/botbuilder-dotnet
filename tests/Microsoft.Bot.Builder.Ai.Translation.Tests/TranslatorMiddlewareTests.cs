// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai.Translation;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.QnA.Tests
{
    /// <summary>
    /// A specialized translator that can handle specific scenarios.
    /// </summary>
    class SpecializedTranslatorMiddleware : TranslationMiddleware
    {
        public SpecializedTranslatorMiddleware(string[] nativeLanguages, string translatorKey) : base(nativeLanguages, translatorKey)
        { }

        public override async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            // alter the original utterance before translation. 
            if (context.Activity.Text == "mañana")
            {
                context.Activity.Text = "para mañana";
            }

            await base.OnTurnAsync(context, next, cancellationToken);
        }
    }

    [TestClass]
    public class TranslatorMiddlewareTests
    {
        public string translatorKey = TestUtilities.GetKey("TRANSLATORKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void MethodsAreVirual()
        {
            var type = typeof(TranslationMiddleware);
            foreach (var methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // Check that the methos is overridable (Virtual and Not Final).
                // See https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.isvirtual?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(System.Reflection.MethodBase.IsVirtual);k(SolutionItemsProject);k(DevLang-csharp)%26rd%3Dtrue&view=netframework-4.7.2
                Assert.IsTrue(methodInfo.IsVirtual && !methodInfo.IsFinal, $"{methodInfo.Name} should be virtual");
            }
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task SpecializedTranslator()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
            }

            var adapter = new TestAdapter()
                .Use(new SpecializedTranslatorMiddleware(new[] { "en" }, translatorKey));

            await new TestFlow(adapter, context =>
                {
                    if (!context.Responded)
                    {
                        context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                    }

                    return Task.CompletedTask;
                })
                .Send("hola")
                .AssertReply("Hello")
                .Send("mañana")
                .AssertReply("For tomorrow")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task TranslatorMiddleware_DetectAndTranslateToEnglish()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            var userState = new UserState(new MemoryStorage());
            var languageStateProperty = userState.CreateProperty<string>("languageState", "en");

            var adapter = new TestAdapter()
                .Use(new TranslationMiddleware(new[] { "en" }, translatorKey));

            await new TestFlow(adapter, async context =>
                {
                    if (!await HandleChangeLanguageRequest(context, languageStateProperty))
                    {
                        await context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                    }
                })
                .Send("salut")
                .AssertReply("Hello")
                .Send("salut 10-20")
                .AssertReply("Hi 10-20")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task TranslatorMiddleware_TranslateFrenchToEnglish()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }
            var userState = new UserState(new MemoryStorage());
            var languageStateProperty = userState.CreateProperty<string>("languageState", "en");

            var adapter = new TestAdapter()
                .Use(userState)
                .Use(new TranslationMiddleware(new[] { "en" }, translatorKey, new Dictionary<string, List<string>>(), new CustomDictionary(), languageStateProperty));

            await new TestFlow(adapter, async context =>
                {
                    if (!await HandleChangeLanguageRequest(context, languageStateProperty))
                    {
                        await context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                    }
                })
                .Send("set my language to fr")
                .AssertReply("Changing your language to fr")
                .Send("salut")
                .AssertReply("Hello")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task TranslatorMiddleware_TranslateFrenchToEnglishToUserLanguage()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }
            var userState = new UserState(new MemoryStorage());
            var languageStateProperty = userState.CreateProperty<string>("languageState", "en");

            var adapter = new TestAdapter()
                .Use(userState)
                .Use(new TranslationMiddleware(new[] { "en" }, translatorKey, new Dictionary<string, List<string>>(), new CustomDictionary(), languageStateProperty, true));

            await new TestFlow(adapter, async context =>
                {
                    if (!await HandleChangeLanguageRequest(context, languageStateProperty))
                    {
                        await context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                    }
                })
                .Send("set my language to fr")
                .AssertReply("Changing your language to fr")
                .Send("salut")
                .AssertReply("Salut")
                .StartTestAsync();
        }

        protected async Task<bool> HandleChangeLanguageRequest(ITurnContext context, IPropertyAccessor<string> languageProperty)
        {
            var changeLang = false; //logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language 
            var messageActivity = context.Activity.AsMessageActivity();
            if (messageActivity.Text.ToLower().StartsWith("set my language to"))
            {
                changeLang = true;
            }

            if (changeLang)
            {
                var newLang = messageActivity.Text.ToLower().Replace("set my language to", "").Trim();
                if (!string.IsNullOrWhiteSpace(newLang))
                {
                    await languageProperty.SetAsync(context, newLang);
                    await context.SendActivityAsync($@"Changing your language to {newLang}");
                }
                else
                {
                    await context.SendActivityAsync($@"{newLang} is not a supported language.");
                }

                //intercepts message
                return true;
            }

            return false;
        }

        private bool EnvironmentVariablesDefined()
        {
            return translatorKey != null;
        }
    }
}
