using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Luis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class LuisTryFindEntityTests
    {
        [TestMethod]
        public void Luis_TryFindEntity_Foo_Where_Built_In_Number_Overlaps()
        {
            // assemble

            var recommendations = new List<EntityRecommendation>
            {
                new EntityRecommendation { Type = "foo", Entity = "3", StartIndex=12, EndIndex=12},
                new EntityRecommendation { Type = "builtin.number", Entity = "3", StartIndex=12, EndIndex=12}
            };

            var result = new LuisResult("total miles 3", recommendations);

            // act

            EntityRecommendation recommendation;
            result.TryFindEntity("builtin.number", out recommendation);
            EntityRecommendation recommendationFoo;
            result.TryFindEntity("foo", out recommendationFoo);

            // assert
            Assert.IsNotNull(recommendation, "builtin.number entity recommendation not found");
            Assert.AreEqual("3", recommendation.Entity, "wrong builtin.number entity recommendation selected");

            Assert.IsNotNull(recommendationFoo, "foo entity recommendation not found");
            Assert.AreEqual("3", recommendationFoo.Entity, "wrong foo entity recommendation selected");
        }

        [TestMethod]
        public void Luis_TryFindEntity_Date_And_Multiple_Numbers_Where_One_Number_Overlaps()
        {
            // assemble

            var recommendations = new List<EntityRecommendation>
            {
                new EntityRecommendation { Type = "builtin.datetimeV2.time", Entity = "10 pm", StartIndex=25, EndIndex=30},
                new EntityRecommendation { Type = "builtin.number", Entity = "10", StartIndex=25, EndIndex=27},
                new EntityRecommendation { Type = "builtin.number", Entity = "8", StartIndex=35, EndIndex=36}
            };

            var result = new LuisResult("make me a reservation at 10 pm for 8 people", recommendations);

            // act

            EntityRecommendation recommendation;
            result.TryFindEntity("builtin.number", out recommendation);

            // assert

            Assert.IsNotNull(recommendation,"entity recommendation not found");
            Assert.AreEqual("8", recommendation.Entity, "wrong entity recommendation selected"); // it should not select 10 since that overlaps with the datetime entity
        }

        [TestMethod]
        public void Luis_TryFindEntity_Find_Number_No_Overlaps()
        {
            // assemble

            var recommendations = new List<EntityRecommendation>
            {
                new EntityRecommendation { Type = "builtin.number", Entity = "8", StartIndex=26, EndIndex=27},
            };

            var result = new LuisResult("make me a reservation for 8 people", recommendations);

            // act

            EntityRecommendation numberRecommendation;
            result.TryFindEntity("builtin.number", out numberRecommendation);


            // assert

            Assert.IsNotNull(numberRecommendation, "number recommendation not found");
            Assert.AreEqual("8", numberRecommendation.Entity, "wrong entity recommendation selected");
        }
    }
}
