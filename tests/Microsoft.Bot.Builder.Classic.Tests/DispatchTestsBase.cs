using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Luis;
using Microsoft.Bot.Builder.Classic.Luis.Models;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Match = System.Text.RegularExpressions.Match;
using System.Threading.Tasks;
using System.Threading;
using Moq;
using Microsoft.Bot.Builder.Classic.Scorables;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [Ignore]
    public abstract class DispatchTestsBase
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

        public static readonly EntityModel EntityA = new EntityModel(type: EntityTypeA, entity: EntityValueA);
        public static readonly EntityModel EntityB = new EntityModel(type: EntityTypeB, entity: EntityValueB);

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

            [MethodBind]
            [ScorableGroup(2)]
            Task Activity(IMessageActivity activity);
            [MethodBind]
            [ScorableGroup(2)]
            Task Activity(ITypingActivity activity);
            [MethodBind]
            [ScorableGroup(2)]
            Task Activity(IActivity activity);


            [LuisIntent(IntentAll)]
            [ScorableGroup(1)]
            Task LuisAllTypes(
                ILuisModel model,
                IntentRecommendation intent,
                LuisResult result,
                [Entity(EntityTypeA)] string entityA_as_String,
                [Entity(EntityTypeA)] IEnumerable<string> entityA_as_IEnumerable_String,
                [Entity(EntityTypeA)] IReadOnlyCollection<string> entityA_as_IReadOnlyCollection_String,
                [Entity(EntityTypeA)] IReadOnlyList<string> entityA_as_IReadOnlyList_String,
                [Entity(EntityTypeA)] EntityModel entityA_as_EntityModel,
                [Entity(EntityTypeA)] IEnumerable<EntityModel> entityA_as_IEnumerable_EntityModel,
                [Entity(EntityTypeA)] IReadOnlyCollection<EntityModel> entityA_as_IReadOnlyCollection_EntityModel,
                [Entity(EntityTypeA)] IReadOnlyList<EntityModel> entityA_as_IReadOnlyList_EntityModel,
                [Entity(EntityTypeB)] string entityB_as_String,
                [Entity(EntityTypeB)] IEnumerable<string> entityB_as_IEnumerable_String,
                [Entity(EntityTypeB)] IReadOnlyCollection<string> entityB_as_IReadOnlyCollection_String,
                [Entity(EntityTypeB)] IReadOnlyList<string> entityB_as_IReadOnlyList_String,
                [Entity(EntityTypeB)] EntityModel entityB_as_EntityModel,
                [Entity(EntityTypeB)] IEnumerable<EntityModel> entityB_as_IEnumerable_EntityModel,
                [Entity(EntityTypeB)] IReadOnlyCollection<EntityModel> entityB_as_IReadOnlyCollection_EntityModel,
                [Entity(EntityTypeB)] IReadOnlyList<EntityModel> entityB_as_IReadOnlyList_EntityModel
                );

            [LuisIntent(IntentOne)]
            [ScorableGroup(1)]
            Task LuisOne(
                ILuisModel model,
                [Entity(EntityTypeA)] IEnumerable<string> entityA);

            [LuisIntent(IntentTwo)]
            [ScorableGroup(1)]
            Task LuisTwo(
                ILuisModel model,
                [Entity(EntityTypeA)] string entityA);

            [LuisIntent(IntentNone)]
            [ScorableGroup(1)]
            Task LuisNone(ILuisModel model);

            [RegexPattern("RegexAll (?<captureAll>.*)")]
            [ScorableGroup(0)]
            Task RegexAllTypes(
                Regex regex,
                Match match,
                CaptureCollection captures,
                [Entity("captureAll")] System.Text.RegularExpressions.Capture capture,
                [Entity("captureAll")] string text);

            [RegexPattern("RegexOne (?<captureOne>.*)")]
            [ScorableGroup(0)]
            Task RegexOne(
                [Entity("captureOne")] System.Text.RegularExpressions.Capture capture);

            [RegexPattern("RegexTwo (?<captureTwo>.*)")]
            [ScorableGroup(0)]
            Task RegexTwo(
                [Entity("captureTwo")] string capture);
        }

        protected readonly CancellationToken token = new CancellationTokenSource().Token;
        protected readonly Mock<IMethods> methods = new Mock<IMethods>(MockBehavior.Strict);
        protected readonly Mock<ILuisService> luisOne = new Mock<ILuisService>(MockBehavior.Strict);
        protected readonly Mock<ILuisService> luisTwo = new Mock<ILuisService>(MockBehavior.Strict);
        protected readonly Activity activity = (Activity)DialogTestBase.MakeTestMessage();

        protected readonly Dictionary<string, LuisResult> luisOneByText = new Dictionary<string, LuisResult>();
        protected readonly Dictionary<string, LuisResult> luisTwoByText = new Dictionary<string, LuisResult>();

        public DispatchTestsBase()
        {
            luisOne
                .Setup(l => l.BuildUri(It.IsAny<LuisRequest>()))
                .Returns<LuisRequest>(q => new UriBuilder() { Host = "one", Path = q.Query }.Uri);

            luisOne
                .Setup(l => l.ModifyRequest(It.IsAny<LuisRequest>()))
                .Returns<LuisRequest>(r => r);

            luisOne
                .Setup(l => l.QueryAsync(It.IsAny<Uri>(), token))
                .Returns<Uri, CancellationToken>(async (u, t) =>
                {
                    var text = u.LocalPath.Substring(1);
                    return luisOneByText[text];
                });

            luisTwo
                .Setup(l => l.BuildUri(It.IsAny<LuisRequest>()))
                .Returns<LuisRequest>(q => new UriBuilder() { Host = "two", Path = q.Query }.Uri);

            luisTwo
                .Setup(l => l.ModifyRequest(It.IsAny<LuisRequest>()))
                .Returns<LuisRequest>(r => r);

            luisTwo
                .Setup(l => l.QueryAsync(It.IsAny<Uri>(), token))
                .Returns<Uri, CancellationToken>(async (u, t) =>
                {
                    var text = u.LocalPath.Substring(1);
                    return luisTwoByText[text];
                });
        }

        public ILuisService MakeLuisService(ILuisModel model)
        {
            if (model.SubscriptionKey == KeyOne && model.ModelID == ModelOne) return luisOne.Object;
            if (model.SubscriptionKey == KeyTwo && model.ModelID == ModelTwo) return luisTwo.Object;
            throw new NotImplementedException();
        }

        [TestInitialize]
        public virtual void TestInitialize()
        {
            this.activity.Type = null;
            this.activity.Text = null;
            this.methods.Reset();
            this.luisOne.ResetCalls();
            this.luisTwo.ResetCalls();
            this.luisOneByText.Clear();
            this.luisTwoByText.Clear();
        }

        public abstract Task ActAsync();

        public virtual void VerifyMocks()
        {
            methods.VerifyAll();

            foreach (var luis in new[] { luisOne, luisTwo })
            {
                luis
                    .Verify(l => l.QueryAsync(It.IsAny<Uri>(), token), Times.AtMostOnce);
            }
        }

        [TestMethod]
        public async Task Dispatch_Activity_Message()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            luisOneByText[activity.Text] = Result(null, null, null, null);
            luisTwoByText[activity.Text] = Result(null, null, null, null);
            methods
                .Setup(m => m.Activity((IMessageActivity) this.activity))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Activity_Typing()
        {
            // arrange
            activity.Type = ActivityTypes.Typing;
            activity.Text = "blah";

            luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.Activity((ITypingActivity)this.activity))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Activity_Generic()
        {
            // arrange
            activity.Type = ActivityTypes.DeleteUserData;
            activity.Text = "blah";

            luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.Activity((IActivity)this.activity))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Regex_All_Types()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "RegexAll captureThis";

            luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.RegexAllTypes
                    (
                        It.IsAny<Regex>(),
                        It.Is<Match>(i => i.Success),
                        It.Is<CaptureCollection>(c => c.Count > 0),
                        It.Is<System.Text.RegularExpressions.Capture>(c => c.Value == "captureThis"),
                        It.Is<string>(s => s == "captureThis")
                    ))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Regex_One()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "RegexOne captureOneValue";

            luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.RegexOne
                    (
                        It.Is<System.Text.RegularExpressions.Capture>(c => c.Value == "captureOneValue")))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Regex_Two()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "RegexTwo captureTwoValue";

            luisOneByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);
            luisTwoByText[activity.Text] = Result(0.7, 0.8, 0.9, 1.0);
            methods
                .Setup(m => m.RegexTwo
                    (
                        It.Is<string>(s => s == "captureTwoValue")))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Luis_All_Types()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            luisOneByText[activity.Text] = Result(0.9, 0.8, 0.7, 0.6);
            luisTwoByText[activity.Text] = Result(1.0, 0.9, 0.8, 0.7);

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
                    It.Is<EntityModel>(e => e.Entity == EntityValueA),
                    It.Is<IEnumerable<EntityModel>>(e => e.Single().Entity == EntityValueA),
                    It.Is<IReadOnlyCollection<EntityModel>>(e => e.Single().Entity == EntityValueA),
                    It.Is<IReadOnlyList<EntityModel>>(e => e.Single().Entity == EntityValueA),
                    EntityValueB,
                    new[] { EntityValueB },
                    new[] { EntityValueB },
                    new[] { EntityValueB },
                    It.Is<EntityModel>(e => e.Entity == EntityValueB),
                    It.Is<IEnumerable<EntityModel>>(e => e.Single().Entity == EntityValueB),
                    It.Is<IReadOnlyCollection<EntityModel>>(e => e.Single().Entity == EntityValueB),
                    It.Is<IReadOnlyList<EntityModel>>(e => e.Single().Entity == EntityValueB)
                ))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Luis_Intent_One_Model_One()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            luisOneByText[activity.Text] = Result(0.0, 0.9, 0.5, 0.5);
            luisTwoByText[activity.Text] = Result(0.0, 0.5, 0.5, 0.5);

            methods
                .Setup(m => m.LuisOne
                (
                    It.Is<ILuisModel>(l => l.ModelID == ModelOne && l.SubscriptionKey == KeyOne),
                    new[] { EntityValueA }
                ))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Luis_Intent_One_Model_Two()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            luisOneByText[activity.Text] = Result(0.0, 0.5, 0.5, 0.5);
            luisTwoByText[activity.Text] = Result(0.0, 0.9, 0.5, 0.5);

            methods
                .Setup(m => m.LuisOne
                (
                    It.Is<ILuisModel>(l => l.ModelID == ModelTwo && l.SubscriptionKey == KeyTwo),
                    new[] { EntityValueA }
                ))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Luis_Intent_Two_Model_One()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            luisOneByText[activity.Text] = Result(0.0, 0.5, 0.9, 0.5);
            luisTwoByText[activity.Text] = Result(0.0, 0.5, 0.5, 0.5);

            methods
                .Setup(m => m.LuisTwo
                (
                    It.Is<ILuisModel>(l => l.ModelID == ModelOne && l.SubscriptionKey == KeyOne),
                    EntityValueA
                ))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }

        [TestMethod]
        public async Task Dispatch_Luis_Intent_Two_Model_Two()
        {
            // arrange
            activity.Type = ActivityTypes.Message;
            activity.Text = "blah";

            luisOneByText[activity.Text] = Result(0.0, 0.5, 0.5, 0.5);
            luisTwoByText[activity.Text] = Result(0.0, 0.5, 0.9, 0.5);

            methods
                .Setup(m => m.LuisTwo
                (
                    It.Is<ILuisModel>(l => l.ModelID == ModelTwo && l.SubscriptionKey == KeyTwo),
                    EntityValueA
                ))
                .Returns(Task.CompletedTask);

            // act
            await ActAsync();

            // assert
            VerifyMocks();
        }
    }
}
