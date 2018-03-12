// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Storage;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class LocaleConverterMiddlewareTests
    {
        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverterMiddleware_ConvertFromFrench()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(new BotStateManager(new FileStorage(System.IO.Path.GetTempPath()))) //store user state in a temp directory
                .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "en-us", new LocaleConverter()));

            bot.OnReceive((context) =>
            {
                if (context.Responses.Count == 0)
                {
                    context.Reply(context.Request.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            });

            await adapter
                .Send("set my locale to fr-fr")
                    .AssertReply("Changing your locale to fr-fr")
                .Send("Set a meeting on 30/9/2017")
                    .AssertReply("Set a meeting on 09/30/2017")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverterMiddleware_ConvertToChinese()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(new BotStateManager(new FileStorage(System.IO.Path.GetTempPath()))) //store user state in a temp directory
                .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "zh-cn", new LocaleConverter()));

            bot.OnReceive((context) =>
            {
                if (context.Responses.Count == 0)
                {
                    context.Reply(context.Request.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            });

            await adapter
                .Send("set my locale to en-us")
                    .AssertReply("Changing your locale to en-us")
                .Send("Book me a plane ticket for France on 12/25/2018")
                    .AssertReply("Book me a plane ticket for France on 2018-12-25")
                .StartTest();
        }

        private void SetLocale(IBotContext context, string locale) => context.State.User[@"LocaleConverterMiddleware.fromLocale"] = locale;

        protected async Task<bool> SetActiveLocale(IBotContext context)
        {
            bool changeLocale = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            var messageActivity = context.Request.AsMessageActivity();
            if (messageActivity.Text.ToLower().StartsWith("set my locale to"))
            {
                changeLocale = true;
            }
            if (changeLocale)
            {
                var newLocale = messageActivity.Text.ToLower().Replace("set my locale to", "").Trim(); //extracted by the user using user state 
                if (!string.IsNullOrWhiteSpace(newLocale))
                {
                    SetLocale(context, newLocale);
                    context.Reply($@"Changing your locale to {newLocale}");
                }
                else
                {
                    context.Reply($@"{newLocale} is not a supported locale.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected string GetActiveLocale(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message
                && context.State.User.ContainsKey(@"LocaleConverterMiddleware.fromLocale"))
            {
                return (string)context.State.User[@"LocaleConverterMiddleware.fromLocale"];
            }

            return "en-us";
        }
    }
}
