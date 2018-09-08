using System.Collections.Generic;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LanguageGenerationResolverTests
    {
        private LanguageGenerationApplication _lgEndpoint;
        private LanguageGenerationOptions _lgOptions;
        private IServiceAgent _serviceAgentMock;

        [TestInitialize]
        public void TestInitialize()
        {
            var endpointKey = "cc7bbcc0-3715-44f0-b7c9-d8fee333dce1";
            var lgAppId = "ab48996d-abe2-4785-8eff-f18d15fc3560";
            var endpointUri = "westus";

            _lgEndpoint = new LanguageGenerationApplication(endpointKey, lgAppId, endpointUri);
            _lgOptions = new LanguageGenerationOptions();
            var resolutionsDictionary = new Dictionary<string, string>
            {
                { "wPhrase", "Hello" },
                { "welcomeUser", "welcome {userName}" },
                { "offerHelp", "How can I help you?" },
                { "errorReadout", "Sorry, something went wrong, could you repeate this again?" },
            };
            _serviceAgentMock = new ServiceAgentMock(resolutionsDictionary);
        }


        [TestMethod]
        public async Task TestEndToEnd_OneTemplateModifyActivityText_ValidAsync()
        {
            var activity = new Activity
            {
                Text = "[wPhrase] my friend"
            };

            var lgResolver = new LanguageGenerationResolver(_lgEndpoint, _lgOptions, _serviceAgentMock);
            var metaData = new Dictionary<string, object>();
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Hello my friend", activity.Text);
        }

        [TestMethod]
        public async Task TestEndToEnd_OneTemplateModifyActivitySpeak_ValidAsync()
        {
            var activity = new Activity
            {
                Speak = "[wPhrase] my friend"
            };

            var lgResolver = new LanguageGenerationResolver(_lgEndpoint, _lgOptions, _serviceAgentMock);
            var metaData = new Dictionary<string, object>();
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Hello my friend", activity.Speak);
        }

        [TestMethod]
        public async Task TestEndToEnd_OneTemplateModifyActivitySuggestedActions_ValidAsync()
        {
            var activity = new Activity
            {
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Text = "[wPhrase] my friend",
                            DisplayText = "[wPhrase] my friend"
                        }
                    }
                }
            };

            var lgResolver = new LanguageGenerationResolver(_lgEndpoint, _lgOptions, _serviceAgentMock);
            var metaData = new Dictionary<string, object>();
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.IsNotNull(activity.SuggestedActions);
            Assert.IsNotNull(activity.SuggestedActions.Actions);
            var cardActions = activity.SuggestedActions.Actions;
            Assert.AreEqual("Hello my friend", cardActions[0].Text);
            Assert.AreEqual("Hello my friend", cardActions[0].DisplayText);
        }

        [TestMethod]
        public async Task TestEndToEnd_OneTemplateModifyActivityAll_ValidAsync()
        {
            var activity = new Activity
            {
                Text = "[wPhrase] my friend",
                Speak = "[wPhrase] my friend",
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Text = "[wPhrase] my friend",
                            DisplayText = "[wPhrase] my friend"
                        }
                    }
                }
            };

            var lgResolver = new LanguageGenerationResolver(_lgEndpoint, _lgOptions, _serviceAgentMock);
            var metaData = new Dictionary<string, object>();
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Hello my friend", activity.Text);
            Assert.AreEqual("Hello my friend", activity.Speak);
            Assert.IsNotNull(activity.SuggestedActions);
            Assert.IsNotNull(activity.SuggestedActions.Actions);

            var cardActions = activity.SuggestedActions.Actions;
            Assert.AreEqual("Hello my friend", cardActions[0].Text);
            Assert.AreEqual("Hello my friend", cardActions[0].DisplayText);
        }

        [TestMethod]
        public async Task TestEndToEnd_MultipleTemplateModifyActivityText_ValidAsync()
        {
            var activity = new Activity
            {
                Text = "[welcomeUser] , [offerHelp]",
            };

            var lgResolver = new LanguageGenerationResolver(_lgEndpoint, _lgOptions, _serviceAgentMock);
            var metaData = new Dictionary<string, object>()
            {
                { "userName", "Amr" },
            };
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("welcome Amr , How can I help you?", activity.Text);
        }

        [TestMethod]
        public async Task TestEndToEnd_MultipleTemplateModifyActivitySpeakAndText_ValidAsync()
        {
            var activity = new Activity
            {
                Text = "[wPhrase]",
                Speak = "[welcomeUser]",
            };

            var lgResolver = new LanguageGenerationResolver(_lgEndpoint, _lgOptions, _serviceAgentMock);
            var metaData = new Dictionary<string, object>()
            {
                { "userName", "Amr" },
            };
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Hello", activity.Text);
            Assert.AreEqual("welcome Amr", activity.Speak);
        }

        [TestMethod]
        public async Task TestEndToEnd_MultipleTemplateModifyActivityAll_ValidAsync()
        {
            var activity = new Activity
            {
                Text = "[wPhrase]",
                Speak = "[welcomeUser]",
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Text = "[offerHelp]",
                            DisplayText = "[offerHelp] my friend"
                        }
                    }
                }
            };

            var lgResolver = new LanguageGenerationResolver(_lgEndpoint, _lgOptions, _serviceAgentMock);
            var metaData = new Dictionary<string, object>()
            {
                { "userName", "Amr" },
            };
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Hello", activity.Text);
            Assert.AreEqual("welcome Amr", activity.Speak);
            Assert.IsNotNull(activity.SuggestedActions);
            Assert.IsNotNull(activity.SuggestedActions.Actions);

            var cardActions = activity.SuggestedActions.Actions;
            Assert.AreEqual("How can I help you?", cardActions[0].Text);
            Assert.AreEqual("How can I help you? my friend", cardActions[0].DisplayText);
        }
    }
}
