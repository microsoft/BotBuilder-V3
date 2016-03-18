using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Form;
using Microsoft.Bot.Builder.Form.Advanced;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.FormTest
{
    public enum DebugOptions { None, AnnotationsAndNumbers, AnnotationsAndNoNumbers, NoAnnotations, NoFieldOrder };
    [Serializable]
    public class Choices
    {
        public DebugOptions Choice;
    }

    class Program
    {
        static void Interactive(IDialogCollection dialogs, IDialog form)
        {
            var message = new Message()
            {
                ConversationId = Guid.NewGuid().ToString(),
                Text = ""
            };
            string prompt;
            do
            {
                var task = ConsoleSession.MessageReceivedAsync(message, dialogs, form);
                var reply = task.GetAwaiter().GetResult();
                prompt = reply.Msg.Text;
                if (prompt != null)
                {
                    Console.WriteLine(prompt);
                    Console.Write("> ");
                    message.Text = Console.ReadLine();
                }
            } while (prompt != null);
        }

        static IForm<PizzaOrder> AddFields(IForm<PizzaOrder> form, bool noNumbers)
        {
            ConditionalDelegate<PizzaOrder> isBYO = (pizza) => pizza.Kind == PizzaOptions.BYOPizza;
            ConditionalDelegate<PizzaOrder> isSignature = (pizza) => pizza.Kind == PizzaOptions.SignaturePizza;
            ConditionalDelegate<PizzaOrder> isGourmet = (pizza) => pizza.Kind == PizzaOptions.GourmetDelitePizza;
            ConditionalDelegate<PizzaOrder> isStuffed = (pizza) => pizza.Kind == PizzaOptions.StuffedPizza;
            // form.Configuration().DefaultPrompt.Feedback = FeedbackOptions.Always;
            if (noNumbers)
            {
                form.Configuration().DefaultPrompt.Format = "{1}";
                form.Configuration().DefaultPrompt.AllowNumbers = BoolDefault.No;
            }
            else
            {
                form.Configuration().DefaultPrompt.Format = "{0}. {1}";
            }
            return form
                .Message("Welcome to the pizza bot!!!")
                .Message("Lets make pizza!!!")
                .Field("Name")
                .Field(nameof(PizzaOrder.NumberOfPizzas))
                .Field(nameof(PizzaOrder.Size))
                .Field(nameof(PizzaOrder.Kind))
                .Field("Size")
                .Field("BYO.Crust", isBYO)
                .Field("BYO.Sauce", isBYO)
                .Field("BYO.Toppings", isBYO)
                .Message("Almost there!!! {*filled}", isBYO)
                .Field(nameof(PizzaOrder.GourmetDelite), isGourmet)
                .Field(nameof(PizzaOrder.Signature), isSignature)
                .Field(nameof(PizzaOrder.Stuffed), isStuffed)
                .Message("What we have is a {?{Signature} signature pizza} {?{GourmetDelite} gourmet pizza} {?{Stuffed} {&Stuffed}} {?{?{BYO.Crust} {&BYO.Crust}} {?{BYO.Sauce} {&BYO.Sauce}} {?{BYO.Toppings}}} pizza")
                .AddRemainingFields()
                .Confirm("Would you like a {Size}, {[BYO.Crust BYO.Sauce BYO.Toppings]} pizza delivered to {DeliveryAddress}?", isBYO)
                .Confirm("Would you like a {Size}, {&Signature} {Signature} pizza delivered to {DeliveryAddress}?", isSignature, dependencies: new string[] { "Size", "Kind", "Signature" })
                .Confirm("Would you like a {Size}, {&GourmetDelite} {GourmetDelite} pizza delivered to {DeliveryAddress}?", isGourmet)
                .Confirm("Would you like a {Size}, {&Stuffed} {Stuffed} pizza delivered to {DeliveryAddress}?", isStuffed)
                .OnCompletion((session, pizza) => Console.WriteLine("{0}", pizza));
            ;
        }

        static void Main(string[] args)
        {
            var dialogs = new DialogCollection();
            var annotationsAndNumbers = AddFields(new Form<PizzaOrder>("AnnotationsAndNumbers"), false);
            var annotationsAndWords = AddFields(new Form<PizzaOrder>("AnnotationsAndWords"), true);
            var debugForm = new Form<Choices>("Choices").AddRemainingFields();
            var callDebug = new CallDialog("Root", debugForm, async (session, taskResult) =>
            {
                if (taskResult.Status == TaskStatus.RanToCompletion)
                {
                    var testResult = await taskResult as Choices;
                    if (testResult != null)
                    {
                        switch (testResult.Choice)
                        {
                            case DebugOptions.AnnotationsAndNumbers: return await session.BeginDialogAsync(annotationsAndNumbers, Tasks.Null);
                            case DebugOptions.AnnotationsAndNoNumbers: return await session.BeginDialogAsync(annotationsAndWords, Tasks.Null);
                        }
                    }
                }

                return await session.EndDialogAsync(dialogs.Get("Root"), taskResult);
            });
            dialogs.Add(annotationsAndNumbers).Add(annotationsAndWords).Add(callDebug).Add(debugForm);
            ;
            Interactive(dialogs, callDebug);
            // Interactive(dialogs, annotationsAndWords);
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
