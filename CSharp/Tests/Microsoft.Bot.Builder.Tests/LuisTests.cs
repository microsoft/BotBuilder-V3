using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Bot.Builder.Luis;

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

        private Task PublicHandlerWithNoAttribute(IDialogContext context, LuisResult luisResult)
        {
            throw new NotImplementedException();
        }

        private Task PrivateHandlerWithNoAttribute(IDialogContext context, LuisResult luisResult)
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public sealed class LuisTests
    {
        [TestMethod]
        public void AllMethodsAreFound()
        {
            var service = new Mock<ILuisService>();

            var dialog = new DerivedLuisDialog(service.Object);

            var handlers = LuisDialog.AttributeBasedHandlers(dialog).ToArray();

            Assert.AreEqual(2, handlers.Length);
        }
    }
}
