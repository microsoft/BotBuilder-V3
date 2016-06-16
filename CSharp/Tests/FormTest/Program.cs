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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable 649

using Autofac;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Connector;

using AnnotatedSandwichOrder = Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder;
using SimpleSandwichOrder = Microsoft.Bot.Sample.SimpleSandwichBot.SandwichOrder;
using System.Resources;
using System.Text;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using System.Dynamic;

public class Globals
{
    public JObject state;
    public dynamic dstate;
    public object value;
    public IField<JObject> field;
}

namespace Microsoft.Bot.Builder.FormFlowTest
{
    public enum DebugOptions
    {
        None, AnnotationsAndNumbers, AnnotationsAndNoNumbers, AnnotationsAndText, NoAnnotations, NoFieldOrder,
        WithState,
        Localized,
        SimpleSandwichBot, AnnotatedSandwichBot, JSONSandwichBot
    };
    [Serializable]
    public class Choices
    {
        public DebugOptions Choice;
    }

    class Program
    {

        static public string Locale = CultureInfo.CurrentUICulture.Name;

        static async Task Interactive<T>(IDialog<T> form) where T : class
        {
            // NOTE: I use the DejaVuSansMono fonts as described here: http://stackoverflow.com/questions/21751827/displaying-arabic-characters-in-c-sharp-console-application
            // But you don't have to reboot.
            // If you don't want the multi-lingual support just comment this out
            Console.OutputEncoding = Encoding.GetEncoding(65001);
            var message = new Message()
            {
                From = new ChannelAccount { Id = "Console" },
                ConversationId = Guid.NewGuid().ToString(),
                To = new ChannelAccount { Id = "FormTest", IsBot = true },
                Text = ""
            };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule_MakeRoot());
            builder
                .Register(c => new BotToUserTextWriter(new BotToUserQueue(message, new Queue<Message>()), Console.Out))
                .As<IBotToUser>()
                .InstancePerLifetimeScope();
            using (var container = builder.Build())
            using (var scope = DialogModule.BeginLifetimeScope(container, message))
            {
                Func<IDialog<object>> MakeRoot = () => form;
                DialogModule_MakeRoot.Register(scope, MakeRoot);

                var task = scope.Resolve<IPostToBot>();
                var botData = scope.Resolve<IBotData>();
                await botData.LoadAsync();
                var stack = scope.Resolve<IDialogStack>();

                stack.Call(MakeRoot(), null);
                await stack.PollAsync(CancellationToken.None);

                while (true)
                {
                    message.Text = await Console.In.ReadLineAsync();
                    message.Language = Locale;
                    await task.PostAsync(message, CancellationToken.None);
                }
            }
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

        public static IFormDialog<T> MakeForm<T>(BuildFormDelegate<T> buildForm) where T : class, new()
        {
            return new FormDialog<T>(new T(), buildForm, options: FormOptions.PromptInStart);
        }

        public static async Task<T> Run<T>(Func<Task<T>> fun, string desc)
        {
            var memory = System.GC.GetTotalMemory(true);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var result = await fun();
            var end = timer.ElapsedMilliseconds;
            var endMemory = System.GC.GetTotalMemory(true);
            Console.WriteLine($"{desc}: {end}ms, total {endMemory}, delta {endMemory - memory}");
            return result;
        }

        public static void Run(string code, Globals globals, string prefix)
        {
            var options = Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .AddReferences(
                    typeof(JObject).Assembly,
                    typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly
                    )
                ;
            var script = Run<Script<bool>>(async () => CSharpScript.Create<bool>(code, options, typeof(Globals)), prefix + "Create").Result;
            var fun = Run<ScriptRunner<bool>>(async () => script.CreateDelegate(), prefix + "Delegate").Result;
            var cResult = Run<bool>(async () => await fun(globals), prefix + "Compiled").Result;
            var rResult = Run<ScriptState<bool>>(async () => await CSharpScript.RunAsync<bool>(code, options, globals), prefix + "Run").Result;
            var eResult = Run<bool>(async () => await CSharpScript.EvaluateAsync<bool>(code, options, globals), prefix + "Eval").Result;
        }

        public static bool NonWord(string word)
        {
            bool nonWord = true;
            foreach (var ch in word)
            {
                if (!(char.IsControl(ch) || char.IsPunctuation(ch) || char.IsWhiteSpace(ch)))
                {
                    nonWord = false;
                    break;
                }
            }
            return nonWord;
        }

        static void Main(string[] args)
        {
            // TestValidate();
            var callDebug =
                Chain
                .From(() => new PromptDialog.PromptString("Locale?", null, 1))
                .ContinueWith<string, Choices>(async (ctx, locale) =>
                    {
                        Locale = await locale;
                        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(Locale);
                        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
                        return FormDialog.FromType<Choices>(FormOptions.PromptInStart);
                    })
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
                            return MakeForm(() => PizzaOrder.BuildForm(noNumbers: false));
                        case DebugOptions.AnnotationsAndNoNumbers:
                            return MakeForm(() => PizzaOrder.BuildForm(noNumbers: true));
                        case DebugOptions.AnnotationsAndText:
                            return MakeForm(() => PizzaOrder.BuildForm(style: ChoiceStyleOptions.AutoText));
                        case DebugOptions.NoAnnotations:
                            return MakeForm(() => PizzaOrder.BuildForm(noNumbers: true, ignoreAnnotations: true));
                        case DebugOptions.NoFieldOrder:
                            return MakeForm(() => new FormBuilder<PizzaOrder>().Build());
                        case DebugOptions.WithState:
                            return new FormDialog<PizzaOrder>(new PizzaOrder()
                            { Size = SizeOptions.Large, Kind = PizzaOptions.BYOPizza },
                            () => PizzaOrder.BuildForm(noNumbers: false),
                            options: FormOptions.PromptInStart,
                            entities: new Luis.Models.EntityRecommendation[] {
                                new Luis.Models.EntityRecommendation("Address", "abc", "DeliveryAddress"),
                                new Luis.Models.EntityRecommendation("Toppings", "onions", "BYO.Toppings"),
                                new Luis.Models.EntityRecommendation("Toppings", "peppers", "BYO.Toppings"),
                                new Luis.Models.EntityRecommendation("Toppings", "ice", "BYO.Toppings"),
                                new Luis.Models.EntityRecommendation("NotFound", "OK", "Notfound")
                            }
                            );
                        case DebugOptions.Localized:
                            {
                                var form = PizzaOrder.BuildForm(false, false);
                                using (var stream = new FileStream("pizza.resx", FileMode.Create))
                                using (var writer = new ResXResourceWriter(stream))
                                {
                                    form.SaveResources(writer);
                                }
                                Process.Start(new ProcessStartInfo(@"RView.exe", "pizza.resx -c " + Locale) { UseShellExecute = false, CreateNoWindow = true }).WaitForExit();
                                return MakeForm(() => PizzaOrder.BuildForm(false, false, true));
                            }
                        case DebugOptions.SimpleSandwichBot:
                            return MakeForm(() => SimpleSandwichOrder.BuildForm());
                        case DebugOptions.AnnotatedSandwichBot:
                            return MakeForm(() => AnnotatedSandwichOrder.BuildLocalizedForm());
                        case DebugOptions.JSONSandwichBot:
                            return MakeForm(() => AnnotatedSandwichOrder.BuildJsonForm());
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
                .DefaultIfException()
                .Loop();
            Interactive(callDebug).GetAwaiter().GetResult();
        }
    }
}
