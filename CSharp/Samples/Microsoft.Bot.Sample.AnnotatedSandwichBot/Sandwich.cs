using Microsoft.Bot.Builder.Form;
using System;
using System.Collections.Generic;
#pragma warning disable 649

// The SandwichOrder is the simple form you want to fill out.  It must be serializable so the bot can be stateless.
// The order of fields defines the default order in which questions will be asked.
// Enumerations shows the legal options for each field in the SandwichOrder and the order is the order values will be presented 
// in a conversation.
namespace Microsoft.Bot.Sample.AnnotatedSandwichBot
{
    public enum SandwichOptions
    {
        Unspecified, BLT, BlackForestHam, BuffaloChicken, ChickenAndBaconRanchMelt, ColdCutCombo, MeatballMarinara,
        OverRoastedChicken, RoastBeef, RotisserieStyleChicken, SpicyItalian, SteakAndCheese, SweetOnionTeriyaki, Tuna,
        TurkeyBreast, Veggie
    };
    public enum LengthOptions { Unspecified, SixInch, FootLong};
    public enum BreadOptions { Unspecified, NineGrainWheat, NineGrainHoneyOat, Italian, ItalianHerbsAndCheese, Flatbread };
    public enum CheeseOptions { Unspecifed, American, MontereyCheddar, Pepperjack};
    public enum ToppingOptions { Unspecified, Avocado, BananaPeppers, Cucumbers, GreenBellPeppers, Jalapenos,
        Lettuce, Olives, Pickles, RedOnion, Spinach, Tomatoes};
    public enum SauceOptions { Unspecified, ChipotleSouthwest, HoneyMustard, LightMayonnaise, RegularMayonnaise,
        Mustard, Oil, Pepper, Ranch, SweetOnion, Vinegar };

    [Serializable]
    class SandwichOrder
    {
        public SandwichOptions Sandwich;
        public LengthOptions Length;
        public BreadOptions Bread;

        // An optional annotation means that it is possible to not make a choice in the field.
        [Optional]
        public CheeseOptions Cheese;

        [Optional]
        public List<ToppingOptions> Toppings;

        [Optional]
        public List<SauceOptions> Sauces;
    };
}