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
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public sealed class LuisTests
    {
        public sealed class DerivedLuisDialog : LuisDialog<object>
        {
            public DerivedLuisDialog(ILuisService service)
                : base(service)
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
    }
}
