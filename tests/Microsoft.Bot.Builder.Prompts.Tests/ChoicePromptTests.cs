using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Choice Prompts")]
    public class ChoicePromptTests
    {
        [TestMethod]
        public async Task ChoicePrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var dictionaryColors = new Dictionary<IEnumerable<string>, Color>()
                {
                    {
                        new string[] { "amber", "aureolin", "gold", "mustard", "vanilla", "yellow" },
                        Color.Yellow
                    },
                    {
                        new string[] { "azure", "blue", "blue-gray", "cerulean", "cyan", "indigo", "prussian blue", "turquoise" },
                        Color.Blue
                    },
                    {
                        new string[] { "amaranth", "auburn", "magenta", "raspberry", "rose", "ruby", "scarlet", "vermilion", "wine" },
                        Color.Red
                    }
                };

                var testPrompt = new ChoicePrompt<Color>(Culture.English, dictionaryColors);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await testPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var choiceResult = await testPrompt.Recognize(context);
                    if (choiceResult.Succeeded())
                    {
                        Assert.IsNotNull(choiceResult.Text);
                        await context.SendActivity($"{choiceResult.Value.Name}");
                    }
                    else
                        await context.SendActivity(choiceResult.Status.ToString());
                }
            })
                .Send("hello")
                    .AssertReply("Gimme:")
                .Send("tprussian tblue")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(".. prussian blue")
                    .AssertReply("Blue")
                .Send(".. auburn")
                    .AssertReply("Red")
                .Send(".. the first color")
                    .AssertReply("Yellow")
                .Send(".. 3")
                    .AssertReply("Red")
                .StartTest();
        }

        [TestMethod]
        public async Task ChoicePrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var dictionaryColors = new Dictionary<Regex, Color>()
                {
                    {
                        new Regex("(amber|aureolin|gold|beige|mustard|vanilla|yellow)"),
                        Color.Yellow
                    },
                    {
                        new Regex("(blue navy|azure|blue|blue-gray|cerulean|cyan|indigo|prussian blue|turquoise)"),
                        Color.Blue
                    },
                    {
                        new Regex("(amaranth|auburn|magenta|raspberry|rose|ruby|scarlet|vermilion|wine)"),
                        Color.Red
                    }
                };
                
                var testPrompt = new ChoicePrompt<Color>(Culture.English, dictionaryColors, async (ctx, result) =>
                {
                    if (ctx.Activity.Text.Contains("xxx"))
                        result.Status = PromptStatus.NotRecognized;
                });

                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await testPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var choiceResult = await testPrompt.Recognize(context);
                    if (choiceResult.Succeeded())
                        await context.SendActivity($"{choiceResult.Value.Name}");
                    else
                        await context.SendActivity(choiceResult.Status.ToString());
                }
            })
                .Send("hello")
                    .AssertReply("Gimme:")
                .Send(" prussian blue xxx")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(" beige xxx")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(" prussian blue ")
                    .AssertReply("Blue")
                .Send(" beige ")
                    .AssertReply("Yellow")
                .StartTest();
        }
    }
}
