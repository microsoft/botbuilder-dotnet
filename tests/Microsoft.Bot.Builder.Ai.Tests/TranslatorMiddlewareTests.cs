// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class TranslatorMiddlewareTests
    {
        public string translatorKey = TestUtilities.GetKey("TRANSLATORKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task TranslatorMiddleware_DetectAndTranslateToEnglish()
        {

            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(new BotStateManager(new FileStorage(System.IO.Path.GetTempPath()))) //store user state in a temp directory
                .Use(new TranslationMiddleware(new string[] { "en-us" }, translatorKey));

            bot.OnReceive((context) =>
            {
                if (context.Responses.Count == 0)
                {
                    context.Reply(context.Request.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            });

            await adapter
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

            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(new BotStateManager(new FileStorage(System.IO.Path.GetTempPath()))) //store user state in a temp directory
                .Use(new TranslationMiddleware(new string[] { "en-us" }, translatorKey, "", GetActiveLanguage, SetActiveLanguage));

            bot.OnReceive((context) =>
            {
                if (context.Responses.Count == 0)
                {
                    context.Reply(context.Request.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            });

            await adapter
                .Send("set my language to fr")
                    .AssertReply("Changing your language to fr")
                .Send("salut")
                    .AssertReply("Hello")
                .StartTest();
        }

        private void SetLanguage(IBotContext context, string language) => context.State.User[@"Microsoft.API.translateTo"] = language;
       
        protected async Task<bool> SetActiveLanguage(IBotContext context)
        {
            bool changeLang = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            var messageActivity = context.Request.AsMessageActivity();
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
                    context.Reply($@"Changing your language to {newLang}");
                }
                else
                {
                    context.Reply($@"{newLang} is not a supported language.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected string GetActiveLanguage(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message
                && context.State.User.ContainsKey(@"Microsoft.API.translateTo"))
            {
                return (string)context.State.User[@"Microsoft.API.translateTo"];
            }

            return "en";
        }   
    }
}
