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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Autofac;
using Moq;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Builder.Tests
{
    public abstract class LuisTestBase : DialogTestBase
    {
        public static IntentRecommendation[] IntentsFor<D>(Expression<Func<D, Task>> expression, double? score)
        {
            var body = (MethodCallExpression)expression.Body;
            var attribute = body.Method.GetCustomAttribute<LuisIntentAttribute>();
            var name = attribute.IntentName;
            var intent = new IntentRecommendation(name, score);
            return new[] { intent };
        }

        public static EntityRecommendation EntityFor(string type, string entity)
        {
            return new EntityRecommendation(type: type) { Entity = entity };
        }

        public static void SetupLuis<D>(
            Mock<ILuisService> luis,
            Expression<Func<D, Task>> expression,
            double? score,
            params EntityRecommendation[] entities
            )
        {
            luis
                .Setup(l => l.QueryAsync(It.IsAny<Uri>()))
                .ReturnsAsync(new LuisResult()
                {
                    Intents = IntentsFor(expression, score),
                    Entities = entities
                });
        }
    }

    [TestClass]
    public sealed class LuisTests : LuisTestBase
    {
        public sealed class DerivedLuisDialog : LuisDialog<object>
        {
            public DerivedLuisDialog(params ILuisService[] services)
                : base(services)
            {
            }

            [LuisIntent("PublicHandlerWithAttribute")]
            public Task PublicHandlerWithAttribute(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            [LuisIntent("PrivateHandlerWithAttribute")]
            public Task PrivateHandlerWithAttribute(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            [LuisIntent("PublicHandlerWithAttributeOne")]
            [LuisIntent("PublicHandlerWithAttributeTwo")]
            public Task PublicHandlerWithTwoAttributes(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            private Task PublicHandlerWithNoAttribute(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            private Task PrivateHandlerWithNoAttribute(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            public Task PublicHandlerWithCovariance(IDialogContext context, object luisResult)
            {
                throw new NotImplementedException();
            }

            public void DoesNotMatchReturnType(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }

            public void DoesNotMatchArgumentType(IDialogContext context, int notLuisResult)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void All_Handlers_Are_Found()
        {
            var service = new Mock<ILuisService>();
            var dialog = new DerivedLuisDialog(service.Object);
            var handlers = LuisDialog.EnumerateHandlers(dialog).ToArray();
            Assert.AreEqual(7, handlers.Length);
        }

        [Serializable]
        public sealed class MultiServiceLuisDialog : LuisDialog<object>
        {
            public MultiServiceLuisDialog(params ILuisService[] services)
                : base(services)
            {
            }

            [LuisIntent("ServiceOne")]
            public async Task ServiceOne(IDialogContext context, LuisResult luisResult)
            {
                await context.PostAsync(luisResult.Entities.Single().Type);
                context.Wait(MessageReceived);
            }

            [LuisIntent("ServiceTwo")]
            public async Task ServiceTwo(IDialogContext context, LuisResult luisResult)
            {
                await context.PostAsync(luisResult.Entities.Single().Type);
                context.Wait(MessageReceived);
            }
        }

        [TestMethod]
        public async Task All_Services_Are_Called()
        {
            var service1 = new Mock<ILuisService>();
            var service2 = new Mock<ILuisService>();

            var dialog = new MultiServiceLuisDialog(service1.Object, service2.Object);

            using (new FiberTestBase.ResolveMoqAssembly(service1.Object, service2.Object))
            using (var container = Build(Options.ResolveDialogFromContainer, service1.Object, service2.Object))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(dialog)
                    .As<IDialog<object>>();
                builder.Update(container);

                const string EntityOne = "one";
                const string EntityTwo = "two";

                SetupLuis<MultiServiceLuisDialog>(service1, d => d.ServiceOne(null, null), 1.0, new EntityRecommendation(type: EntityOne));
                SetupLuis<MultiServiceLuisDialog>(service2, d => d.ServiceTwo(null, null), 0.0, new EntityRecommendation(type: EntityTwo));

                await AssertScriptAsync(container, "hello", EntityOne);

                SetupLuis<MultiServiceLuisDialog>(service1, d => d.ServiceOne(null, null), 0.0, new EntityRecommendation(type: EntityOne));
                SetupLuis<MultiServiceLuisDialog>(service2, d => d.ServiceTwo(null, null), 1.0, new EntityRecommendation(type: EntityTwo));

                await AssertScriptAsync(container, "hello", EntityTwo);
            }
        }

        public sealed class InvalidLuisDialog : LuisDialog<object>
        {
            public InvalidLuisDialog(ILuisService service)
                : base(service)
            {
            }

            [LuisIntent("HasAttributeButDoesNotMatchReturnType")]
            public void HasAttributeButDoesNotMatchReturnType(IDialogContext context, LuisResult luisResult)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidIntentHandlerException))]
        public void Invalid_Handle_Throws_Error()
        {
            var service = new Mock<ILuisService>();
            var dialog = new InvalidLuisDialog(service.Object);
            var handlers = LuisDialog.EnumerateHandlers(dialog).ToArray();
        }

        [TestMethod]
        public void UrlEncoding_UTF8_Then_Hex()
        {
            ILuisService service = new LuisService(new LuisModelAttribute("modelID", "subscriptionID"));

            var uri = service.BuildUri("Français");

            // https://github.com/Microsoft/BotBuilder/issues/247
            // https://github.com/Microsoft/BotBuilder/pull/76
            Assert.AreNotEqual("https://api.projectoxford.ai/luis/v1/application?id=modelID&subscription-key=subscriptionID&q=Fran%25u00e7ais", uri.AbsoluteUri);
            Assert.AreEqual("https://api.projectoxford.ai/luis/v1/application?id=modelID&subscription-key=subscriptionID&q=Fran%C3%A7ais", uri.AbsoluteUri);
        }
    }
}
