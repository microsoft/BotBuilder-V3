using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Form;
#pragma warning disable 649

namespace Microsoft.Bot.Builder.FormTest
{
    public enum SizeOptions
    {
        // 0 value in enums is reserved for unknown values.  Either you can supply an explicit one or start enumeration at 1.
        // Unknown,
        [Terms(new string[] { "med", "medium" })]
        Medium,
        Large,

        [Terms(new string[] { "family", "extra large" })]
        Family
    };
    public enum PizzaOptions
    {
        Unkown, SignaturePizza, GourmetDelitePizza, StuffedPizza,

        [Terms(new string[] { "byo", "build your own" })]
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
        [Terms(new string[] { "traditional", "tomatoe?" })]
        Traditional = 1,

        CreamyGarlic, OliveOil
    };

    public enum ToppingOptions
    {
        [Terms(new string[] { "except", "but", "not", "no", "all", "everything" })]
        [Describe("All except")]
        All = 1,
        Beef,
        BlackOlives,
        CanadianBacon,
        CrispyBacon,
        Garlic,
        GreenPeppers,
        GrilledChicken,

        [Terms(new string[] { "herb & cheese", "herb and cheese", "herb and cheese blend", "herb" })]
        HerbAndCheeseBlend,

        ItalianSausage,
        ArtichokeHearts,
        MixedOnions,

        [Terms("MozzarellaCheese", MaxPhrase = 2)]
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
    class BYOPizza
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
                         where !options.Contains(topping)
                         select topping).ToList();
            }
            return options;
        }
    };

    [Serializable]
    class PizzaOrder
    {
        [Numeric(0, 10)]
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
        //         [Optional]
        // public List<CouponOptions> Coupons;
        [Optional]
        public CouponOptions Coupon;
        public string DeliveryAddress;
        [Numeric(1, 5)]
        [Optional]
        public double? Rating;
        // public DateTime Available;

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
            builder.AppendFormat(", {0}, {1})", DeliveryAddress, Coupon);
            return builder.ToString();
        }
    };
}