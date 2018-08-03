// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.Translation.Tests
{

    [TestClass]
    public class LocaleConverterMiddlewareTests
    {

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public void LocaleConverterMiddleware_LocaleConstructor()
        {
            var userState = new UserState(new MemoryStorage());
            var userLocaleProperty = userState.CreateProperty<string>("locale");
            var lcm = new LocaleConverterMiddleware(userLocaleProperty, toLocale:"en-us", localeConverter:LocaleConverter.Converter, defaultLocale:"en-us");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LocaleConverterMiddleware_NullDefaultLocale()
        {
            var userState = new UserState(new MemoryStorage());
            var userLocaleProperty = userState.CreateProperty<string>("locale");
            var lcm = new LocaleConverterMiddleware(userLocaleProperty, toLocale: "en-us", localeConverter: LocaleConverter.Converter, defaultLocale: null);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LocaleConverterMiddleware_EmptyDefaultLocale()
        {
            var userState = new UserState(new MemoryStorage());
            var userLocaleProperty = userState.CreateProperty<string>("locale");
            var lcm = new LocaleConverterMiddleware(userLocaleProperty, toLocale: "en-us", localeConverter: LocaleConverter.Converter, defaultLocale: " ");
        }





        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public async Task LocaleConverterMiddleware_ConvertFromFrench()
        {
            var userState = new UserState(new MemoryStorage());
            var userLocaleProperty = userState.CreateProperty<string>("locale");

            TestAdapter adapter = new TestAdapter()
                .Use(userState)
                .Use(new LocaleConverterMiddleware(userLocaleProperty, "en-us", LocaleConverter.Converter));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    if (!await ChangeLocaleRequest(context, userLocaleProperty))
                    {
                        await context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                    }
                    return;
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
            var userState = new UserState(new MemoryStorage());
            var userLocaleProperty = userState.CreateProperty<string>("locale");


            TestAdapter adapter = new TestAdapter()
                .Use(userState)
                .Use(new LocaleConverterMiddleware(userLocaleProperty, "en-us", LocaleConverter.Converter));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    if (!await ChangeLocaleRequest(context, userLocaleProperty))
                    {
                        await context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                    }
                    return;
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
            var userState = new UserState(new MemoryStorage());
            var userLocaleProperty = userState.CreateProperty<string>("locale");

            TestAdapter adapter = new TestAdapter()
                .Use(userState)
                .Use(new LocaleConverterMiddleware(userLocaleProperty, "zh-cn", LocaleConverter.Converter));


            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                if (!await ChangeLocaleRequest(context, userLocaleProperty))
                {
                    await context.SendActivityAsync(context.Activity.AsMessageActivity().Text);
                }
                return;
            })
                .Send("set my locale to en-us")
                    .AssertReply("Changing your locale to en-us")
                .Send("Book me a plane ticket for France on 12/25/2018")
                    .AssertReply("Book me a plane ticket for France on 2018/12/25")
                .StartTestAsync();
        }

        protected async Task<bool> ChangeLocaleRequest(ITurnContext context, IStatePropertyAccessor<string> userLocaleProperty)
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
                var newLocale = messageActivity.Text.ToLower().Replace("set my locale to", string.Empty).Trim(); //extracted by the user using user state 
                if (!string.IsNullOrWhiteSpace(newLocale))
                {
                    await userLocaleProperty.SetAsync(context, newLocale);
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
    }

    class LocaleState
    {
        public string Locale { get; set; }
    }
}
