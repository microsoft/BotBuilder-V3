using System.Linq;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class ActivityExTests
    {
        [TestMethod]
        public void HasContent_Test()
        {
            IMessageActivity activity = DialogTestBase.MakeTestMessage();
            Assert.IsFalse(activity.HasContent());
            activity.Text = "test";
            Assert.IsTrue(activity.HasContent());

        }

        public void GetMentions_Test()
        {
            IMessageActivity activity = DialogTestBase.MakeTestMessage();
            Assert.IsFalse(activity.GetMentions().Any());
            activity.Entities.Add(new Mention() { Text = "testMention" });
            Assert.IsTrue(activity.GetMentions().Any());
            Assert.AreEqual("testMention", activity.GetMentions()[0].Text);
        }
    }
}
