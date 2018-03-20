// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting; 
using System.Threading.Tasks;

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
            TestAdapter adapter = new TestAdapter()
             .Use(new BatchOutputMiddleware())
             .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "en-us", new LocaleConverter()));


            await new TestFlow(adapter, (context) =>
            {
                if (!context.Responded)
                {
                    context.Batch().Reply(context.Request.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            })
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
            TestAdapter adapter = new TestAdapter()
                .Use(new BatchOutputMiddleware())
             .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "zh-cn", new LocaleConverter()));


            await new TestFlow(adapter, (context) =>
            {
                if (!context.Responded)
                {
                    context.Batch().Reply(context.Request.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            })
                .Send("set my locale to en-us")
                    .AssertReply("Changing your locale to en-us")
                .Send("Book me a plane ticket for France on 12/25/2018")
                    .AssertReply("Book me a plane ticket for France on 2018-12-25")
                .StartTest();
        }

        private void SetLocale(IBotContext context, string locale) => context.Set(@"LocaleConverterMiddleware.fromLocale",locale);

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
                    await context.SendActivity($@"Changing your locale to {newLocale}");
                }
                else
                {
                    await context.SendActivity($@"{newLocale} is not a supported locale.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected string GetActiveLocale(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message
                && context.Has(@"LocaleConverterMiddleware.fromLocale"))
            {
                return (string)context.Get(@"LocaleConverterMiddleware.fromLocale");
            }

            return "en-us";
        }
    }
}
