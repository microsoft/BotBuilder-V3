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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace Microsoft.Bot.Builder.FormFlowTest
{
    public enum DebugOptions
    {
        None, AnnotationsAndNumbers, AnnotationsAndNoNumbers, NoAnnotations, NoFieldOrder,
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

        static async Task Interactive<T>(IDialog<T> form)
        {
            // NOTE: I use the DejaVuSansMono fonts as described here: http://stackoverflow.com/questions/21751827/displaying-arabic-characters-in-c-sharp-console-application
            // But you don't have to reboot.
            // If you don't want the multi-lingual support just comment this out
            Console.OutputEncoding = Encoding.GetEncoding(65001);
            var message = new Message()
            {
                ConversationId = Guid.NewGuid().ToString(),
                Text = ""
            };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule());
            builder
                .Register(c => new BotToUserTextWriter(new BotToUserQueue(message, new Queue<Message>()), Console.Out))
                .As<IBotToUser>()
                .InstancePerLifetimeScope();
            using (var container = builder.Build())
            using (var scope = DialogModule.BeginLifetimeScope(container, message))
            {
                var task = scope.Resolve<IDialogTask>();

                Func<IDialog<T>> MakeRoot = () => form;

                await task.PollAsync(() => form);

                while (true)
                {
                    message.Text = await Console.In.ReadLineAsync();
                    message.Language = Locale;
                    await task.PostAsync(message, () => form);
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
                        case DebugOptions.NoAnnotations:
                            return MakeForm(() => PizzaOrder.BuildForm(noNumbers: true, ignoreAnnotations: true));
                        case DebugOptions.NoFieldOrder:
                            return MakeForm(() => new FormBuilder<PizzaOrder>().Build());
                        case DebugOptions.WithState:
                            return new FormDialog<PizzaOrder>(new PizzaOrder()
                            { Size = SizeOptions.Large, DeliveryAddress = "123 State", Kind = PizzaOptions.BYOPizza },
                            () => PizzaOrder.BuildForm(noNumbers: false), options: FormOptions.PromptInStart);
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
                .Loop();

            Interactive(callDebug).GetAwaiter().GetResult();
        }
    }
}
