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
    public sealed class DerivedLuisDialog : LuisDialog
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

    [TestClass]
    public sealed class LuisTests
    {
        [TestMethod]
        public void All_Handlers_Are_Found()
        {
            var service = new Mock<ILuisService>();

            var dialog = new DerivedLuisDialog(service.Object);

            var handlers = LuisDialog.EnumerateHandlers(dialog).ToArray();

            Assert.AreEqual(7, handlers.Length);
        }
    }
}
