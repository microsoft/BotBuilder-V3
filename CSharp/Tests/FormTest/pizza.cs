// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FormFlow;
#pragma warning disable 649

namespace Microsoft.Bot.Builder.FormFlowTest
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
        Unkown, SignaturePizza, GourmetDelitePizza, StuffedPizza,

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
                           where topping != ToppingOptions.All && !options.Contains(topping)
                           select topping).ToList();
            }
            return options;
        }
    };

    [Serializable]
    class PizzaOrder
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
        //         [Optional]
        // public List<CouponOptions> Coupons;
        [Optional]
        public CouponOptions Coupon;
        public string DeliveryAddress;
        [Numeric(1, 5)]
        [Optional]
        public double? Rating;
        public DateTime Available;
        public string Specials;

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
    };
}