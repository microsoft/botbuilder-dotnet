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

using FormTest.Resource;
using Microsoft.Bot.Builder.Classic.FormFlow;
using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
using Microsoft.Bot.Builder.Classic.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
#pragma warning disable 649

namespace Microsoft.Bot.Builder.Classic.FormFlowTest
{
    public enum SizeOptions
    {
        // 0 value in enums is reserved for unknown values.  Either you can supply an explicit one or start enumeration at 1.
        // Unknown,
        [Terms("med", "medium")]
        Medium,
        Large,

        [Terms("family", "extra large")]
        Family
    };
    public enum PizzaOptions
    {
        Unknown, SignaturePizza, GourmetDelitePizza, StuffedPizza,

        [Terms("byo", "build your own")]
        [Describe("Build your own")]
        BYOPizza
    };
    public enum SignatureOptions { Hawaiian = 1, Pepperoni, MurphysCombo, ChickenGarlic, TheCowboy };
    public enum GourmetDeliteOptions { SpicyFennelSausage = 1, AngusSteakAndRoastedGarlic, GourmetVegetarian, ChickenBaconArtichoke, HerbChickenMediterranean };
    public enum StuffedOptions { ChickenBaconStuffed = 1, ChicagoStyleStuffed, FiveMeatStuffed };

    // Fresh Pan is large pizza only
    public enum CrustOptions
    {
        Original = 1, Thin, Stuffed, FreshPan, GlutenFree
    };

    public enum SauceOptions
    {
        [Terms("traditional", "tomatoe?")]
        Traditional = 1,

        CreamyGarlic, OliveOil
    };

    public enum ToppingOptions
    {
        [Terms("except", "but", "not", "no", "all", "everything")]
        [Describe("All except")]
        All = 1,
        Beef,
        BlackOlives,
        CanadianBacon,
        CrispyBacon,
        Garlic,
        GreenPeppers,
        GrilledChicken,

        [Terms("herb & cheese", "herb and cheese", "herb and cheese blend", "herb")]
        HerbAndCheeseBlend,

        ItalianSausage,
        ArtichokeHearts,
        MixedOnions,

        [Terms("Mozzarella Cheese", MaxPhrase = 2)]
        MozzarellaCheese,
        Mushroom,
        Onions,
        ParmesanCheese,
        Pepperoni,
        Pineapple,
        RomaTomatoes,
        Salami,
        Spinach,
        SunDriedTomatoes,
        Zucchini,
        ExtraCheese
    };

    public enum CouponOptions { None, Large20Percent, Pepperoni20Percent };

    [Serializable]
    public class BYOPizza
    {
        public CrustOptions Crust;
        public SauceOptions Sauce;

        private List<ToppingOptions> _toppings;
        public List<ToppingOptions> Toppings
        {
            get { return _toppings; }
            set
            {
                _toppings = _ProcessToppings(value);
            }
        }

        public bool HalfAndHalf;
        private List<ToppingOptions> _halfToppings;
        public List<ToppingOptions> HalfToppings
        {
            get
            {
                return _halfToppings;
            }
            set
            {
                _halfToppings = _ProcessToppings(value);
            }
        }

        private List<ToppingOptions> _ProcessToppings(List<ToppingOptions> options)
        {
            if (options != null && options.Contains(ToppingOptions.All))
            {
                options = (from ToppingOptions topping in Enum.GetValues(typeof(ToppingOptions))
                           where topping != ToppingOptions.All && !options.Contains(topping)
                           select topping).ToList();
            }
            return options;
        }
    };

    [Serializable]
    public class PizzaOrder
    {
        [Numeric(1, 10)]
        public int NumberOfPizzas = 1;
        [Optional]
        public SizeOptions? Size;
        // [Prompt("What kind of pizza do you want? {||}", Format = "{1}")]
        [Prompt("What kind of pizza do you want? {||}")]
        // [Prompt("What {&Kind} of pizza do you want? {||}", Name = "inline", Style = PromptStyle.Inline)]
        // [Prompt("What {&Kind} of pizza do you want? {||}", Name = "value", Format = "{1}")]
        [Template(TemplateUsage.NotUnderstood, new string[] { "What does \"{0}\" mean???", "Really \"{0}\"???" })]
        [Describe("Kind of pizza")]
        public PizzaOptions Kind;
        public SignatureOptions Signature;
        public GourmetDeliteOptions GourmetDelite;
        public StuffedOptions Stuffed;
        public BYOPizza BYO;
        [Optional]
        public List<CouponOptions> Coupons;
        [Optional]
        public CouponOptions Coupon;
        public string DeliveryAddress;
        [Numeric(1, 5)]
        [Optional]
        public double? Rating;
        public DateTime Available;
        [Optional]
        [Numeric(1, 3)]
        public int? Bottles;
        public List<string> Specials;

        [Pattern(@"(\(\d{3}\))?\s*\d{3}(-|\s*)\d{4}")]
        public string Phone;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("PizzaOrder({0}, ", Size);
            switch (Kind)
            {
                case PizzaOptions.BYOPizza:
                    builder.AppendFormat("{0}, {1}, {2}, [", Kind, BYO.Crust, BYO.Sauce);
                    foreach (var topping in BYO.Toppings)
                    {
                        builder.AppendFormat("{0} ", topping);
                    }
                    builder.AppendFormat("]");
                    if (BYO.HalfAndHalf)
                    {
                        builder.AppendFormat(", [");
                        foreach (var topping in BYO.HalfToppings)
                        {
                            builder.AppendFormat("{0} ", topping);
                        }
                        builder.Append("]");
                    }
                    break;
                case PizzaOptions.GourmetDelitePizza:
                    builder.AppendFormat("{0}, {1}", Kind, GourmetDelite);
                    break;
                case PizzaOptions.SignaturePizza:
                    builder.AppendFormat("{0}, {1}", Kind, Signature);
                    break;
                case PizzaOptions.StuffedPizza:
                    builder.AppendFormat("{0}, {1}", Kind, Stuffed);
                    break;
            }
            builder.AppendFormat(", {0}, {1}, {2})", DeliveryAddress, Coupon, Rating ?? 0.0);
            return builder.ToString();
        }

        public static IForm<PizzaOrder> BuildForm(bool noNumbers = false, bool ignoreAnnotations = false, bool localize = false, ChoiceStyleOptions style = ChoiceStyleOptions.AutoText)
        {
            var builder = new FormBuilder<PizzaOrder>(ignoreAnnotations);

            ActiveDelegate<PizzaOrder> isBYO = (pizza) => pizza.Kind == PizzaOptions.BYOPizza;
            ActiveDelegate<PizzaOrder> isSignature = (pizza) => pizza.Kind == PizzaOptions.SignaturePizza;
            ActiveDelegate<PizzaOrder> isGourmet = (pizza) => pizza.Kind == PizzaOptions.GourmetDelitePizza;
            ActiveDelegate<PizzaOrder> isStuffed = (pizza) => pizza.Kind == PizzaOptions.StuffedPizza;
            // form.Configuration().DefaultPrompt.Feedback = FeedbackOptions.Always;
            if (noNumbers)
            {
                builder.Configuration.DefaultPrompt.ChoiceFormat = "{1}";
                builder.Configuration.DefaultPrompt.ChoiceCase = CaseNormalization.Lower;
                builder.Configuration.DefaultPrompt.ChoiceParens = BoolDefault.False;
            }
            else
            {
                builder.Configuration.DefaultPrompt.ChoiceFormat = "{0}. {1}";
            }
            builder.Configuration.DefaultPrompt.ChoiceStyle = style;
            Func<PizzaOrder, double> computeCost = (order) =>
            {
                double cost = 0.0;
                switch (order.Size)
                {
                    case SizeOptions.Medium: cost = 10; break;
                    case SizeOptions.Large: cost = 15; break;
                    case SizeOptions.Family: cost = 20; break;
                }
                return cost;
            };
            MessageDelegate<PizzaOrder> costDelegate = async (state) =>
            {
                double cost = 0.0;
                switch (state.Size)
                {
                    case SizeOptions.Medium: cost = 10; break;
                    case SizeOptions.Large: cost = 15; break;
                    case SizeOptions.Family: cost = 20; break;
                }
                cost *= state.NumberOfPizzas;
                return new PromptAttribute(string.Format(DynamicPizza.Cost, cost));
            };
            var form = builder
                .Message("Welcome to the pizza bot!!!")
                .Message("Lets make pizza!!!")
                .Field(nameof(PizzaOrder.NumberOfPizzas))
                .Field(nameof(PizzaOrder.Size))
                .Field(nameof(PizzaOrder.Kind))
                .Field(new FieldReflector<PizzaOrder>(nameof(PizzaOrder.Specials))
                    .SetType(null)
                    .SetDefine(async (state, field) =>
                    {
                        var specials = field
                        .SetFieldDescription(DynamicPizza.Special)
                        .SetFieldTerms(DynamicPizza.SpecialTerms.SplitList())
                        .RemoveValues();
                        if (state.NumberOfPizzas > 1)
                        {
                            specials
                                .SetAllowsMultiple(true)
                                .AddDescription("special1", DynamicPizza.Special1)
                                .AddTerms("special1", DynamicPizza.Special1Terms.SplitList());
                        }
                        specials
                            .AddDescription("special2", DynamicPizza.Special2)
                            .AddTerms("special2", DynamicPizza.Special2Terms.SplitList());
                        return true;
                    }))
                .Field("BYO.HalfAndHalf", isBYO)
                .Field("BYO.Crust", isBYO)
                .Field("BYO.Sauce", isBYO)
                .Field("BYO.Toppings", isBYO)
                .Field("BYO.HalfToppings", (pizza) => isBYO(pizza) && pizza.BYO != null && pizza.BYO.HalfAndHalf)
                .Message("Almost there!!! {*filled}", isBYO)
                .Field(nameof(PizzaOrder.GourmetDelite), isGourmet)
                .Field(nameof(PizzaOrder.Signature), isSignature)
                .Field(nameof(PizzaOrder.Stuffed), isStuffed)

                .Message("What we have is a {?{Signature} signature pizza} {?{GourmetDelite} gourmet pizza} {?{Stuffed} {&Stuffed}} {?{?{BYO.Crust} {&BYO.Crust}} {?{BYO.Sauce} {&BYO.Sauce}} {?{BYO.Toppings}}} pizza")
                .Field("DeliveryAddress", validate:
                    async (state, value) =>
                    {
                        var result = new ValidateResult { IsValid = true, Value = value };
                        var str = value as string;
                        if (str.Length == 0 || str[0] < '1' || str[0] > '9')
                        {
                            result.Feedback = DynamicPizza.AddressHelp;
                            result.IsValid = false;
                        }
                        else
                        {
                            result.Feedback = DynamicPizza.AddressFine;
                            if (str == "1")
                            {
                                // Test to see if step is skipped
                                state.Phone = "111-1111";
                            }
                            else if (str == "2")
                            {
                                result.Choices = new List<Choice>{
                                    new Choice { Description = new DescribeAttribute("2 Iowa St"), Terms = new TermsAttribute("iowa"), Value = "2 Iowa St"},
                                    new Choice { Description = new DescribeAttribute("2 Kansas St"), Terms = new TermsAttribute("kansas"), Value = "2 Kansas St"}
                                };
                                result.IsValid = false;
                            }
                            else if (str == "3")
                            {
                                result.FeedbackCard = new FormPrompt()
                                {
                                    Prompt = "Secret place",
                                    Description = new DescribeAttribute(image: @"https://placeholdit.imgix.net/~text?txtsize=12&txt=secret&w=80&h=40&txttrack=0&txtclr=000&txtfont=bold")
                                };
                            }
                        }
                        return result;
                    })
                .Message(costDelegate)
                .Confirm(async (state) =>
                {
                    var cost = computeCost(state);
                    return new PromptAttribute(string.Format(DynamicPizza.CostConfirm, cost));
                })
                .AddRemainingFields()
                .Message("Rating = {Rating:F1} and [{Rating:F2}]")
                .Confirm("Would you like a {Size}, {[{BYO.Crust} {BYO.Sauce} {BYO.Toppings}]} pizza delivered to {DeliveryAddress}?", isBYO)
                .Confirm("Would you like a {Size}, {&Signature} {Signature} pizza delivered to {DeliveryAddress}?", isSignature, dependencies: new string[] { "Size", "Kind", "Signature" })
                .Confirm("Would you like a {Size}, {&GourmetDelite} {GourmetDelite} pizza delivered to {DeliveryAddress}?", isGourmet)
                .Confirm("Would you like a {Size}, {&Stuffed} {Stuffed} pizza delivered to {DeliveryAddress}?", isStuffed)
                .OnCompletion(async (session, pizza) => Console.WriteLine("{0}", pizza))
                .Build();
            if (localize)
            {
                using (var stream = new FileStream("pizza-" + Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName + ".resx", FileMode.Open))
                using (var reader = new ResXResourceReader(stream))
                {
                    IEnumerable<string> missing, extra;
                    form.Localize(reader.GetEnumerator(), out missing, out extra);
                }
            }
            return form;
        }
    };

}