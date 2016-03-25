using Microsoft.Bot.Builder.Form;
using System;
using System.Collections.Generic;
using System.Linq;
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
        OverRoastedChicken, RoastBeef,
        [Terms(@"rotis\w* style chicken", MaxPhrase = 3)]
        RotisserieStyleChicken, SpicyItalian, SteakAndCheese, SweetOnionTeriyaki, Tuna,
        TurkeyBreast, Veggie
    };
    public enum LengthOptions { SixInch, FootLong};
    public enum BreadOptions { NineGrainWheat, NineGrainHoneyOat, Italian, ItalianHerbsAndCheese, Flatbread };
    public enum CheeseOptions { American, MontereyCheddar, Pepperjack};
    public enum ToppingOptions {
        [Terms("except", "but", "not", "no", "all", "everything")]
        AllExcept,
        Avocado, BananaPeppers, Cucumbers, GreenBellPeppers, Jalapenos,
        Lettuce, Olives, Pickles, RedOnion, Spinach, Tomatoes};
    public enum SauceOptions { ChipotleSouthwest, HoneyMustard, LightMayonnaise, RegularMayonnaise,
        Mustard, Oil, Pepper, Ranch, SweetOnion, Vinegar };

    [Serializable]
    [Template(TemplateUsage.EnumSelectOne, "What kind of {&} would you like on your sandwich? {||}", ChoiceStyle = ChoiceStyleOptions.PerLine)]
    [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
    class SandwichOrder
    {
        [Prompt("What kind of {&} would you like? {||}")]
        // [Prompt("What kind of {&} would you like? {||}", ChoiceFormat ="{1}")]
        // [Prompt("What kind of {&} would you like?")]
        public SandwichOptions? Sandwich;

        [Prompt("What size of sandwich do you want? {||}")]
        public LengthOptions? Length;

        public BreadOptions? Bread;

        // An optional annotation means that it is possible to not make a choice in the field.
        [Optional]
        public CheeseOptions? Cheese;

        [Optional]
        public List<ToppingOptions> Toppings
        {
            get { return _toppings; }
            set
            {
                if (value.Contains(ToppingOptions.AllExcept))
                {
                    _toppings = (from topping in value
                                 where topping != ToppingOptions.AllExcept && !value.Contains(topping)
                                 select topping).ToList();
                }
                else
                {
                    _toppings = value;
                }
            }
        }
        private List<ToppingOptions> _toppings;

        [Optional]
        public List<SauceOptions> Sauces;
    };
}