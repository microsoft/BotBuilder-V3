using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Internals.Scorables;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Match = System.Text.RegularExpressions.Match;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Runtime.CompilerServices;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public sealed class DispatchTests
    {
        public const string IntentAll = "intentAll";
        public const string IntentOne = "intentOne";
        public const string IntentTwo = "intentTwo";
        public const string IntentNone = "none";

        public const string EntityTypeA = "entityTypeA";
        public const string EntityTypeB = "entityTypeB";
        public const string EntityValueA = "EntityValueA";
        public const string EntityValueB = "EntityValueB";

        public const string ModelOne = "modelOne";
        public const string KeyOne = "keyOne";

        public const string ModelTwo = "modelTwo";
        public const string KeyTwo = "keyTwo";

        public static readonly EntityRecommendation EntityA = new EntityRecommendation(type: EntityTypeA, entity: EntityValueA);
        public static readonly EntityRecommendation EntityB = new EntityRecommendation(type: EntityTypeB, entity: EntityValueB);

        public static LuisResult Result(double? scoreAll, double? scoreOne, double? scoreTwo, double? scoreNone)
        {
            var intents = new List<IntentRecommendation>();
            if (scoreAll.HasValue) intents.Add(new IntentRecommendation() { Intent = IntentAll, Score = scoreAll.Value });
            if (scoreOne.HasValue) intents.Add(new IntentRecommendation() { Intent = IntentOne, Score = scoreOne.Value });
            if (scoreTwo.HasValue) intents.Add(new IntentRecommendation() { Intent = IntentTwo, Score = scoreTwo.Value });
            if (scoreNone.HasValue) intents.Add(new IntentRecommendation() { Intent = IntentNone, Score = scoreNone.Value });

            return new LuisResult()
            {
                Intents = intents.ToArray(),
                Entities = new[] { EntityA, EntityB }
            };
        }

        [LuisModel(ModelOne, KeyOne)]
        [LuisModel(ModelTwo, KeyTwo)]
        public interface IMethods
        {
            // test ideas
            //  luis: none intents, multiple models
            //  regex: longer matches and result scoring
            //  errors: ambiguous binding message, no match found?

            Task Activity(IMessageActivity activity);
            Task Activity(ITypingActivity activity);
            Task Activity(IActivity activity);


            [LuisIntent(IntentAll)]
            Task LuisAllTypes
                (
                ILuisModel model,
                IntentRecommendation intent,
                LuisResult result,
                [Entity(EntityTypeA)] string entityA_S,
                [Entity(EntityTypeA)] IEnumerable<string> entityA_IE_S,
                [Entity(EntityTypeA)] IReadOnlyCollection<string> entityA_IC_S,
                [Entity(EntityTypeA)] IReadOnlyList<string> entityA_IL_S,
                [Entity(EntityTypeA)] EntityRecommendation entityA_E,
                [Entity(EntityTypeA)] IEnumerable<EntityRecommendation> entityA_IE_E,
                [Entity(EntityTypeA)] IReadOnlyCollection<EntityRecommendation> entityA_IC_E,
                [Entity(EntityTypeA)] IReadOnlyList<EntityRecommendation> entityA_IL_E,
                [Entity(EntityTypeB)] string entityB_S,
                [Entity(EntityTypeB)] IEnumerable<string> entityB_IE_S,
                [Entity(EntityTypeB)] IReadOnlyCollection<string> entityB_IC_S,
                [Entity(EntityTypeB)] IReadOnlyList<string> entityB_IL_S,
                [Entity(EntityTypeB)] EntityRecommendation entityB_E,
                [Entity(EntityTypeB)] IEnumerable<EntityRecommendation> entityB_IE_E,
                [Entity(EntityTypeB)] IReadOnlyCollection<EntityRecommendation> entityB_IC_E,
                [Entity(EntityTypeB)] IReadOnlyList<EntityRecommendation> entityB_IL_E
                );

            [LuisIntent(IntentOne)]
            Task LuisOne(ILuisModel model, [Entity(EntityTypeA)] IEnumerable<string> entityA);

            [LuisIntent(IntentTwo)]
            Task LuisTwo(ILuisModel model, [Entity(EntityTypeA)] string entityA);

            [LuisIntent(IntentNone)]
            Task LuisNone(ILuisModel model);

            [RegexPattern("RegexAll (?<captureAll>.*)")]
            Task RegexAllTypes(Regex regex, Match match, CaptureCollection captures, [Entity("captureAll")] Capture capture, [Entity("captureAll")] string text);

            [RegexPattern("RegexOne (?<captureOne>.*)")]
            Task RegexOne([Entity("captureOne")] Capture capture);

            [RegexPattern("RegexTwo (?<captureTwo>.*)")]
            Task RegexTwo([Entity("captureTwo")] string capture);
        }

        private readonly CancellationToken token = new CancellationTokenSource().Token;
        private readonly Mock<IMethods> methods = new Mock<IMethods>(MockBehavior.Strict);
        private readonly Mock<ILuisService> luisOne = new Mock<ILuisService>(MockBehavior.Strict);
        private readonly Mock<ILuisService> luisTwo = new Mock<ILuisService>(MockBehavior.Strict);
        private readonly Activity activity = new Activity();
        private readonly IResolver resolver;
        private readonly IScorable<IResolver, object> scorable;

        private readonly Dictionary<string, LuisResult> luisOneByText = new Dictionary<string, LuisResult>();
        private readonly Dictionary<string, LuisResult> luisTwoByText = new Dictionary<string, LuisResult>();

        public DispatchTests()
        {
            // TODO: not working
            //methods.Setup(m => m.ToString()).Returns("methods");

            this.resolver =
                new ActivityResolver(
                    new DictionaryResolver(new Dictionary<Type, object>()
                    {
                        { typeof(IActivity), this.activity },
                        { typeof(IMethods), this.methods.Object },
                    }, new NullResolver()));

            luisOne
                .Setup(l => l.BuildUri(It.IsAny<string>()))
                .Returns<string>(q => new UriBuilder() { Path = q }.Uri);

            luisOne
                .Setup(l => l.QueryAsync(It.IsAny<Uri>(), token))
                .Returns<Uri, CancellationToken>(async (u, t) =>
                {
                    var text = u.LocalPath.Substring(1);
                    return luisOneByText[text];
                });

            luisTwo
                .Setup(l => l.BuildUri(It.IsAny<string>()))
                .Returns<string>(q => new UriBuilder() { Path = q }.Uri);

            luisTwo
                .Setup(l => l.QueryAsync(It.IsAny<Uri>(), token))
                .Returns<Uri, CancellationToken>(async (u, t) =>
                {
                    var text = u.LocalPath.Substring(1);
                    return luisTwoByText[text];
                });

            Func<ILuisModel, ILuisService> make = model =>
            {
                if (model.SubscriptionKey == KeyOne && model.ModelID == ModelOne) return luisOne.Object;
                if (model.SubscriptionKey == KeyTwo && model.ModelID == ModelTwo) return luisTwo.Object;
                throw new NotImplementedException();
            };

            this.scorable = new AttributeScorable(typeof(IMethods), make);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            this.activity.Type = null;
            this.activity.Text = null;
            this.methods.Reset();
            this.luisOne.ResetCalls();
            this.luisTwo.ResetCalls();
            this.luisOneByText.Clear();
            this.luisTwoByText.Clear();
        }

        [TestMethod]
        public async Task Dispatch_Activity_Message()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            this.luisOneByText[activity.Text] = Result(null, null, null, null);
            this.luisTwoByText[activity.Text] = Result(null, null, null, null);
            methods
                .Setup(m => m.Activity((IMessageActivity) this.activity))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Activity_Typing()
        {
            // arrange
            activity.Type = ActivityTypes.Typing;
            activity.Text = "blah";

            this.luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            this.luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.Activity((ITypingActivity)this.activity))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Activity_Generic()
        {
            // arrange
            activity.Type = ActivityTypes.DeleteUserData;
            activity.Text = "blah";

            this.luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            this.luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.Activity((IActivity)this.activity))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Regex_All_Types()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "RegexAll captureThis";

            this.luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            this.luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.RegexAllTypes
                    (
                        It.IsAny<Regex>(),
                        It.Is<Match>(i => i.Success),
                        It.Is<CaptureCollection>(c => c.Count > 0),
                        It.Is<Capture>(c => c.Value == "captureThis"),
                        It.Is<string>(s => s == "captureThis")
                    ))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Regex_One()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "RegexOne captureOneValue";

            this.luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            this.luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.RegexOne
                    (
                        It.Is<Capture>(c => c.Value == "captureOneValue")))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Regex_Two()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "RegexTwo captureTwoValue";

            this.luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            this.luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.RegexTwo
                    (
                        It.Is<string>(s => s == "captureTwoValue")))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Luis_All_Types()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            this.luisOneByText[activity.Text] = Result(0.9, 0.8, 0.7, 0.6);
            this.luisTwoByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);

            methods
                .Setup(m => m.LuisAllTypes
                (
                    It.Is<ILuisModel>(l => l.ModelID == ModelTwo && l.SubscriptionKey == KeyTwo),
                    It.Is<IntentRecommendation>(i => i.Intent == IntentAll),
                    It.IsAny<LuisResult>(),
                    EntityValueA,
                    new[] { EntityValueA },
                    new[] { EntityValueA },
                    new[] { EntityValueA },
                    It.Is<EntityRecommendation>(e => e.Entity == EntityValueA),
                    It.Is<IEnumerable<EntityRecommendation>>(e => e.Single().Entity == EntityValueA),
                    It.Is<IReadOnlyCollection<EntityRecommendation>>(e => e.Single().Entity == EntityValueA),
                    It.Is<IReadOnlyList<EntityRecommendation>>(e => e.Single().Entity == EntityValueA),
                    EntityValueB,
                    new[] { EntityValueB },
                    new[] { EntityValueB },
                    new[] { EntityValueB },
                    It.Is<EntityRecommendation>(e => e.Entity == EntityValueB),
                    It.Is<IEnumerable<EntityRecommendation>>(e => e.Single().Entity == EntityValueB),
                    It.Is<IReadOnlyCollection<EntityRecommendation>>(e => e.Single().Entity == EntityValueB),
                    It.Is<IReadOnlyList<EntityRecommendation>>(e => e.Single().Entity == EntityValueB)
                ))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
            luisOne.VerifyAll();
            luisTwo.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Luis_Intent_One_Model_One()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            this.luisOneByText[activity.Text] = Result(0.0, 0.9, 0.5, 0.5);
            this.luisTwoByText[activity.Text] = Result(0.0, 0.5, 0.5, 0.5);

            methods
                .Setup(m => m.LuisOne
                (
                    It.Is<ILuisModel>(l => l.ModelID == ModelOne && l.SubscriptionKey == KeyOne),
                    new[] { EntityValueA }
                ))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
            luisOne.VerifyAll();
            luisTwo.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Luis_Intent_One_Model_Two()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            this.luisOneByText[activity.Text] = Result(0.0, 0.5, 0.5, 0.5);
            this.luisTwoByText[activity.Text] = Result(0.0, 0.9, 0.5, 0.5);

            methods
                .Setup(m => m.LuisOne
                (
                    It.Is<ILuisModel>(l => l.ModelID == ModelTwo && l.SubscriptionKey == KeyTwo),
                    new[] { EntityValueA }
                ))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
            luisOne.VerifyAll();
            luisTwo.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Luis_Intent_Two_Model_One()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            this.luisOneByText[activity.Text] = Result(0.0, 0.5, 0.9, 0.5);
            this.luisTwoByText[activity.Text] = Result(0.0, 0.5, 0.5, 0.5);

            methods
                .Setup(m => m.LuisTwo
                (
                    It.Is<ILuisModel>(l => l.ModelID == ModelOne && l.SubscriptionKey == KeyOne),
                    EntityValueA
                ))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
            luisOne.VerifyAll();
            luisTwo.VerifyAll();
        }

        [TestMethod]
        public async Task Dispatch_Luis_Intent_Two_Model_Two()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            this.luisOneByText[activity.Text] = Result(0.0, 0.5, 0.5, 0.5);
            this.luisTwoByText[activity.Text] = Result(0.0, 0.5, 0.9, 0.5);

            methods
                .Setup(m => m.LuisTwo
                (
                    It.Is<ILuisModel>(l => l.ModelID == ModelTwo && l.SubscriptionKey == KeyTwo),
                    EntityValueA
                ))
                .Returns(Task.CompletedTask);

            // act
            Assert.IsTrue(await scorable.TryPostAsync(resolver, token));

            // assert
            methods.VerifyAll();
            luisOne.VerifyAll();
            luisTwo.VerifyAll();
        }
    }
}