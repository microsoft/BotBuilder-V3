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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
#pragma warning disable 649

using Autofac;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

using AnnotatedSandwichOrder = Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder;
using SimpleSandwichOrder = Microsoft.Bot.Sample.SimpleSandwichBot.SandwichOrder;
using System.Resources;

namespace Microsoft.Bot.Builder.FormFlowTest
{
    public enum DebugOptions
    {
        None, AnnotationsAndNumbers, AnnotationsAndNoNumbers, NoAnnotations, NoFieldOrder,
        WithState,
#if LOCALIZE
        Localized,
#endif
        SimpleSandwichBot, AnnotatedSandwichBot
    };
    [Serializable]
    public class Choices
    {
        public DebugOptions Choice;
    }

    class Program
    {

        static async Task Interactive<T>(IDialog<T> form)
        {
            var message = new Message()
            {
                ConversationId = Guid.NewGuid().ToString(),
                Text = ""
            };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule());
            builder
                .Register(c => new BotToUserTextWriter(new BotToUserQueue(message), Console.Out))
                .Keyed<IBotToUser>(FiberModule.Key_DoNotSerialize)
                .As<IBotToUser>()
                .SingleInstance();
            using (var container = builder.Build())
            {
                var store = container.Resolve<IDialogContextStore>(TypedParameter.From(message));

                Func<IDialog<T>> MakeRoot = () => form;

                await store.PollAsync(() => form);

                while (true)
                {
                    message.Text = await Console.In.ReadLineAsync();
                    await store.PostAsync(message, () => form);
                }
            }
        }

        private static IForm<PizzaOrder> BuildForm(bool noNumbers, bool ignoreAnnotations = false, bool localize = false)
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
                     return new PromptAttribute($"Your pizza will cost ${cost}");
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
                        .SetFieldDescription("Specials")
                        .SetFieldTerms("specials")
                        .RemoveValues();
                        if (state.NumberOfPizzas > 1)
                        {
                            specials
                                .SetAllowsMultiple(true)
                                .AddDescription("special1", "Free drink")
                                .AddTerms("special1", "drink");
                        }
                        specials
                            .AddDescription("special2", "Free garlic bread")
                            .AddTerms("special2", "bread", "garlic");
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
                        var result = new ValidateResult { IsValid = true };
                        var str = value as string;
                        if (str.Length == 0 || str[0] < '1' || str[0] > '9')
                        {
                            result.Feedback = "Address must start with number.";
                            result.IsValid = false;
                        }
                        else
                        {
                            result.Feedback = "Your address is fine.";
                        }
                        return result;
                    })
                 .Message(async (state) => { var cost = computeCost(state); return new PromptAttribute($"Your pizza will cost ${cost}"); })
                 .Confirm(async (state) => { var cost = computeCost(state); return new PromptAttribute($"Your pizza will cost ${cost} is that OK?"); })
                .AddRemainingFields()
                .Message("Rating = {Rating:F1} and [{Rating:F2}]")
                .Confirm("Would you like a {Size}, {[{BYO.Crust} {BYO.Sauce} {BYO.Toppings}]} pizza delivered to {DeliveryAddress}?", isBYO)
                .Confirm("Would you like a {Size}, {&Signature} {Signature} pizza delivered to {DeliveryAddress}?", isSignature, dependencies: new string[] { "Size", "Kind", "Signature" })
                .Confirm("Would you like a {Size}, {&GourmetDelite} {GourmetDelite} pizza delivered to {DeliveryAddress}?", isGourmet)
                .Confirm("Would you like a {Size}, {&Stuffed} {Stuffed} pizza delivered to {DeliveryAddress}?", isStuffed)
                .OnCompletionAsync(async (session, pizza) => Console.WriteLine("{0}", pizza))
                .Build();
#if LOCALIZE
            if (localize)
            {
                using (var stream = new FileStream("pizza.resx", FileMode.Create))
                using (var writer = new ResXResourceWriter(stream))
                {
                    form.SaveResources(writer);
                }
                Process.Start(new ProcessStartInfo(@"RView.exe", "pizza.resx -c en-uk -p t-") { UseShellExecute = false, CreateNoWindow = true }).WaitForExit();
                using (var stream = new FileStream("pizza-en-uk.resx", FileMode.Open))
                using (var reader = new ResXResourceReader(stream))
                {
                    IEnumerable<string> missing, extra;
                    form.Localize(reader, out missing, out extra);
                }
            }
#endif
            return form;
        }

        public static void TestValidate()
        {
            try
            {
                var form = new FormBuilder<PizzaOrder>()
                    .Message("{NotField}")
                    .Build();
                Debug.Fail("Validation failed");
            }
            catch (ArgumentException)
            {
            }
            try
            {
                var form = new FormBuilder<PizzaOrder>()
                    .Message("[{NotField}]")
                    .Build();
                Debug.Fail("Validation failed");
            }
            catch (ArgumentException)
            {
            }
            try
            {
                var form = new FormBuilder<PizzaOrder>()
                    .Message("{? {[{NotField}]}")
                    .Build();
                Debug.Fail("Validation failed");
            }
            catch (ArgumentException)
            {
            }
            try
            {
                var form = new FormBuilder<PizzaOrder>()
                    .Field(new FieldReflector<PizzaOrder>(nameof(PizzaOrder.Size))
                        .ReplaceTemplate(new TemplateAttribute(TemplateUsage.Double, "{Notfield}")))
                        .Build();
                Debug.Fail("Validation failed");
            }
            catch (ArgumentException)
            {
            }
        }

        public static IFormDialog<T> MakeForm<T>(BuildForm<T> buildForm) where T : class, new()
        {
            return new FormDialog<T>(new T(), buildForm, options: FormOptions.PromptInStart);
        }

        static void Main(string[] args)
        {
            // TestValidate();
            var callDebug =
                Chain
                .From(() => FormDialog.FromType<Choices>(FormOptions.PromptInStart))
                .ContinueWith<Choices, object>(async (context, result) =>
                {
                    Choices choices;
                    try
                    {
                        choices = await result;
                    }
                    catch (Exception error)
                    {
                        await context.PostAsync(error.ToString());
                        throw;
                    }

                    switch (choices.Choice)
                    {
                        case DebugOptions.AnnotationsAndNumbers:
                            return MakeForm(() => BuildForm(noNumbers: false));
                        case DebugOptions.AnnotationsAndNoNumbers:
                            return MakeForm(() => BuildForm(noNumbers: true));
                        case DebugOptions.NoAnnotations:
                            return MakeForm(() => BuildForm(noNumbers: true, ignoreAnnotations: true));
                        case DebugOptions.NoFieldOrder:
                            return MakeForm(() => new FormBuilder<PizzaOrder>().Build());
                        case DebugOptions.WithState:
                            return new FormDialog<PizzaOrder>(new PizzaOrder()
                            { Size = SizeOptions.Large, DeliveryAddress = "123 State", Kind = PizzaOptions.BYOPizza },
                            () => BuildForm(noNumbers: false), options: FormOptions.PromptInStart);
#if LOCALIZE
                        case DebugOptions.Localized:
                            return MakeForm(() => BuildForm(false, false, true));
#endif
                        case DebugOptions.SimpleSandwichBot:
                            return MakeForm(() => SimpleSandwichOrder.BuildForm());
                        case DebugOptions.AnnotatedSandwichBot:
                            return MakeForm(() => AnnotatedSandwichOrder.BuildForm());
                        default:
                            throw new NotImplementedException();
                    }
                })
                .Do(async (context, result) =>
                {
                    try
                    {
                        var item = await result;
                        Debug.WriteLine(item);
                    }
                    catch (FormCanceledException e)
                    {
                        if (e.InnerException == null)
                        {
                            await context.PostAsync($"Quit on {e.Last} step.");
                        }
                        else
                        {
                            await context.PostAsync($"Exception {e.Message} on step {e.Last}.");
                        }
                    }
                })
                .Loop();

            Interactive(callDebug).GetAwaiter().GetResult();
        }
    }
}
