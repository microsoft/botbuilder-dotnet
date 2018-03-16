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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.FormFlow;
using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
#if FORMFLOW_JSON
using Microsoft.Bot.Builder.Classic.FormFlow.Json;
#endif
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Luis.Models;
using Microsoft.Bot.Builder.Classic.FormFlowTest;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Classic.Tests
{
#pragma warning disable CS1998

    [TestClass]
    public sealed class FormTests : DialogTestBase
    {
        // http://stackoverflow.com/questions/3330989/order-of-serialized-fields-using-json-net
        public class OrderedContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();
            }
        }

        public static string SerializeToJson(object item)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new OrderedContractResolver()
            };
            return JsonConvert.SerializeObject(item, settings);
        }

        public async Task RecordFormScript<T>(string filePath,
            string locale, BuildFormDelegate<T> buildForm, FormOptions options, T initialState, IEnumerable<EntityRecommendation> entities,
            params string[] inputs)
            where T : class
        {
            using (var stream = new StreamWriter(filePath))
            using (var container = Build(Options.ResolveDialogFromContainer | Options.Reflection))
            {
                var root = new FormDialog<T>(initialState, buildForm, options, entities, CultureInfo.GetCultureInfo(locale));
                stream.WriteLine($"{locale}");
                stream.WriteLine($"{SerializeToJson(initialState)}");
                stream.WriteLine($"{SerializeToJson(entities)}");
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(root)
                    .AsSelf()
                    .As<IDialog<object>>();
                builder.Update(container);
                await Script.RecordScript(container, false, stream, () => "State:" + SerializeToJson(initialState), inputs);
            }
        }

        public async Task VerifyFormScript<T>(string filePath,
            string locale, BuildFormDelegate<T> buildForm, FormOptions options, T initialState, IEnumerable<EntityRecommendation> entities,
            params string[] inputs)
            where T : class
        {
            var newPath = Script.NewScriptPathFor(filePath);
            File.Delete(newPath);
            var currentState = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(initialState));
            try
            {
                using (var stream = new StreamReader(filePath))
                using (var container = Build(Options.Reflection))
                {
                    Func<IDialog<object>> makeRoot = () => new FormDialog<T>(currentState, buildForm, options, entities);
                    Assert.AreEqual(locale, stream.ReadLine());
                    Assert.AreEqual(SerializeToJson(initialState), stream.ReadLine());
                    Assert.AreEqual(SerializeToJson(entities), stream.ReadLine());
                    await Script.VerifyScript(container, makeRoot, false, stream, (stack, state) =>
                    {
                        var form = ((FormDialog<T>)stack.Frames[0].Target);
                        Assert.AreEqual(state, SerializeToJson(form.State));
                    }, inputs, locale);
                }
            }
            catch (Exception)
            {
                // There was an error, so record new script and pass on error
                await RecordFormScript(newPath, locale, buildForm, options, initialState, entities, inputs);
#if MISSING
                TestContext.AddResultFile(newPath);
#endif
                throw;
            }
        }

        public interface IFormTarget
        {
            string Text { get; set; }
            int Integer { get; set; }
            float Float { get; set; }
        }

        private static class Input
        {
            public const string Text = "some text here";
            public const int Integer = 99;
            public const float Float = 1.5f;
        }

        [Serializable]
        private sealed class FormTarget : IFormTarget
        {
            float IFormTarget.Float { get; set; }
            int IFormTarget.Integer { get; set; }
            string IFormTarget.Text { get; set; }
        }

        public enum SimpleChoices
        {
            One = 1,
            [Terms("Two", "More than one")]
            Two,
            [Terms("Three", "More than one")]
            Three,
            [Terms("word", @"\bpword\(123\)", @"32 jump\b")]
            Four
        };

        [Serializable]
        private sealed class SimpleForm
        {
            public string Text { get; set; }
            public int Integer { get; set; }
            public float? Float { get; set; }
            [Template(TemplateUsage.NotUnderstood, "Choices {||}")]
            public SimpleChoices SomeChoices { get; set; }
            public DateTime Date { get; set; }
        }

        private static async Task RunScriptAgainstForm(IEnumerable<EntityRecommendation> entities, params string[] script)
        {
            IFormTarget target = new FormTarget();
            using (var container = Build(Options.ResolveDialogFromContainer, target))
            {
                {
                    var root = new FormDialog<IFormTarget>(target, entities: entities);
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance(root)
                        .AsSelf()
                        .As<IDialog<object>>();
                    builder.Update(container);
                }

                await AssertScriptAsync(container, script);
                {
                    Assert.AreEqual(Input.Text, target.Text);
                    Assert.AreEqual(Input.Integer, target.Integer);
                    Assert.AreEqual(Input.Float, target.Float);
                }
            }
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        [DeploymentItem(@"Scripts\SimpleForm.script")]
        public async Task Simple_Form_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us", () => new FormBuilder<SimpleForm>().AddRemainingFields().Build(), FormOptions.None, new SimpleForm(), Array.Empty<EntityRecommendation>(),
                "Hi",

                "?",
                "some text here",

                "?",
                "99",
                "back",
                "c",

                "?",
                "1.5",

                "?",
                "one",

                "help",
                "status",
                "1/1/2016"
                );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\SimpleForm-next.script")]
        public async Task SimpleForm_Next_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us", () => new FormBuilder<SimpleForm>()
                    .Field(new FieldReflector<SimpleForm>("Text")
                        .SetNext((value, state) => new NextStep(new string[] { "Float" })))
                    .AddRemainingFields()
                    .Build(),
                FormOptions.None, new SimpleForm(), Array.Empty<EntityRecommendation>(),
                "Hi",
                "some text here",
                "1.5",
                "one",
                "1/1/2016",
                "99"
                );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\SimpleForm-dependency.script")]
        public async Task SimpleForm_Dependency_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us",
                () => new FormBuilder<SimpleForm>()
                    .Field("Float")
                    .Field("SomeChoices",
                        validate: async (state, value) =>
                        {
                            var result = new ValidateResult { IsValid = true, Value = value };
                            if ((SimpleChoices)value == SimpleChoices.One)
                            {
                                state.Float = null;
                            }
                            return result;
                        })
                    .Confirm("All OK?")
                    .Build(),
            FormOptions.None, new SimpleForm(), Array.Empty<EntityRecommendation>(),
            "Hi",
            "1.0",
            "one",
            "2.0",
            "no",
            "Some Choices",
            "one",
            "3.0",
            "no",
            "some choices",
            "two",
            "yes"
        );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\SimpleForm-NotUnderstood.script")]
        public async Task SimpleForm_NotUnderstood_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us", () => new FormBuilder<SimpleForm>().AddRemainingFields().Build(), FormOptions.None, new SimpleForm(), Array.Empty<EntityRecommendation>(),
                "Hi",
                "some text here",
                "99",
                "1.5",
                "more than one",
                "foo",
                "two",
                "1/1/2016"
                );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\SimpleForm-Prompter.script")]
        public async Task SimpleForm_Prompter_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us",
                () => new FormBuilder<SimpleForm>()
                .Prompter(async (context, prompt, state, field) =>
                {
                    if (field != null)
                    {
                        prompt.Prompt = field.Name + ": " + prompt.Prompt;
                    }
                    var preamble = context.MakeMessage();
                    var promptMessage = context.MakeMessage();
                    if (prompt.GenerateMessages(preamble, promptMessage))
                    {
                        await context.PostAsync(preamble);
                    }
                    await context.PostAsync(promptMessage);
                    return prompt;
                })
                .AddRemainingFields()
                .Confirm(@"**Results**
* Text: {Text}
* Integer: {Integer}
* Float: {Float}
* SomeChoices: {SomeChoices}
* Date: {Date}
Is this what you wanted? {||}")
                .Build(),
                FormOptions.None, new SimpleForm(), Array.Empty<EntityRecommendation>(),
                "Hi",
                "some text here",
                "99",
                "1.5",
                "more than one",
                "foo",
                "two",
                "1/1/2016",
                "no",
                "text",
                "abc",
                "yes"
                );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\SimpleForm-Preamble.script")]
        public async Task SimpleForm_Preamble_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us",
                () => new FormBuilder<SimpleForm>()
                .AddRemainingFields()
                .Confirm(@"**Results**
* Text: {Text}
* Integer: {Integer}
* Float: {Float}
* SomeChoices: {SomeChoices}
* Date: {Date}
Is this what you wanted? {||}")
                .Build(),
                FormOptions.None, new SimpleForm(), Array.Empty<EntityRecommendation>(),
                "Hi",
                "some text here",
                "99",
                "1.5",
                "more than one",
                "foo",
                "two",
                "1/1/2016",
                "no",
                "text",
                "abc",
                "yes"
                );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\SimpleForm-Limits.script")]
        public async Task SimpleForm_Limits_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us",
                () => new FormBuilder<SimpleForm>().Build(),
                FormOptions.None, new SimpleForm(), Array.Empty<EntityRecommendation>(),
                "hi",
                "integer",
                // Test the limits of int vs long
                ((long)int.MaxValue + 1).ToString(),
                ((long)int.MinValue - 1).ToString(),

                // Test the limits beyond long
                long.MaxValue.ToString() + "1",
                long.MinValue.ToString() + "1",

                // Min and max accepted values
                int.MaxValue.ToString(),
                "back",
                int.MinValue.ToString(),

                // Test the limits of float vs. double
                ((double)float.MaxValue + 1.0).ToString(),
                ((double)float.MinValue * 2.0).ToString(),

                // Test limits beyond double
                (double.MaxValue).ToString().Replace("308", "309"),
                (double.MinValue).ToString().Replace("308", "309"),

                // Min and max accepted values
                float.MaxValue.ToString(),
                "back",
                float.MinValue.ToString(),
                "quit");
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\SimpleForm-Skip.script")]
        public async Task SimpleForm_Skip_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us",
                () => new FormBuilder<SimpleForm>().Build(),
                FormOptions.None, new SimpleForm() { Float = 4.3f }, Array.Empty<EntityRecommendation>(),
                "hi",
                "some text",
                "99",
                // Float should be skipped
                "word",
                "quit");
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\PizzaForm.script")]
        public async Task Pizza_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us", () => PizzaOrder.BuildForm(), FormOptions.None, new PizzaOrder(), Array.Empty<EntityRecommendation>(),
                "hi",
                "garbage",
                "2",
                "med",
                "4",
                "help",
                "drink bread",
                "back",
                "c",
                "garbage",
                "no",
                "thin",
                "1",
                "?",
                "garbage",
                "beef, onion, ice cream",
                "garbage",
                "onions",
                "status",
                "abc",
                "2",
                "garbage",
                "iowa",
                "y",
                "1 2",
                "none",
                "garbage",
                "2.5",
                "garbage",
                "2/25/1962 3pm",
                "no",
                "1234",
                "123-4567",
                "no",
                "toppings",
                "everything but spinach",
                "y"
                );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\PizzaForm-entities.script")]
        public async Task Pizza_Entities_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us", () => PizzaOrder.BuildForm(), FormOptions.None, new PizzaOrder(),
                new Luis.Models.EntityRecommendation[] {
                                new Luis.Models.EntityRecommendation("DeliveryAddress", entity:"2"),
                                new Luis.Models.EntityRecommendation("Kind", entity:"byo"),
                                // This should be skipped because it is not active
                                new Luis.Models.EntityRecommendation("Signature", entity:"Hawaiian"),
                                new Luis.Models.EntityRecommendation("BYO.Toppings", entity:"onions"),
                                new Luis.Models.EntityRecommendation("BYO.Toppings", entity:"peppers"),
                                new Luis.Models.EntityRecommendation("BYO.Toppings", entity:"ice"),
                                new Luis.Models.EntityRecommendation("NumberOfPizzas", entity:"5"),
                                new Luis.Models.EntityRecommendation("NotFound", entity:"OK")
                            },
                "hi",
                "1", // onions for topping clarification
                "2", // address choice from validation
                "med",
                // Kind "4",
                "drink bread",
                "thin",
                "1",
                // "beef, onion, ice cream",
                // Already have address
                "y",
                "1 2",
                "none",
                "2.5",
                "2/25/1962 3pm",
                "no",
                "123-4567",
                "y"
                );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\PizzaFormButton.script")]
        public async Task Pizza_Button_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us", () => PizzaOrder.BuildForm(style: ChoiceStyleOptions.Auto), FormOptions.None, new PizzaOrder(), Array.Empty<EntityRecommendation>(),
                "hi",
                "garbage",
                "2",
                "med",
                "4",
                "help",
                "drink bread",
                "back",
                "c",
                "garbage",
                "no",
                "thin",
                "1",
                "?",
                "garbage",
                "beef, onion, ice cream",
                "garbage",
                "onions",
                "status",
                "abc",
                "2",
                "garbage",
                "iowa",
                "y",
                "1 2",
                "none",
                "garbage",
                "2.5",
                "garbage",
                "2/25/1962 3pm",
                "no",
                "1234",
                "123-4567",
                "no",
                "toppings",
                "everything but spinach",
                "y"
                );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\PizzaForm-fr.script")]
        public async Task Pizza_fr_Script()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "fr", () => PizzaOrder.BuildForm(), FormOptions.None, new PizzaOrder(), Array.Empty<EntityRecommendation>(),
                "bonjour",
                "2",
                "moyen",
                "4",
                "?",
                "1 2",
                "retourner",
                "c",
                "non",
                "fine",
                "1",
                "?",
                "bovine, oignons, ice cream",
                "oignons",
                "statut",
                "abc",
                "1 state street",
                "oui",
                "1 2",
                "non",
                "2,5",
                "25/2/1962 3pm",
                "non",
                "1234",
                "123-4567",
                "non",
                "nappages",
                "non epinards",
                "oui"
                );
        }

        public class MyClass
        {
            [Prompt("I didn't get you")]
            public string xxx { get; set; }

            [Optional]
            public string yyy { get; set; }

            public static IForm<MyClass> Build()
            {
                return new FormBuilder<MyClass>()
                    .Message("Welcome")
                    .Field(nameof(xxx))
                    .Field(nameof(yyy), validate: async (state, value) =>
                        new ValidateResult() { IsValid = true })
                    .Build()
                    ;
            }
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\Optional.script")]
        public async Task Optional()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await VerifyFormScript(pathScript,
                "en-us", () => MyClass.Build(), FormOptions.None, new MyClass(), Array.Empty<EntityRecommendation>(),
                "ok",
                "This is something",
                ""
                );
        }

        [TestMethod]
        public async Task FormFlow_Localization()
        {
            // This ensures there are no bad templates in resources
            foreach (var locale in new string[] { "ar", "cs", "de", "en", "es", "fa", "fr", "it", "ja", "pt-BR", "ru", "zh-Hans", "cs", "de-DE" })
            {
                var root = new FormDialog<PizzaOrder>(new PizzaOrder(), () => PizzaOrder.BuildForm(), cultureInfo: CultureInfo.GetCultureInfo(locale));
                Assert.AreNotEqual(null, root);
            }
        }

        [TestMethod]
        public async Task Form_Can_Fill_In_Scalar_Types()
        {
            IEnumerable<EntityRecommendation> entities = Enumerable.Empty<EntityRecommendation>();
            await RunScriptAgainstForm(entities,
                    "hello",
                    "Please enter text ",
                    Input.Text,
                    "Please enter a number for integer (current choice: 0)",
                    Input.Integer.ToString(),
                    "Please enter a number for float (current choice: 0)",
                    Input.Float.ToString()
                );
        }

        [TestMethod]
        public async Task Form_Can_Handle_Luis_Entity()
        {
            IEnumerable<EntityRecommendation> entities = new[] { new EntityRecommendation(type: nameof(IFormTarget.Text), entity: Input.Text) };
            await RunScriptAgainstForm(entities,
                    "hello",
                    "Please enter a number for integer (current choice: 0)",
                    Input.Integer.ToString(),
                    "Please enter a number for float (current choice: 0)",
                    Input.Float.ToString()
                );
        }

        [TestMethod]
        public async Task Form_Can_Handle_Irrelevant_Luis_Entity()
        {
            IEnumerable<EntityRecommendation> entities = new[] { new EntityRecommendation(type: "some random entity", entity: Input.Text) };
            await RunScriptAgainstForm(entities,
                    "hello",
                    "Please enter text ",
                    Input.Text,
                    "Please enter a number for integer (current choice: 0)",
                    Input.Integer.ToString(),
                    "Please enter a number for float (current choice: 0)",
                    Input.Float.ToString()
                );
        }

        [TestMethod]
        [DeploymentItem(@"Scripts\Form_Term_Matching.script")]
        public async Task Form_Term_Matching()
        {
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            // [Terms("word", @"\bpword\(123\)", @"32 jump\b")]
            await VerifyFormScript(pathScript,
                "en-us", () => new FormBuilder<SimpleForm>().Build(), FormOptions.None, new SimpleForm(), Array.Empty<EntityRecommendation>(),
                "Hi",

                "some choices",
                "aword",
                "wordb",
                "word",

                "back",
                "3pword(123)",
                "pword(123)",

                "back",
                "32 jumped",
                "32 jump",

                "back",
                "this word",

                "back",
                "word that",

                "back",
                "-word",

                "back",
                "word-"
                );
        }

#if FORMFLOWJSON
        [TestMethod]
        public async Task CanResolveDynamicFormFromContainer()
        {
            // This test has two purposes.
            // 1. show that IFormDialog can be resolved from the container
            // 2. show that json schema forms can be dynamically generated based on the incoming message
            // You will likely find that the extensibility in IForm's callback methods may be sufficient enough for most scenarios.

            using (var container = Build(Options.ResolveDialogFromContainer))
            {
                var builder = new ContainerBuilder();

                // make a dynamic IForm model based on the incoming message
                builder
                    .Register(c =>
                    {
                        var message = c.Resolve<IMessageActivity>();

                        // use the user's name as the prompt
                        const string TEMPLATE_PREFIX =
                        @"
                        {
                          'type': 'object',
                          'properties': {
                            'name': {
                              'type': 'string',
                              'Prompt': { 'Patterns': [ '";

                        const string TEMPLATE_SUFFIX =
                        @"' ] },
                            }
                          }
                        }
                        ";

                        var text = TEMPLATE_PREFIX + message.From.Id + TEMPLATE_SUFFIX;
                        var schema = JObject.Parse(text);

                        return
                            new FormBuilderJson(schema)
                            .AddRemainingFields()
                            .Build();
                    })
                    .As<IForm<JObject>>()
                    // lifetime must match lifetime scope tag of Message, since we're dependent on the Message
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

                builder
                    .Register<BuildFormDelegate<JObject>>(c =>
                    {
                        var cc = c.Resolve<IComponentContext>();
                        return () => cc.Resolve<IForm<JObject>>();
                    })
                    // tell the serialization framework to recover this delegate from the container
                    // rather than trying to serialize it with the dialog
                    // normally, this delegate is a static method that is trivially serializable without any risk of a closure capturing the environment
                    .Keyed<BuildFormDelegate<JObject>>(FiberModule.Key_DoNotSerialize)
                    .AsSelf()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

                builder
                    .RegisterType<FormDialog<JObject>>()
                    // root dialog is an IDialog<object>
                    .As<IDialog<object>>()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

                builder
                    // our default form state
                    .Register<JObject>(c => new JObject())
                    .AsSelf()
                    .InstancePerDependency();

                builder.Update(container);

                // verify that the form dialog prompt is dynamically generated from the incoming message
                await AssertScriptAsync(container,
                    "hello",
                    ChannelID.User
                    );
            }
        }
#endif

        private class TestFormAttribute
        {
            public string FieldNameWithoutAttributes { get; set; }

            [Describe(description: " ")]
            public string FieldNameWithDescribeSpaceDescriptionAttributeOnly { get; set; }

            [Describe(description: "")]
            public string FieldNameWithDescribeEmptyDescriptionAttributeOnly { get; set; }

            [Describe(description: "FieldDescribeDescription1")]
            public string FieldNameWithDescribeDescriptionAttributeOnly { get; set; }

            [Describe(title: "FieldDescribeNullDescription1")]
            public string FieldNameWithDescribeNullDescriptionAttributeOnly { get; set; }

            [Describe("FieldName2")]
            [Terms("FieldName2")]
            public string FieldNameWithDescribeTermsAttributesSame { get; set; }

            [Describe("FieldDescribe3")]
            [Terms("FieldTerms3")]
            public string FieldNameWithDescribeTermsAttributesDiffer { get; set; }

            [Terms("FieldTerms4")]
            public string FieldNameWithTermsAttributeOnly { get; set; }

            public IForm<TestFormAttribute> FormBuilder;
            public TestFormAttribute()
            {
                FormBuilder = BuildForm();
            }

            public static IForm<TestFormAttribute> BuildForm()
            {
                return new FormBuilder<TestFormAttribute>()
                    .Message("Provide test field name:")
                    .Build();

            }


        }

        [TestMethod]
        public async Task VerifyFormBuilderDescribeTermsAttributes()
        {
            foreach (var field in (new TestFormAttribute()).FormBuilder.Fields)
            {
                if (field.Name == "FieldNameWithoutAttributes")
                {
                    Assert.IsTrue(field.FieldDescription.Description == "Field Name Without Attributes");
                    Assert.IsTrue(field.FieldTerms.Any(ft => ft.StartsWith(field.FieldDescription.Description.ToLower())));
                }
                else if (field.Name == "FieldNameWithDescribeSpaceDescriptionAttributeOnly")
                {
                    Assert.IsTrue(field.FieldDescription.Description == " ");
                    Assert.IsTrue(field.FieldTerms.Contains("field name with describe space description attribute only"));
                }
                else if (field.Name == "FieldNameWithDescribeEmptyDescriptionAttributeOnly")
                {
                    Assert.IsTrue(field.FieldDescription.Description == "");
                    Assert.IsTrue(field.FieldTerms.Contains("field name with describe empty description attribute only"));
                }
                else if (field.Name == "FieldNameWithDescribeDescriptionAttributeOnly")
                {
                    Assert.IsTrue(field.FieldDescription.Description == "FieldDescribeDescription1");
                    Assert.IsTrue(field.FieldTerms.Any(ft => ft.StartsWith(field.FieldDescription.Description.ToLower())));
                }
                else if (field.Name == "FieldNameWithDescribeNullDescriptionAttributeOnly")
                {
                    Assert.IsTrue(field.FieldDescription.Description == null);
                    Assert.IsTrue(field.FieldTerms.Contains("field name with describe null description attribute only"));
                }
                else if (field.Name == "FieldNameWithDescribeTermsAttributesSame")
                {
                    Assert.IsTrue(field.FieldDescription.Description == "FieldName2");
                    Assert.IsTrue(field.FieldTerms.Contains("FieldName2"));
                }
                else if (field.Name == "FieldNameWithDescribeTermsAttributesDiffer")
                {
                    Assert.IsTrue(field.FieldDescription.Description == "FieldDescribe3");
                    Assert.IsTrue(field.FieldTerms.Contains("FieldTerms3"));
                }
                else if (field.Name == "FieldNameWithTermsAttributeOnly")
                {
                    Assert.IsTrue(field.FieldDescription.Description == "Field Name With Terms Attribute Only");
                    Assert.IsTrue(field.FieldTerms.Contains("FieldTerms4"));
                }
            }
        }

    }
}
