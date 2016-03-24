using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Form;
using Microsoft.Bot.Builder.Form.Advanced;
using System.Threading.Tasks;

using SimpleSandwichOrder = Microsoft.Bot.Sample.SimpleSandwichBot.SandwichOrder;
using AnnotatedSandwichOrder = Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder;

namespace Microsoft.Bot.Builder.FormTest
{
    public enum DebugOptions { None, AnnotationsAndNumbers, AnnotationsAndNoNumbers, NoAnnotations, NoFieldOrder,
        SimpleSandwichBot, AnnotatedSandwichBot };
    [Serializable]
    public class Choices
    {
        public DebugOptions Choice;
    }

    class Program
    {
        static void Interactive<T>(IDialog<T> form)
        {
            var message = new Message()
            {
                ConversationId = Guid.NewGuid().ToString(),
                Text = ""
            };
            string prompt;
            do
            {
                var task = CompositionRoot.PostAsync(message, () => form);
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

        private static IFormModel<PizzaOrder> MakeModel(bool noNumbers, bool ignoreAnnotations = false)
        {
            var form = FormModelBuilder<PizzaOrder>.Start(ignoreAnnotations);

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
                    (state, value) =>
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
                .Confirm("Would you like a {Size}, {[{BYO.Crust} {BYO.Sauce} {BYO.Toppings}]} pizza delivered to {DeliveryAddress}?", isBYO)
                .Confirm("Would you like a {Size}, {&Signature} {Signature} pizza delivered to {DeliveryAddress}?", isSignature, dependencies: new string[] { "Size", "Kind", "Signature" })
                .Confirm("Would you like a {Size}, {&GourmetDelite} {GourmetDelite} pizza delivered to {DeliveryAddress}?", isGourmet)
                .Confirm("Would you like a {Size}, {&Stuffed} {Stuffed} pizza delivered to {DeliveryAddress}?", isStuffed)
                .OnCompletion((session, pizza) => Console.WriteLine("{0}", pizza))
                .Build();
        }

        static void Main(string[] args)
        {
            var choiceForm = new FormDialog<Choices>();
            var callDebug = new CallDialog<InitialState<Choices>, Choices>(choiceForm, async (root, context, result) =>
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
                            var form = new FormDialog<PizzaOrder>(() => MakeModel(noNumbers: false));
                            var initialState = new InitialState<PizzaOrder>() { PromptInStart = true };
                            context.Call<IFormDialog<PizzaOrder>, InitialState<PizzaOrder>, PizzaOrder>(form, initialState, root.CallChild);
                            return;
                        }
                    case DebugOptions.AnnotationsAndNoNumbers:
                        {
                            var form = new FormDialog<PizzaOrder>(() => MakeModel(noNumbers: true));
                            var initialState = new InitialState<PizzaOrder>() { PromptInStart = true };
                            context.Call<IFormDialog<PizzaOrder>, InitialState<PizzaOrder>, PizzaOrder>(form, initialState, root.CallChild);
                            return;
                        }
                case DebugOptions.NoAnnotations:
                        {
                            var form = new FormDialog<PizzaOrder>(() => MakeModel(true, true));
                            var initialState = new InitialState<PizzaOrder>() { PromptInStart = true };
                            context.Call<IFormDialog<PizzaOrder>, InitialState<PizzaOrder>, PizzaOrder>(form, initialState, root.CallChild);
                            return;
                        }
                case DebugOptions.NoFieldOrder:
                        {
                            var form = new FormDialog<PizzaOrder>(() => FormModelBuilder<PizzaOrder>.Start().Build());
                            var initialState = new InitialState<PizzaOrder>() { PromptInStart = true };
                            context.Call<IFormDialog<PizzaOrder>, InitialState<PizzaOrder>, PizzaOrder>(form, initialState, root.CallChild);
                            return;
                        }
                    case DebugOptions.SimpleSandwichBot:
                        {
                            var form = new FormDialog<SimpleSandwichOrder>(() => FormModelBuilder<Microsoft.Bot.Sample.SimpleSandwichBot.SandwichOrder>.Start().Build());
                            var initialState = new InitialState<SimpleSandwichOrder>() { PromptInStart = true };
                            context.Call<IFormDialog<SimpleSandwichOrder>, InitialState<SimpleSandwichOrder>, SimpleSandwichOrder>(form, initialState, root.CallChild);
                            return;
                        }
                    case DebugOptions.AnnotatedSandwichBot:
                        {
                            var form = new FormDialog<AnnotatedSandwichOrder>(() => FormModelBuilder<AnnotatedSandwichOrder>.Start().Build());
                            var initialState = new InitialState<AnnotatedSandwichOrder>() { PromptInStart = true };
                            context.Call<IFormDialog<AnnotatedSandwichOrder>, InitialState<AnnotatedSandwichOrder>, AnnotatedSandwichOrder>(form, initialState, root.CallChild);
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
