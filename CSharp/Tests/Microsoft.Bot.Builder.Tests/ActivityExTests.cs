using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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

        [TestMethod]
        public void GetMentions_Test()
        {
            IMessageActivity activity = DialogTestBase.MakeTestMessage();
            Assert.IsFalse(activity.GetMentions().Any());
            activity.Entities = new List<Entity> { new Mention() { Text = "testMention" } };
            // Cloning activity to resemble the incoming activity to bot
            var clonedActivity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
            Assert.IsTrue(clonedActivity.GetMentions().Any());
            Assert.AreEqual("testMention", clonedActivity.GetMentions()[0].Text);
        }
    }
}
