using Microsoft.Bot.Builder.Classic.FormFlow;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
//using Microsoft.Bot.Builder.Classic.FormFlow.Json;
using Microsoft.Bot.Sample.AnnotatedSandwichBot.Resource;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
#pragma warning disable 649

// The SandwichOrder is the simple form you want to fill out.  It must be serializable so the bot can be stateless.
// The order of fields defines the default order in which questions will be asked.
// Enumerations shows the legal options for each field in the SandwichOrder and the order is the order values will be presented 
// in a conversation.
namespace Microsoft.Bot.Sample.AnnotatedSandwichBot
{
    public enum SandwichOptions
    {
        BLT, BlackForestHam, BuffaloChicken, ChickenAndBaconRanchMelt, ColdCutCombo, MeatballMarinara,
        OvenRoastedChicken, RoastBeef,
        [Terms(@"rotis\w* style chicken", MaxPhrase = 3)]
        RotisserieStyleChicken, SpicyItalian, SteakAndCheese, SweetOnionTeriyaki, Tuna,
        TurkeyBreast, Veggie
    };
    public enum LengthOptions { SixInch, FootLong };
    public enum BreadOptions
    {
        // Use an image if generating cards
        // [Describe(Image = @"https://placeholdit.imgix.net/~text?txtsize=12&txt=Special&w=100&h=40&txttrack=0&txtclr=000&txtfont=bold")]
        NineGrainWheat,
        NineGrainHoneyOat,
        Italian,
        ItalianHerbsAndCheese,
        Flatbread
    };
    public enum CheeseOptions { American, MontereyCheddar, Pepperjack };
    public enum ToppingOptions
    {
        // This starts at 1 because 0 is the "no value" value
        [Terms("except", "but", "not", "no", "all", "everything")]
        Everything = 1,
        Avocado, BananaPeppers, Cucumbers, GreenBellPeppers, Jalapenos,
        Lettuce, Olives, Pickles, RedOnion, Spinach, Tomatoes
    };
    public enum SauceOptions
    {
        ChipotleSouthwest, HoneyMustard, LightMayonnaise, RegularMayonnaise,
        Mustard, Oil, Pepper, Ranch, SweetOnion, Vinegar
    };

    [Serializable]
    [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
    [Template(TemplateUsage.EnumSelectOne, "What kind of {&} would you like on your sandwich? {||}")]
    // [Template(TemplateUsage.EnumSelectOne, "What kind of {&} would you like on your sandwich? {||}", ChoiceStyle = ChoiceStyleOptions.PerLine)]
    public class SandwichOrder
    {
        [Prompt("What kind of {&} would you like? {||}")]
        [Describe(Image = @"https://placeholdit.imgix.net/~text?txtsize=16&txt=Sandwich&w=125&h=40&txttrack=0&txtclr=000&txtfont=bold")]
        // [Prompt("What kind of {&} would you like? {||}", ChoiceFormat ="{1}")]
        // [Prompt("What kind of {&} would you like?")]
        public SandwichOptions? Sandwich;

        [Prompt("What size of sandwich do you want? {||}")]
        public LengthOptions? Length;

        // Specify Title and SubTitle if generating cards
        [Describe(Title = "Sandwich Bot", SubTitle = "Bread Picker")]
        public BreadOptions? Bread;

        // An optional annotation means that it is possible to not make a choice in the field.
        [Optional]
        public CheeseOptions? Cheese;

        [Optional]
        public List<ToppingOptions> Toppings { get; set; }

        [Optional]
        public List<SauceOptions> Sauces;

        [Optional]
        [Template(TemplateUsage.NoPreference, "None")]
        public string Specials;

        public string DeliveryAddress;

        [Pattern(@"(\(\d{3}\))?\s*\d{3}(-|\s*)\d{4}")]
        public string PhoneNumber;

        [Optional]
        [Template(TemplateUsage.StatusFormat, "{&}: {:t}", FieldCase = CaseNormalization.None)]
        public DateTime? DeliveryTime;

        [Numeric(1, 5)]
        [Optional]
        [Describe("your experience today")]
        public double? Rating;

        public static IForm<SandwichOrder> BuildForm()
        {
            OnCompletionAsyncDelegate<SandwichOrder> processOrder = async (context, state) =>
            {
                await context.PostAsync("We are currently processing your sandwich. We will message you the status.");
            };

            return new FormBuilder<SandwichOrder>()
                        .Message("Welcome to the sandwich order bot!")
                        .Field(nameof(Sandwich))
                        .Field(nameof(Length))
                        .Field(nameof(Bread))
                        .Field(nameof(Cheese))
                        .Field(nameof(Toppings),
                            validate: async (state, value) =>
                            {
                                var values = ((List<object>)value)?.OfType<ToppingOptions>();
                                var result = new ValidateResult { IsValid = true, Value = values };
                                if (values != null && values.Contains(ToppingOptions.Everything))
                                {
                                    result.Value = (from ToppingOptions topping in Enum.GetValues(typeof(ToppingOptions))
                                                    where topping != ToppingOptions.Everything && !values.Contains(topping)
                                                    select topping).ToList();
                                }
                                return result;
                            })
                        .Message("For sandwich toppings you have selected {Toppings}.")
                        .Field(nameof(SandwichOrder.Sauces))
                        .Field(new FieldReflector<SandwichOrder>(nameof(Specials))
                            .SetType(null)
                            .SetActive((state) => state.Length == LengthOptions.FootLong)
                            .SetDefine(async (state, field) =>
                            {
                                field
                                    .AddDescription("cookie", "Free cookie")
                                    .AddTerms("cookie", "cookie", "free cookie")
                                    .AddDescription("drink", "Free large drink")
                                    .AddTerms("drink", "drink", "free drink");
                                return true;
                            }))
                        .Confirm(async (state) =>
                        {
                            var cost = 0.0;
                            switch (state.Length)
                            {
                                case LengthOptions.SixInch: cost = 5.0; break;
                                case LengthOptions.FootLong: cost = 6.50; break;
                            }
                            return new PromptAttribute($"Total for your sandwich is {cost:C2} is that ok? {{||}}");
                        })
                        .Field(nameof(SandwichOrder.DeliveryAddress),
                            validate: async (state, response) =>
                            {
                                var result = new ValidateResult { IsValid = true, Value = response };
                                var address = (response as string).Trim();
                                if (address.Length > 0 && (address[0] < '0' || address[0] > '9'))
                                {
                                    result.Feedback = "Address must start with a number.";
                                    result.IsValid = false;
                                }
                                return result;
                            })
                        .Field(nameof(SandwichOrder.DeliveryTime), "What time do you want your sandwich delivered? {||}")
                        .Confirm("Do you want to order your {Length} {Sandwich} on {Bread} {&Bread} with {[{Cheese} {Toppings} {Sauces}]} to be sent to {DeliveryAddress} {?at {DeliveryTime:t}}?")
                        .AddRemainingFields()
                        .Message("Thanks for ordering a sandwich!")
                        .OnCompletion(processOrder)
                        .Build();
        }

        //public static IForm<JObject> BuildJsonForm()
        //{
        //    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Bot.Sample.AnnotatedSandwichBot.AnnotatedSandwich.json"))
        //    {
        //        var schema = JObject.Parse(new StreamReader(stream).ReadToEnd());
        //        return new FormBuilderJson(schema)
        //            .AddRemainingFields()
        //            .Build();
        //    }
        //}

        //public static IForm<JObject> BuildJsonFormExplicit()
        //{
        //    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Bot.Sample.AnnotatedSandwichBot.AnnotatedSandwich.json"))
        //    {
        //        var schema = JObject.Parse(new StreamReader(stream).ReadToEnd());
        //        OnCompletionAsyncDelegate<JObject> processOrder = async (context, state) =>
        //        {
        //            await context.PostAsync(DynamicSandwich.Processing);
        //        };
        //        var builder = new FormBuilderJson(schema);
        //        return builder
        //                    .Message("Welcome to the sandwich order bot!")
        //                    .Field("Sandwich")
        //                    .Field("Length")
        //                    .Field("Ingredients.Bread")
        //                    .Field("Ingredients.Cheese")
        //                    .Field("Ingredients.Toppings",
        //                    validate: async (state, response) =>
        //                    {
        //                        var value = (IList<object>)response;
        //                        var result = new ValidateResult() { IsValid = true };
        //                        if (value != null && value.Contains("Everything"))
        //                        {
        //                            result.Value = (from topping in new string[] {
        //                            "Avocado", "BananaPeppers", "Cucumbers", "GreenBellPeppers",
        //                            "Jalapenos", "Lettuce", "Olives", "Pickles",
        //                            "RedOnion", "Spinach", "Tomatoes"}
        //                                            where !value.Contains(topping)
        //                                            select topping).ToList();
        //                        }
        //                        else
        //                        {
        //                            result.Value = value;
        //                        }
        //                        return result;
        //                    }
        //                    )
        //                    .Message("For sandwich toppings you have selected {Ingredients.Toppings}.")
        //                    .Field("Ingredients.Sauces")
        //                    .Field(new FieldJson(builder, "Specials")
        //                        .SetType(null)
        //                        .SetActive((state) => (string)state["Length"] == "FootLong")
        //                        .SetDefine(async (state, field) =>
        //                        {
        //                            field
        //                            .AddDescription("cookie", DynamicSandwich.FreeCookie)
        //                            .AddTerms("cookie", Language.GenerateTerms(DynamicSandwich.FreeCookie, 2))
        //                            .AddDescription("drink", DynamicSandwich.FreeDrink)
        //                            .AddTerms("drink", Language.GenerateTerms(DynamicSandwich.FreeDrink, 2));
        //                            return true;
        //                        }))
        //                    .Confirm(async (state) =>
        //                    {
        //                        var cost = 0.0;
        //                        switch ((string)state["Length"])
        //                        {
        //                            case "SixInch": cost = 5.0; break;
        //                            case "FootLong": cost = 6.50; break;
        //                        }
        //                        return new PromptAttribute(string.Format(DynamicSandwich.Cost, cost));
        //                    })
        //                    .Field("DeliveryAddress",
        //                        validate: async (state, value) =>
        //                        {
        //                            var result = new ValidateResult { IsValid = true, Value = value };
        //                            var address = (value as string).Trim();
        //                            if (address.Length > 0 && (address[0] < '0' || address[0] > '9'))
        //                            {
        //                                result.Feedback = DynamicSandwich.BadAddress;
        //                                result.IsValid = false;
        //                            }
        //                            return result;
        //                        })
        //                    .Field("DeliveryTime", "What time do you want your sandwich delivered? {||}")
        //                    .Confirm("Do you want to order your {Length} {Sandwich} on {Ingredients.Bread} {&Ingredients.Bread} with {[{Ingredients.Cheese} {Ingredients.Toppings} {Ingredients.Sauces}]} to be sent to {DeliveryAddress} {?at {DeliveryTime:t}}?")
        //                    .AddRemainingFields()
        //                    .Message("Thanks for ordering a sandwich!")
        //                    .OnCompletion(processOrder)
        //            .Build();
        //    }
        //}

        // Cache of culture specific forms. 
        private static ConcurrentDictionary<CultureInfo, IForm<SandwichOrder>> _forms = new ConcurrentDictionary<CultureInfo, IForm<SandwichOrder>>();

        public static IForm<SandwichOrder> BuildLocalizedForm()
        {
            var culture = Thread.CurrentThread.CurrentUICulture;
            IForm<SandwichOrder> form;
            if (!_forms.TryGetValue(culture, out form))
            {
                OnCompletionAsyncDelegate<SandwichOrder> processOrder = async (context, state) =>
                                {
                                    await context.PostAsync(DynamicSandwich.Processing);
                                };
                // Form builder uses the thread culture to automatically switch framework strings
                // and also your static strings as well.  Dynamically defined fields must do their own localization.
                var builder = new FormBuilder<SandwichOrder>()
                        .Message("Welcome to the sandwich order bot!")
                        .Field(nameof(Sandwich))
                        .Field(nameof(Length))
                        .Field(nameof(Bread))
                        .Field(nameof(Cheese))
                        .Field(nameof(Toppings),
                            validate: async (state, value) =>
                            {
                                var values = ((List<object>)value)?.OfType<ToppingOptions>();
                                var result = new ValidateResult { IsValid = true, Value = values };
                                if (values != null && values.Contains(ToppingOptions.Everything))
                                {
                                    result.Value = (from ToppingOptions topping in Enum.GetValues(typeof(ToppingOptions))
                                                    where topping != ToppingOptions.Everything && !values.Contains(topping)
                                                    select topping).ToList();
                                }
                                return result;
                            })
                        .Message("For sandwich toppings you have selected {Toppings}.")
                        .Field(nameof(SandwichOrder.Sauces))
                        .Field(new FieldReflector<SandwichOrder>(nameof(Specials))
                            .SetType(null)
                            .SetActive((state) => state.Length == LengthOptions.FootLong)
                            .SetDefine(async (state, field) =>
                                {
                                    field
                                        .AddDescription("cookie", DynamicSandwich.FreeCookie)
                                        .AddTerms("cookie", Language.GenerateTerms(DynamicSandwich.FreeCookie, 2))
                                        .AddDescription("drink", DynamicSandwich.FreeDrink)
                                        .AddTerms("drink", Language.GenerateTerms(DynamicSandwich.FreeDrink, 2));
                                    return true;
                                }))
                        .Confirm(async (state) =>
                            {
                                var cost = 0.0;
                                switch (state.Length)
                                {
                                    case LengthOptions.SixInch: cost = 5.0; break;
                                    case LengthOptions.FootLong: cost = 6.50; break;
                                }
                                return new PromptAttribute(string.Format(DynamicSandwich.Cost, cost) + "{||}");
                            })
                        .Field(nameof(SandwichOrder.DeliveryAddress),
                            validate: async (state, response) =>
                            {
                                var result = new ValidateResult { IsValid = true, Value = response };
                                var address = (response as string).Trim();
                                if (address.Length > 0 && address[0] < '0' || address[0] > '9')
                                {
                                    result.Feedback = DynamicSandwich.BadAddress;
                                    result.IsValid = false;
                                }
                                return result;
                            })
                        .Field(nameof(SandwichOrder.DeliveryTime), "What time do you want your sandwich delivered? {||}")
                        .Confirm("Do you want to order your {Length} {Sandwich} on {Bread} {&Bread} with {[{Cheese} {Toppings} {Sauces}]} to be sent to {DeliveryAddress} {?at {DeliveryTime:t}}?")
                        .AddRemainingFields()
                        .Message("Thanks for ordering a sandwich!")
                        .OnCompletion(processOrder);
                builder.Configuration.DefaultPrompt.ChoiceStyle = ChoiceStyleOptions.Auto;
                form = builder.Build();
                _forms[culture] = form;
            }
            return form;
        }
    };
}