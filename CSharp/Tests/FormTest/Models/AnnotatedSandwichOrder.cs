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

using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Builder.FormFlow.Json;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FormTest.Resource;

namespace FormTest.Models.AnnotatedSandwich
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
    public class AnnotatedSandwichOrder
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

        public static IForm<AnnotatedSandwichOrder> BuildForm()
        {
            OnCompletionAsyncDelegate<AnnotatedSandwichOrder> processOrder = async (context, state) =>
            {
                await context.PostAsync(new Activity() { Type = ActivityTypes.Message, Text = "We are currently processing your sandwich. We will message you the status." });
            };

            return new FormBuilder<AnnotatedSandwichOrder>()
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
                        .Field(nameof(AnnotatedSandwichOrder.Sauces))
                        .Field(new FieldReflector<AnnotatedSandwichOrder>(nameof(Specials))
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
                        .Field(nameof(AnnotatedSandwichOrder.DeliveryAddress),
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
                        .Field(nameof(AnnotatedSandwichOrder.DeliveryTime), "What time do you want your sandwich delivered? {||}")
                        .Confirm("Do you want to order your {Length} {Sandwich} on {Bread} {&Bread} with {[{Cheese} {Toppings} {Sauces}]} to be sent to {DeliveryAddress} {?at {DeliveryTime:t}}?")
                        .AddRemainingFields()
                        .Message("Thanks for ordering a sandwich!")
                        .OnCompletion(processOrder)
                        .Build();
        }

        public static IForm<JObject> BuildJsonForm()
        {
            // TODO: Remove
            var ass = Assembly.GetExecutingAssembly();
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FormTest.Resource.AnnotatedSandwich.json"))
            {
                var schema = JObject.Parse(new StreamReader(stream).ReadToEnd());
                return new FormBuilderJson(schema)
                    .AddRemainingFields()
                    .Build();
            }
        }

        public static IForm<JObject> BuildJsonFormExplicit()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FormTest.Resource.AnnotatedSandwich.json"))
            {
                var schema = JObject.Parse(new StreamReader(stream).ReadToEnd());
                OnCompletionAsyncDelegate<JObject> processOrder = async (context, state) =>
                {
                    await context.PostAsync(new Activity() { Type = ActivityTypes.Message, Text = DynamicSandwich.Processing });
                };
                var builder = new FormBuilderJson(schema);
                return builder
                            .Message("Welcome to the sandwich order bot!")
                            .Field("Sandwich")
                            .Field("Length")
                            .Field("Ingredients.Bread")
                            .Field("Ingredients.Cheese")
                            .Field("Ingredients.Toppings",
                            validate: async (state, response) =>
                            {
                                var value = (IList<object>)response;
                                var result = new ValidateResult() { IsValid = true };
                                if (value != null && value.Contains("Everything"))
                                {
                                    result.Value = (from topping in new string[] {
                                    "Avocado", "BananaPeppers", "Cucumbers", "GreenBellPeppers",
                                    "Jalapenos", "Lettuce", "Olives", "Pickles",
                                    "RedOnion", "Spinach", "Tomatoes"}
                                                    where !value.Contains(topping)
                                                    select topping).ToList();
                                }
                                else
                                {
                                    result.Value = value;
                                }
                                return result;
                            }
                            )
                            .Message("For sandwich toppings you have selected {Ingredients.Toppings}.")
                            .Field("Ingredients.Sauces")
                            .Field(new FieldJson(builder, "Specials")
                                .SetType(null)
                                .SetActive((state) => (string)state["Length"] == "FootLong")
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
                                switch ((string)state["Length"])
                                {
                                    case "SixInch": cost = 5.0; break;
                                    case "FootLong": cost = 6.50; break;
                                }
                                return new PromptAttribute(string.Format(DynamicSandwich.Cost, cost));
                            })
                            .Field("DeliveryAddress",
                                validate: async (state, value) =>
                                {
                                    var result = new ValidateResult { IsValid = true, Value = value };
                                    var address = (value as string).Trim();
                                    if (address.Length > 0 && (address[0] < '0' || address[0] > '9'))
                                    {
                                        result.Feedback = DynamicSandwich.BadAddress;
                                        result.IsValid = false;
                                    }
                                    return result;
                                })
                            .Field("DeliveryTime", "What time do you want your sandwich delivered? {||}")
                            .Confirm("Do you want to order your {Length} {Sandwich} on {Ingredients.Bread} {&Ingredients.Bread} with {[{Ingredients.Cheese} {Ingredients.Toppings} {Ingredients.Sauces}]} to be sent to {DeliveryAddress} {?at {DeliveryTime:t}}?")
                            .AddRemainingFields()
                            .Message("Thanks for ordering a sandwich!")
                            .OnCompletion(processOrder)
                    .Build();
            }
        }

        // Cache of culture specific forms. 
        private static ConcurrentDictionary<CultureInfo, IForm<AnnotatedSandwichOrder>> _forms = new ConcurrentDictionary<CultureInfo, IForm<AnnotatedSandwichOrder>>();

        public static IForm<AnnotatedSandwichOrder> BuildLocalizedForm()
        {
            var culture = Thread.CurrentThread.CurrentUICulture;
            IForm<AnnotatedSandwichOrder> form;
            if (!_forms.TryGetValue(culture, out form))
            {
                OnCompletionAsyncDelegate<AnnotatedSandwichOrder> processOrder = async (context, state) =>
                {
                    await context.PostAsync(new Activity() { Type = ActivityTypes.Message, Text = DynamicSandwich.Processing });
                };
                // Form builder uses the thread culture to automatically switch framework strings
                // and also your static strings as well.  Dynamically defined fields must do their own localization.
                var builder = new FormBuilder<AnnotatedSandwichOrder>()
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
                        .Field(nameof(AnnotatedSandwichOrder.Sauces))
                        .Field(new FieldReflector<AnnotatedSandwichOrder>(nameof(Specials))
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
                        .Field(nameof(AnnotatedSandwichOrder.DeliveryAddress),
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
                        .Field(nameof(AnnotatedSandwichOrder.DeliveryTime), "What time do you want your sandwich delivered? {||}")
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
