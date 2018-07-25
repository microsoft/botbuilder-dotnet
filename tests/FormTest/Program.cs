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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable 649

using Autofac;

using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.FormFlow;
using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
using Microsoft.Bot.Connector;

using AnnotatedSandwichOrder = Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder;
using SimpleSandwichOrder = Microsoft.Bot.Sample.SimpleSandwichBot.SandwichOrder;
using System.Resources;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.Classic;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Classic.History;
using Luis = Microsoft.Bot.Builder.Classic.Luis;
using Microsoft.Bot.Builder.Adapters;

public class Globals
{
    public JObject state;
    public dynamic dstate;
    public object value;
    public IField<JObject> field;
}

namespace Microsoft.Bot.Builder.Classic.FormFlowTest
{
    public enum DebugOptions
    {
        None,
        AnnotationsAndNumbers,
        AnnotationsAndButtons,
        AnnotationsAndNoNumbers,
        NoAnnotations, NoFieldOrder,
        WithState,
        Localized,
        SimpleSandwichBot, AnnotatedSandwichBot, JSONSandwichBot
    };
    [Serializable]
    public class Choices
    {
        public DebugOptions Choice;
    }

    class Program
    {

        static public string Locale = CultureInfo.CurrentUICulture.Name;

        static async Task Interactive<T>(IDialog<T> form) where T : class
        {
            // NOTE: I use the DejaVuSansMono fonts as described here: http://stackoverflow.com/questions/21751827/displaying-arabic-characters-in-c-sharp-console-application
            // But you don't have to reboot.
            // If you don't want the multi-lingual support just comment this out
            Console.OutputEncoding = Encoding.GetEncoding(65001);
            var message = new Activity()
            {
                From = new ChannelAccount { Id = "ConsoleUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString() },
                Recipient = new ChannelAccount { Id = "FormTest" },
                ChannelId = "Console",
                ServiceUrl = "http://localhost:9000/",
                Text = string.Empty
            };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule_MakeRoot());

            using (var container = builder.Build())
            {
                var adapter = new ConsoleAdapter();
                adapter.ProcessActivity(async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        Func<IDialog<object>> MakeRoot = () => form;
                        await Conversation.SendAsync(context, MakeRoot);
                    }
                }).Wait();
            }
        }

        public static void TestValidate()
        {
            try
            {
                var form = new FormBuilder<PizzaOrder>()
                    .Message("{NotField}")
                    .Build();
                Debug.Fail("Validation failed");
            }
            catch (ArgumentException)
            {
            }
            try
            {
                var form = new FormBuilder<PizzaOrder>()
                    .Message("[{NotField}]")
                    .Build();
                Debug.Fail("Validation failed");
            }
            catch (ArgumentException)
            {
            }
            try
            {
                var form = new FormBuilder<PizzaOrder>()
                    .Message("{? {[{NotField}]}")
                    .Build();
                Debug.Fail("Validation failed");
            }
            catch (ArgumentException)
            {
            }
            try
            {
                var form = new FormBuilder<PizzaOrder>()
                    .Field(new FieldReflector<PizzaOrder>(nameof(PizzaOrder.Size))
                        .ReplaceTemplate(new TemplateAttribute(TemplateUsage.Double, "{Notfield}")))
                        .Build();
                Debug.Fail("Validation failed");
            }
            catch (ArgumentException)
            {
            }
        }

        public static IFormDialog<T> MakeForm<T>(BuildFormDelegate<T> buildForm) where T : class, new()
        {
            return new FormDialog<T>(new T(), buildForm, options: FormOptions.PromptInStart);
        }

        public static bool NonWord(string word)
        {
            bool nonWord = true;
            foreach (var ch in word)
            {
                if (!(char.IsControl(ch) || char.IsPunctuation(ch) || char.IsWhiteSpace(ch)))
                {
                    nonWord = false;
                    break;
                }
            }
            return nonWord;
        }

        static void Main(string[] args)
        {
            // TestValidate();
            var callDebug =
                Chain
                .From(() => new PromptDialog.PromptString("Locale?", null, 1))
                .ContinueWith<string, Choices>(async (ctx, locale) =>
                    {
                        Locale = await locale;
                        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(Locale);
                        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
                        return new FormDialog<Choices>(new Choices(), () =>
                        {
                            var builder = new FormBuilder<Choices>();
                            builder.Configuration.DefaultPrompt.ChoiceStyle = ChoiceStyleOptions.AutoText;
                            return builder.AddRemainingFields().Build();
                        }
                        , FormOptions.PromptInStart);
                    })
                .ContinueWith<Choices, object>(async (context, result) =>
                {
                    Choices choices;
                    try
                    {
                        choices = await result;
                    }
                    catch (Exception error)
                    {
                        await context.PostAsync(error.ToString());
                        throw;
                    }

                    switch (choices.Choice)
                    {
                        case DebugOptions.AnnotationsAndNumbers:
                            return MakeForm(() => PizzaOrder.BuildForm());
                        case DebugOptions.AnnotationsAndNoNumbers:
                            return MakeForm(() => PizzaOrder.BuildForm(noNumbers: true));
                        case DebugOptions.AnnotationsAndButtons:
                            return MakeForm(() => PizzaOrder.BuildForm(style: ChoiceStyleOptions.Auto));
                        case DebugOptions.NoAnnotations:
                            return MakeForm(() => PizzaOrder.BuildForm(noNumbers: true, ignoreAnnotations: true));
                        case DebugOptions.NoFieldOrder:
                            return MakeForm(() => new FormBuilder<PizzaOrder>().Build());
                        case DebugOptions.WithState:
                            return new FormDialog<PizzaOrder>(new PizzaOrder()
                            { Size = SizeOptions.Large, Kind = PizzaOptions.BYOPizza },
                            () => PizzaOrder.BuildForm(),
                            options: FormOptions.PromptInStart | FormOptions.PromptFieldsWithValues,
                            entities: new Luis.Models.EntityModel[] {
                                new Luis.Models.EntityModel("DeliveryAddress", entity:"2"),
                                new Luis.Models.EntityModel("Signature", entity:"Hawaiian"),
                                new Luis.Models.EntityModel("BYO.Toppings", entity:"onions"),
                                new Luis.Models.EntityModel("BYO.Toppings", entity:"peppers"),
                                new Luis.Models.EntityModel("BYO.Toppings", entity:"ice"),
                                new Luis.Models.EntityModel("NumberOfPizzas", entity:"5"),
                                new Luis.Models.EntityModel("NotFound", entity:"OK")
                            }
                            );
                        case DebugOptions.Localized:
                            {
                                var form = PizzaOrder.BuildForm();
                                using (var stream = new FileStream("pizza.resx", FileMode.Create))
                                using (var writer = new ResXResourceWriter(stream))
                                {
                                    form.SaveResources(writer);
                                }
                                Process.Start(new ProcessStartInfo(@"RView.exe", "pizza.resx -c " + Locale) { UseShellExecute = false, CreateNoWindow = true }).WaitForExit();
                                return MakeForm(() => PizzaOrder.BuildForm(false, false, true));
                            }
                        case DebugOptions.SimpleSandwichBot:
                            return MakeForm(() => SimpleSandwichOrder.BuildForm());
                        case DebugOptions.AnnotatedSandwichBot:
                            return MakeForm(() => AnnotatedSandwichOrder.BuildLocalizedForm());
                        //case DebugOptions.JSONSandwichBot:
                        //    return MakeForm(() => AnnotatedSandwichOrder.BuildJsonForm());
                        default:
                            throw new NotImplementedException();
                    }
                })
                .Do(async (context, result) =>
                {
                    try
                    {
                        var item = await result;
                        Debug.WriteLine(item);
                    }
                    catch (FormCanceledException e)
                    {
                        if (e.InnerException == null)
                        {
                            await context.PostAsync($"Quit on {e.Last} step.");
                        }
                        else
                        {
                            await context.PostAsync($"Exception {e.Message} on step {e.Last}.");
                        }
                    }
                })
                .DefaultIfException()
                .Loop();
            Interactive(callDebug).GetAwaiter().GetResult();
        }
    }
}
