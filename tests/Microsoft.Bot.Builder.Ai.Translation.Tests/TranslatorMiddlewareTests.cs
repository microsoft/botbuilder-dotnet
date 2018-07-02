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
    class LanguageState
    {
        public string Language { get; set; }
    }

    /// <summary>
    /// A specialized translator that can handle specific scenarios.
    /// </summary>
    class SpecializedTranslatorMiddleware : TranslationMiddleware
    {
        public SpecializedTranslatorMiddleware(string[] nativeLanguages, string translatorKey) : base(nativeLanguages, translatorKey)
        { }

        public override async Task OnTurn(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            // alter the original utterance before translation. 
            if (context.Activity.Text == "mañana")
            {
                context.Activity.Text = "para mañana";
            }

            await base.OnTurn(context, next, cancellationToken);
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
                .Use(new SpecializedTranslatorMiddleware(new[] {"en-us"}, translatorKey));

            await new TestFlow(adapter, context =>
                {
                    if (!context.Responded)
                    {
                        context.SendActivity(context.Activity.AsMessageActivity().Text);
                    }

                    return Task.CompletedTask;
                })
                .Send("hola")
                .AssertReply("Hello")
                .Send("mañana")
                .AssertReply("For tomorrow")
                .StartTest();
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

            var adapter = new TestAdapter()
                .Use(new TranslationMiddleware(new[] {"en-us"}, translatorKey));

            await new TestFlow(adapter, context =>
                {
                    if (!context.Responded)
                    {
                        context.SendActivity(context.Activity.AsMessageActivity().Text);
                    }

                    return Task.CompletedTask;
                })
                .Send("salut")
                .AssertReply("Hello")
                .Send("salut 10-20")
                .AssertReply("Hi 10-20")
                .StartTest();
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

            var adapter = new TestAdapter()
                .Use(new UserState<LanguageState>(new MemoryStorage()))
                .Use(new TranslationMiddleware(new[] {"en-us"}, translatorKey, new Dictionary<string, List<string>>(), new CustomDictionary(), GetActiveLanguage, SetActiveLanguage));

            await new TestFlow(adapter, context =>
                {
                    if (!context.Responded)
                    {
                        context.SendActivity(context.Activity.AsMessageActivity().Text);
                    }

                    return Task.CompletedTask;
                })
                .Send("set my language to fr")
                .AssertReply("Changing your language to fr")
                .Send("salut")
                .AssertReply("Hello")
                .StartTest();
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

            var adapter = new TestAdapter()
                .Use(new UserState<LanguageState>(new MemoryStorage()))
                .Use(new TranslationMiddleware(new[] {"en-us"}, translatorKey, new Dictionary<string, List<string>>(), new CustomDictionary(), GetActiveLanguage, SetActiveLanguage, true));

            await new TestFlow(adapter, context =>
                {
                    if (!context.Responded)
                    {
                        context.SendActivity(context.Activity.AsMessageActivity().Text);
                    }

                    return Task.CompletedTask;
                })
                .Send("set my language to fr")
                .AssertReply("Changing your language to fr")
                .Send("salut")
                .AssertReply("Salut")
                .StartTest();
        }

        private void SetLanguage(ITurnContext context, string language) => context.GetUserState<LanguageState>().Language = language;

        protected async Task<bool> SetActiveLanguage(ITurnContext context)
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
                    SetLanguage(context, newLang);
                    await context.SendActivity($@"Changing your language to {newLang}");
                }
                else
                {
                    await context.SendActivity($@"{newLang} is not a supported language.");
                }

                //intercepts message
                return true;
            }

            return false;
        }

        protected string GetActiveLanguage(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message
                && !string.IsNullOrEmpty(context.GetUserState<LanguageState>().Language))
            {
                return context.GetUserState<LanguageState>().Language;
            }

            return "en";
        }

        private bool EnvironmentVariablesDefined()
        {
            return translatorKey != null;
        }
    }
}
