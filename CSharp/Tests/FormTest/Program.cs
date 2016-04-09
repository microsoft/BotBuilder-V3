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
using System.Threading.Tasks;

using Autofac;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

using AnnotatedSandwichOrder = Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder;
using SimpleSandwichOrder = Microsoft.Bot.Sample.SimpleSandwichBot.SandwichOrder;

namespace Microsoft.Bot.Builder.FormFlowTest
{
    public enum DebugOptions
    {
        None, AnnotationsAndNumbers, AnnotationsAndNoNumbers, NoAnnotations, NoFieldOrder,
        WithState, 
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

        private static IForm<PizzaOrder> BuildForm(bool noNumbers, bool ignoreAnnotations = false)
        {
            var form = new FormBuilder<PizzaOrder>(ignoreAnnotations);

            ConditionalDelegate<PizzaOrder> isBYO = (pizza) => pizza.Kind == PizzaOptions.BYOPizza;
            ConditionalDelegate<PizzaOrder> isSignature = (pizza) => pizza.Kind == PizzaOptions.SignaturePizza;
            ConditionalDelegate<PizzaOrder> isGourmet = (pizza) => pizza.Kind == PizzaOptions.GourmetDelitePizza;
            ConditionalDelegate<PizzaOrder> isStuffed = (pizza) => pizza.Kind == PizzaOptions.StuffedPizza;
            // form.Configuration().DefaultPrompt.Feedback = FeedbackOptions.Always;
            if (noNumbers)
            {
                form.Configuration.DefaultPrompt.ChoiceFormat = "{1}";
                form.Configuration.DefaultPrompt.ChoiceCase = CaseNormalization.Lower;
                form.Configuration.DefaultPrompt.ChoiceParens = BoolDefault.False;
            }
            else
            {
                form.Configuration.DefaultPrompt.ChoiceFormat = "{0}. {1}";
            }
            return form
                .Message("Welcome to the pizza bot!!!")
                .Message("Lets make pizza!!!")
                .Field(nameof(PizzaOrder.NumberOfPizzas))
                .Field(nameof(PizzaOrder.Size))
                .Field(nameof(PizzaOrder.Kind))
                .Field("Size")
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
                .AddRemainingFields()
                .Message("Rating = {Rating:F1} and [{Rating:F2}]")
                .Confirm("Would you like a {Size}, {[{BYO.Crust} {BYO.Sauce} {BYO.Toppings}]} pizza delivered to {DeliveryAddress}?", isBYO)
                .Confirm("Would you like a {Size}, {&Signature} {Signature} pizza delivered to {DeliveryAddress}?", isSignature, dependencies: new string[] { "Size", "Kind", "Signature" })
                .Confirm("Would you like a {Size}, {&GourmetDelite} {GourmetDelite} pizza delivered to {DeliveryAddress}?", isGourmet)
                .Confirm("Would you like a {Size}, {&Stuffed} {Stuffed} pizza delivered to {DeliveryAddress}?", isStuffed)
                .OnCompletionAsync(async (session, pizza) => Console.WriteLine("{0}", pizza))
                .Build();
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
            catch (ArgumentException )
            {
            }
        }

        public static IFormDialog<T> MakeForm<T>(BuildForm<T> buildForm) where T : class, new()
        {
            return new FormDialog<T>(new T(), buildForm, options: FormOptions.PromptInStart);
        }


        [Serializable]
        public class MyBot : IDialog
        {
            async Task IDialog<object>.StartAsync(IDialogContext context)
            {
                context.Call<TopChoice>(new FormDialog<TopChoice>(new TopChoice()), WhatDoYouWant);
            }

            public async Task WhatDoYouWant(IDialogContext context, IAwaitable<TopChoice> choices)
            {
                switch ((await choices).Choice.Value)
                {
                    case TopChoices.Joke:
                        context.Call<ChooseJoke>(new FormDialog<ChooseJoke>(new ChooseJoke(), options: FormOptions.PromptInStart),
                            TellAJoke);
                        break;
                    default:
                        await context.PostAsync("I don't understand");
                        context.Call<TopChoice>(new FormDialog<TopChoice>(new TopChoice(), options:FormOptions.PromptInStart), WhatDoYouWant);
                        break;
                }
            }

            public async Task TellAJoke(IDialogContext context, IAwaitable<ChooseJoke> joke)
            {
                switch ((await joke).KindOfJoke)
                {
                    case TypeOfJoke.Funny:
                        await context.PostAsync("Something funny");
                        break;
                    case TypeOfJoke.KnockKnock:
                        await context.PostAsync("Knock-knock...");
                        break;
                }
                context.Call<TopChoice>(new FormDialog<TopChoice>(new TopChoice(), options:FormOptions.PromptInStart), WhatDoYouWant);
            }
        }

        public enum TopChoices { Joke, Weather }

        [Serializable]
        public class TopChoice
        {
            public TopChoices? Choice;
        }

        public enum TypeOfJoke { Funny, KnockKnock };

        [Serializable]
        public class ChooseJoke
        {
            public TypeOfJoke? KindOfJoke;
        }

        [Serializable]
        public class NullDialog<T> : IDialog<T>
        {
            public async Task StartAsync(IDialogContext context)
            {
                context.Done<T>(default(T));
            }
        }

        static void Main(string[] args)
        {
            var callJoke = Chain
                .From(() => new FormDialog<TopChoice>(new TopChoice(), options:FormOptions.PromptInStart))
                .ContinueWith<TopChoice, object>(async (context, result) =>
                {
                    switch ((await result).Choice)
                    {
                        case TopChoices.Joke: return new FormDialog<ChooseJoke>(new ChooseJoke(), options: FormOptions.PromptInStart);
                        default:
                            await context.PostAsync("I don't understand");
                            return new NullDialog<object>();
                    }
                })
                .ContinueWith<object, object>(async (context, result) =>
                {
                    var choice = await result;
                    if (choice is ChooseJoke)
                    {
                        switch ((choice as ChooseJoke).KindOfJoke)
                        {
                            case TypeOfJoke.Funny:
                                await context.PostAsync("Something funny");
                                break;
                            case TypeOfJoke.KnockKnock:
                                await context.PostAsync("Knock-knock...");
                                break;
                        }
                    }
                    return new NullDialog<object>();
                });
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
                            () => BuildForm(noNumbers: false), options:FormOptions.PromptInStart);
                        case DebugOptions.SimpleSandwichBot:
                            return MakeForm(() => SimpleSandwichOrder.BuildForm());
                        case DebugOptions.AnnotatedSandwichBot:
                            return MakeForm(() => AnnotatedSandwichOrder.BuildForm());
                        default:
                            throw new NotImplementedException();
                    }
                })
                .Do(async result =>
                {
                    try
                    {
                        var item = await result;
                        Debug.WriteLine(item);
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("you cancelled");
                    }
                })
                .Loop();

            Interactive(callDebug).GetAwaiter().GetResult();
            /*
            var dialogs = new DialogCollection().Add(debugForm);
            var form = AddFields(new Form<PizzaOrder>("full"), noNumbers: true);
            Console.WriteLine("\nWith annotations and numbers\n");
            Interactive<Form<PizzaOrder>>(AddFields(new Form<PizzaOrder>("No numbers"), noNumbers: false));

            Console.WriteLine("With annotations and no numbers");
            Interactive<Form<PizzaOrder>>(form);

            Console.WriteLine("\nWith no annotations\n");
            Interactive<Form<PizzaOrder>>(AddFields(new Form<PizzaOrder>("No annotations", ignoreAnnotations: true), noNumbers: false));

            Console.WriteLine("\nWith no fields.\n");
            Interactive<Form<PizzaOrder>>(new Form<PizzaOrder>("No fields"));
            */
        }
    }
}
