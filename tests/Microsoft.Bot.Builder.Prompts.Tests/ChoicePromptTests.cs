using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Recognizers.Definitions.English;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
                var testPrompt = new ChoicePrompt<Color>(new ColorModel());
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
                var testPrompt = new ChoicePrompt<Color>(new ColorModel(), async (ctx, result) =>
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
                .Send(" cerulean ")
                    .AssertReply("Blue")
                .Send(" beige ")
                    .AssertReply("Yellow")
                .StartTest();
        }

        internal class ColorExtractorConfiguration : IChoiceExtractorConfiguration
        {
            public ColorExtractorConfiguration()
            {
                TokenRegex = new Regex(ChoiceDefinitions.TokenizerRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                MapRegexes = new Dictionary<Regex, string>()
                {
                    {
                        new Regex("amber|apricot|arylide yellow|aureolin|beige|buff|chartreuse|citrine|cream|dark goldenrod|ecru|flax|gold|gold (metallic)|goldenrod|harvest gold|jasmine|jonquil|khaki|lemon chiffon|lion|maize|mikado yellow|mustard|naples yellow|navajo white|old gold|olive|papaya whip|saffron|school bus yellow|selective yellow|stil de grain yellow|straw|sunglow|vanilla|yellow", RegexOptions.IgnoreCase | RegexOptions.Singleline),
                        "yellow"
                    },
                    {
                        new Regex("air force blue|air superiority blue|alice blue|azure|baby blue|bleu de france|blue|blue-gray|bondi blue|brandeis blue|byzantine blue|cambridge blue|carolina blue|celeste|cerulean|cobalt blue|columbia blue|cornflower blue|cyan|dark blue|deep sky blue|denim|dodger blue|duke blue|egyptian blue|electric blue|eton blue|federal blue|glaucous|electric indigo|indigo|international klein blue|iris|light blue|majorelle blue|marian blue|maya blue|medium blue|midnight blue|navy blue|non-photo blue|oxford blue|palatinate blue|periwinkle|persian blue|phthalo blue|powder blue|prussian blue|royal blue|sapphire|sky blue|steel blue|teal|tiffany blue|true blue|tufts blue|turquoise|ucla blue|ultramarine|violet-blue|viridian|yale blue", RegexOptions.IgnoreCase | RegexOptions.Singleline),
                        "blue"
                    },
                    {
                        new Regex("alizarin crimson|amaranth|american rose|auburn|blood red|burgundy|cardinal|carmine|carnelian|cerise|chocolate cosmos|cinnabar|coquelicot|crimson|dark red|electric crimson|fire brick|flame|folly|fuchsia|indian red|magenta|maroon|mahogany|mystic red|oxblood|persian red|pink|raspberry|red|red-violet|redwood|rose|rosewood|rosso corsa|ruby|rust|scarlet|terra cotta|turkey red|tuscan red|tyrian purple|venetian red|vermilion|wine", RegexOptions.IgnoreCase | RegexOptions.Singleline),
                        "red"
                    }
                };
            }

            public IDictionary<Regex, string> MapRegexes { get; private set; }
            public Regex TokenRegex { get; private set; }
            public bool AllowPartialMatch => false;
            public int MaxDistance => 2;
            public bool OnlyTopMatch => true;
        }

        internal class ColorParserConfiguration : IChoiceParserConfiguration<Color>
        {
            public ColorParserConfiguration()
            {
                Resolutions = new Dictionary<string, Color>()
                {
                    { "red", Color.Red },
                    { "blue", Color.Blue },
                    { "yellow", Color.Yellow }
                };
            }

            public IDictionary<string, Color> Resolutions { get; private set; }

        }

        internal class ColorModel : ChoiceModel
        {
            public override string ModelTypeName => "colour";

            public ColorModel() : base(
                new OptionsParser<Color>(new ColorParserConfiguration()),
                new ChoiceExtractor(new ColorExtractorConfiguration()))
            {
            }

            protected override SortedDictionary<string, object> GetResolution(ParseResult parseResult)
            {
                var data = parseResult.Data as OptionsParseDataResult;
                return new SortedDictionary<string, object>()
                {
                    { "value", parseResult.Value },
                    { "score", data.Score },
                    { "otherResults", data.OtherMatches.Select(l => new { l.Text, l.Value, l.Score })},
                };
            }
        }
    }
}
