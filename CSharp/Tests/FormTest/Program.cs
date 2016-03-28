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
using System.Diagnostics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Form;
using Microsoft.Bot.Builder.Form.Advanced;
using System.Threading.Tasks;

using SimpleSandwichOrder = Microsoft.Bot.Sample.SimpleSandwichBot.SandwichOrder;
using AnnotatedSandwichOrder = Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder;

namespace Microsoft.Bot.Builder.FormTest
{
    public enum DebugOptions
    {
        None, AnnotationsAndNumbers, AnnotationsAndNoNumbers, NoAnnotations, NoFieldOrder,
        SimpleSandwichBot, AnnotatedSandwichBot
    };
    [Serializable]
    public class Choices
    {
        public DebugOptions Choice;
    }

    class Program
    {
        static void Interactive(IDialog form)
        {
            var message = new Message()
            {
                ConversationId = Guid.NewGuid().ToString(),
                Text = ""
            };
            string prompt;
            do
            {
                var task = Conversation.SendAsync(message, () => form);
                message = task.GetAwaiter().GetResult();
                prompt = message.Text;
                if (prompt != null)
                {
                    Console.WriteLine(prompt);
                    Console.Write("> ");
                    message.Text = Console.ReadLine();
                }
            } while (prompt != null);
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
                        string feedback = null;
                        var str = value as string;
                        if (str.Length == 0 || str[0] < '1' || str[0] > '9')
                        {
                            feedback = "Address must start with number.";
                        }
                        return feedback;
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

        public static void Call<T>(IDialogContext context, CallDialog<Choices> root, BuildForm<T> buildForm) where T : class, new()
        {
            var form = new FormDialog<T>(new T(), buildForm, options: FormOptions.PromptInStart);
            context.Call<T>(form, root.CallChild);
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
                        .ReplaceTemplate(new Template(TemplateUsage.Double, "{Notfield}")))
                        .Build();
                Debug.Fail("Validation failed");
            }
            catch (ArgumentException exception)
            {
            }
        }

        static void Main(string[] args)
        {
            // TestValidate();
            var choiceForm = FormDialog.FromType<Choices>();
            var callDebug = new CallDialog<Choices>(choiceForm, async (root, context, result) =>
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
                        {
                            Call(context, root, () => BuildForm(noNumbers: false));
                            return;
                        }
                    case DebugOptions.AnnotationsAndNoNumbers:
                        {
                            Call(context, root, () => BuildForm(noNumbers: true));
                            return;
                        }
                    case DebugOptions.NoAnnotations:
                        {
                            Call(context, root, () => BuildForm(noNumbers: true, ignoreAnnotations: true));
                            return;
                        }
                    case DebugOptions.NoFieldOrder:
                        {
                            Call(context, root, () => new FormBuilder<PizzaOrder>().Build());
                            return;
                        }
                    case DebugOptions.SimpleSandwichBot:
                        {
                            Call(context, root, () => SimpleSandwichOrder.BuildForm());
                            return;
                        }
                    case DebugOptions.AnnotatedSandwichBot:
                        {
                            Call(context, root, () => AnnotatedSandwichOrder.BuildForm());
                            return;
                        }
                }

                context.Done(result);
            });

            Interactive(callDebug);
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
