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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Json;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis.Models;

using Moq;
using Autofac;
using Newtonsoft.Json.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
#pragma warning disable CS1998

    [TestClass]
    public sealed class FormTests : DialogTestBase
    {
        public interface IFormTarget
        {
            string Text { get; set; }
            int Integer { get; set; }
            float Float { get; set; }
        }

        private static class Input
        {
            public const string Text = "some text here";
            public const int Integer = 99;
            public const float Float = 1.5f;
        }

        [Serializable]
        private sealed class FormTarget : IFormTarget
        {
            float IFormTarget.Float { get; set; }
            int IFormTarget.Integer { get; set; }
            string IFormTarget.Text { get; set; }
        }

        private static async Task RunScriptAgainstForm(IEnumerable<EntityRecommendation> entities, params string[] script)
        {
            IFormTarget target = new FormTarget();
            using (var container = Build(Options.ResolveDialogFromContainer, target))
            {
                {
                    var root = new FormDialog<IFormTarget>(target, entities: entities);
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance(root)
                        .AsSelf()
                        .As<IDialog<object>>();
                    builder.Update(container);
                }

                await AssertScriptAsync(container, script);

                {
                    Assert.AreEqual(Input.Text, target.Text);
                    Assert.AreEqual(Input.Integer, target.Integer);
                    Assert.AreEqual(Input.Float, target.Float);
                }
            }
        }

        [TestMethod]
        public async Task Form_Can_Fill_In_Scalar_Types()
        {
            IEnumerable<EntityRecommendation> entities = Enumerable.Empty<EntityRecommendation>();
            await RunScriptAgainstForm(entities,
                    "hello",
                    "Please enter text ",
                    Input.Text,
                    "Please enter a number for integer (current choice: 0)",
                    Input.Integer.ToString(),
                    "Please enter a number for float (current choice: 0)",
                    Input.Float.ToString()
                );
        }

        [TestMethod]
        public async Task Form_Can_Handle_Luis_Entity()
        {
            IEnumerable<EntityRecommendation> entities = new[] { new EntityRecommendation(type: nameof(IFormTarget.Text), entity: Input.Text) };
            await RunScriptAgainstForm(entities,
                    "hello",
                    "Please enter a number for integer (current choice: 0)",
                    Input.Integer.ToString(),
                    "Please enter a number for float (current choice: 0)",
                    Input.Float.ToString()
                );
        }

        [TestMethod]
        public async Task Form_Can_Handle_Irrelevant_Luis_Entity()
        {
            IEnumerable<EntityRecommendation> entities = new[] { new EntityRecommendation(type: "some random entity", entity: Input.Text) };
            await RunScriptAgainstForm(entities,
                    "hello",
                    "Please enter text ",
                    Input.Text,
                    "Please enter a number for integer (current choice: 0)",
                    Input.Integer.ToString(),
                    "Please enter a number for float (current choice: 0)",
                    Input.Float.ToString()
                );
        }

        [TestMethod]
        public async Task CanResolveDynamicFormFromContainer()
        {
            // This test has two purposes.
            // 1. show that IFormDialog can be resolved from the container
            // 2. show that json schema forms can be dynamically generated based on the incoming message
            // You will likely find that the extensibility in IForm's callback methods may be sufficient enough for most scenarios.

            using (var container = Build(Options.ResolveDialogFromContainer))
            {
                var builder = new ContainerBuilder();

                // make a dynamic IForm model based on the incoming message
                builder
                    .Register(c =>
                    {
                        var message = c.Resolve<IMessageActivity>();

                        // use the user's name as the prompt
                        const string TEMPLATE_PREFIX =
                        @"
                        {
                          'type': 'object',
                          'properties': {
                            'name': {
                              'type': 'string',
                              'Prompt': { 'Patterns': [ '";

                        const string TEMPLATE_SUFFIX =
                        @"' ] },
                            }
                          }
                        }
                        ";

                        var text = TEMPLATE_PREFIX + message.From.Id + TEMPLATE_SUFFIX;
                        var schema = JObject.Parse(text);

                        return
                            new FormBuilderJson(schema)
                            .AddRemainingFields()
                            .Build();
                    })
                    .As<IForm<JObject>>()
                    // lifetime must match lifetime scope tag of Message, since we're dependent on the Message
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

                builder
                    .Register<BuildFormDelegate<JObject>>(c =>
                    {
                        var cc = c.Resolve<IComponentContext>();
                        return () => cc.Resolve<IForm<JObject>>();
                    })
                    // tell the serialization framework to recover this delegate from the container
                    // rather than trying to serialize it with the dialog
                    // normally, this delegate is a static method that is trivially serializable without any risk of a closure capturing the environment
                    .Keyed<BuildFormDelegate<JObject>>(FiberModule.Key_DoNotSerialize)
                    .AsSelf()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

                builder
                    .RegisterType<FormDialog<JObject>>()
                    // root dialog is an IDialog<object>
                    .As<IDialog<object>>()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

                builder
                    // our default form state
                    .Register<JObject>(c => new JObject())
                    .AsSelf()
                    .InstancePerDependency();

                builder.Update(container);

                // verify that the form dialog prompt is dynamically generated from the incoming message
                await AssertScriptAsync(container,
                    "hello",
                    ChannelID.User
                    );
            }
        }
    }
}
