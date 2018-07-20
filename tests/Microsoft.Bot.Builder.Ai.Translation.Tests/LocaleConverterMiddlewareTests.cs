// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.Translation.Tests
{
    class LocaleState
    {
        public string Locale { get; set; }
    }

    [TestClass]
    public class LocaleConverterMiddlewareTests
    {
        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverterMiddleware_ConvertFromFrench()
        {
            TestAdapter adapter = new TestAdapter()
             .Use(new UserState<LocaleState>(new MemoryStorage()))
             .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "en-us", LocaleConverter.Converter));


            await new TestFlow(adapter, (context) =>
            {
                if (!context.Responded)
                {
                    context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            })
                .Send("set my locale to fr-fr")
                    .AssertReply("Changing your locale to fr-fr")
                .Send("Set a meeting on 30/9/2017")
                    .AssertReply("Set a meeting on 9/30/2017")
                    .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverterMiddleware_ConvertFromSpanishSpain()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new UserState<LocaleState>(new MemoryStorage()))
                .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "en-us", LocaleConverter.Converter));


            await new TestFlow(adapter, (context) =>
                {
                    if (!context.Responded)
                    {
                        context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                    }
                    return Task.CompletedTask;
                })
                .Send("set my locale to es-es")
                .AssertReply("Changing your locale to es-es")
                .Send("La reunión será a las 15:00")
                .AssertReply("La reunión será a las 3:00 PM")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverterMiddleware_ConvertToChinese()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new UserState<LocaleState>(new MemoryStorage()))
                .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "zh-cn", LocaleConverter.Converter));


            await new TestFlow(adapter, (context) =>
            {
                if (!context.Responded)
                {
                    context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            })
                .Send("set my locale to en-us")
                    .AssertReply("Changing your locale to en-us")
                .Send("Book me a plane ticket for France on 12/25/2018")
                    .AssertReply("Book me a plane ticket for France on 2018/12/25")
                .StartTestAsync();
        }

        private void SetLocale(ITurnContext context, string locale) => context.GetUserState<LocaleState>().Locale  = locale;

        protected async Task<bool> SetActiveLocale(ITurnContext context)
        {
            bool changeLocale = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            var messageActivity = context.Activity.AsMessageActivity();
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
                    await context.SendActivityAsync($@"Changing your locale to {newLocale}");
                }
                else
                {
                    await context.SendActivityAsync($@"{newLocale} is not a supported locale.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected string GetActiveLocale(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message
                && !string.IsNullOrEmpty(context.GetUserState<LocaleState>().Locale))
            {
                return context.GetUserState<LocaleState>().Locale;
            }

            return "en-us";
        }
    }
}
