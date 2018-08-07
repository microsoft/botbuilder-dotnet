// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.Ai.Translation.Tests
{
    [TestClass]
    public class TranslatorMiddlewareTests
    {
        private const string _translatorKey = "dummy-key";

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void TranslatorMiddleware_Construct()
        {
            var mockHttp = new MockHttpMessageHandler();
            var tm = new TranslationMiddleware(new[] { "en" }, _translatorKey, httpClient: mockHttp.ToHttpClient(), defaultLocale: "en");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TranslatorMiddleware_ConstructNullLocale()
        {
            var mockHttp = new MockHttpMessageHandler();
            var tm = new TranslationMiddleware(new[] { "en" }, _translatorKey, httpClient: mockHttp.ToHttpClient(), defaultLocale: null);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TranslatorMiddleware_ConstructWSLocale()
        {
            var mockHttp = new MockHttpMessageHandler();
            var tm = new TranslationMiddleware(new[] { "en" }, _translatorKey, httpClient: mockHttp.ToHttpClient(), defaultLocale: "   ");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void TranslatorMiddleware_PropertyLocale()
        {
            var mockHttp = new MockHttpMessageHandler();
            var tm = new TranslationMiddleware(new[] { "en" }, _translatorKey, httpClient: mockHttp.ToHttpClient());
            Assert.AreEqual("en", tm.DefaultLocale);
        }


        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void MethodsAreVirtual()
        {
            var type = typeof(TranslationMiddleware);
            foreach (var methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // Check that the methods are overridable (Virtual and Not Final).
                // See https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.isvirtual?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(System.Reflection.MethodBase.IsVirtual);k(SolutionItemsProject);k(DevLang-csharp)%26rd%3Dtrue&view=netframework-4.7.2
                Assert.IsTrue(methodInfo.IsVirtual && !methodInfo.IsFinal, $"{methodInfo.Name} should be virtual");
            }
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task SpecializedTranslator()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetDetectUri())
                .Respond("application/json", GetResponse("SpecializedTranslator_Detect.json"));
            mockHttp.When(HttpMethod.Post, GetTranslateUri("es", "en")).WithPartialContent("hola")
                .Respond("application/json", GetResponse("SpecializedTranslator_Hello.json"));
            mockHttp.When(HttpMethod.Post, GetTranslateUri("es", "en")).WithPartialContent("para mañana")
                .Respond("application/json", GetResponse("SpecializedTranslator_Tomorrow.json"));

            var adapter = new TestAdapter(sendTraceActivity: true)
                .Use(new SpecializedTranslatorMiddleware(new[] { "en" }, _translatorKey, mockHttp.ToHttpClient()));

            await new TestFlow(adapter, (context, cancellationToken) =>
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
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetDetectUri())
                .Respond("application/json", GetResponse("TranslatorMiddleware_DetectAndTranslateToEnglish_Detect.json"));
            mockHttp.When(HttpMethod.Post, GetTranslateUri("fr", "en")).WithPartialContent("salut 10-20")
                .Respond("application/json", GetResponse("TranslatorMiddleware_DetectAndTranslateToEnglish_Hi.json"));
            mockHttp.When(HttpMethod.Post, GetTranslateUri("fr", "en")).WithPartialContent("salut")
                .Respond("application/json", GetResponse("TranslatorMiddleware_DetectAndTranslateToEnglish_Hello.json"));

            var userState = new UserState(new MemoryStorage());
            var languageStateProperty = userState.CreateProperty<string>("languageState");

            var adapter = new TestAdapter()
                .Use(new TranslationMiddleware(new[] { "en" }, _translatorKey, httpClient: mockHttp.ToHttpClient()));

            await new TestFlow(adapter, async (context, cancellationToken) =>
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
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetDetectUri())
                .Respond("application/json", GetResponse("TranslatorMiddleware_TranslateFrenchToEnglish_Detect.json"));
            mockHttp.When(HttpMethod.Post, GetTranslateUri("fr", "en")).WithPartialContent("salut")
                .Respond("application/json", GetResponse("TranslatorMiddleware_TranslateFrenchToEnglish_Translate.json"));

            var userState = new UserState(new MemoryStorage());
            var languageStateProperty = userState.CreateProperty<string>("languageState");
            
            var adapter = new TestAdapter()
                .Use(userState)
                .Use(new TranslationMiddleware(new[] { "en" }, _translatorKey, new Dictionary<string, List<string>>(), new CustomDictionary(), languageStateProperty, httpClient: mockHttp.ToHttpClient()));

            await new TestFlow(adapter, async (context, cancellationToken) =>
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
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetDetectUri())
                .Respond("application/json", GetResponse("TranslatorMiddleware_TranslateFrenchToEnglishToUserLanguage_Detect.json"));
            mockHttp.When(HttpMethod.Post, GetTranslateUri("fr", "en")).WithPartialContent("salut")
                .Respond("application/json", GetResponse("TranslatorMiddleware_TranslateFrenchToEnglishToUserLanguage_Salut.json"));
            mockHttp.When(HttpMethod.Post, GetTranslateUri("en", "fr")).WithPartialContent("Hello")
                .Respond("application/json", GetResponse("TranslatorMiddleware_TranslateFrenchToEnglishToUserLanguage_Hello.json"));

            var userState = new UserState(new MemoryStorage());
            var languageStateProperty = userState.CreateProperty<string>("languageState");
            
            var adapter = new TestAdapter()
                .Use(userState)
                .Use(new TranslationMiddleware(new[] { "en" }, _translatorKey, new Dictionary<string, List<string>>(), new CustomDictionary(), languageStateProperty, true, mockHttp.ToHttpClient()));

            await new TestFlow(adapter, async (context, cancellationToken) =>
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

        private string GetDetectUri()
        {
            return $"https://api.cognitive.microsofttranslator.com/detect?api-version=3.0";
        }

        private string GetTranslateUri(string from, string to)
        {
            return $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&includeAlignment=true&includeSentenceLength=true&from={from}&to={to}";
        }

        private Stream GetResponse(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
            return File.OpenRead(path);
        }

        protected async Task<bool> HandleChangeLanguageRequest(ITurnContext context, IStatePropertyAccessor<string> languageProperty)
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
                var newLang = messageActivity.Text.ToLower().Replace("set my language to", string.Empty).Trim();
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

    }
    class LanguageState
    {
        public string Language { get; set; }
    }

    /// <summary>
    /// A specialized translator that can handle specific scenarios.
    /// </summary>
    class SpecializedTranslatorMiddleware : TranslationMiddleware
    {
        public SpecializedTranslatorMiddleware(string[] nativeLanguages, string translatorKey, HttpClient client) : base(nativeLanguages, translatorKey, httpClient: client)
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

}
