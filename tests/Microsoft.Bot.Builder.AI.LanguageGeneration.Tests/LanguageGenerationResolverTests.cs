using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Tests.Mocks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LanguageGenerationResolverTests
    {
        private LanguageGenerationApplication _languageGenerationApplication;
        private LanguageGenerationOptions _languageGenerationOptions;
        private IServiceAgent _serviceAgent;
        private bool _mock;
        private string _assemblyName;
        private string _testSettingsFileName;

        [TestInitialize]
        public void TestInitialize()
        {
            _assemblyName = "Microsoft.Bot.Builder.AI.LanguageGeneration.Tests";
            _testSettingsFileName = "testSettings.json";
            var fileContents = LoadResourceFile(_testSettingsFileName);
            dynamic testSettings = JObject.Parse(fileContents);
            bool.TryParse(testSettings.mock.Value, out _mock);

            if (_mock)
            {

                var jsonLanguageGenerationApplication = testSettings.mockApiTestSettings.languageGenerationApplication;

                _languageGenerationApplication = new LanguageGenerationApplication(
                    applicationId: jsonLanguageGenerationApplication.id.Value,
                    applicationRegion: jsonLanguageGenerationApplication.region.Value,
                    applicationLocale: jsonLanguageGenerationApplication.locale.Value,
                    applicationVersion: jsonLanguageGenerationApplication.version.Value,
                    subscriptionKey: jsonLanguageGenerationApplication.subscriptionKey.Value
                );

                _languageGenerationOptions = new LanguageGenerationOptions()
                {
                    ResolverApiEndpoint = testSettings.mockApiTestSettings.resolverEndpoint.Value,
                    TokenGenerationApiEndpoint = testSettings.mockApiTestSettings.tokenGenerationEndPoint.Value,
                };

                var resolutionsDictionary = new Dictionary<string, string>
                {
                    { "wPhrase", "Hello" },
                    { "welcomeUser", "Welcome {userName}" },
                    { "offerHelp", "How can I help you?" },
                    { "errorReadout", "Sorry, something went wrong, could you repeate this again?" },
                };
                _serviceAgent = new ServiceAgentMock(resolutionsDictionary);
            }
            else
            {
                var jsonLanguageGenerationApplication = testSettings.externalApiTestSettings.languageGenerationApplication;

                _languageGenerationApplication = new LanguageGenerationApplication(
                    applicationId: jsonLanguageGenerationApplication.id.Value,
                    applicationRegion: jsonLanguageGenerationApplication.region.Value,
                    applicationLocale: jsonLanguageGenerationApplication.locale.Value,
                    applicationVersion: jsonLanguageGenerationApplication.version.Value,
                    subscriptionKey: jsonLanguageGenerationApplication.subscriptionKey.Value
                );

                _languageGenerationOptions = new LanguageGenerationOptions()
                {
                    ResolverApiEndpoint = testSettings.externalApiTestSettings.resolverEndpoint.Value,
                    TokenGenerationApiEndpoint = testSettings.externalApiTestSettings.tokenGenerationEndPoint.Value,
                };
            }
        }

        [TestMethod]
        public async Task TestEndToEnd_OneTemplateModifyActivityText_ValidAsync()
        {
            var activity = new Activity
            {
                Text = "[wPhrase] my friend"
            };

            var lgResolver = new LanguageGenerationResolver(_languageGenerationApplication, _languageGenerationOptions, _mock ? _serviceAgent : null);
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

            var lgResolver = new LanguageGenerationResolver(_languageGenerationApplication, _languageGenerationOptions, _mock ? _serviceAgent : null);
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

            var lgResolver = new LanguageGenerationResolver(_languageGenerationApplication, _languageGenerationOptions, _mock ? _serviceAgent : null);
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

            var lgResolver = new LanguageGenerationResolver(_languageGenerationApplication, _languageGenerationOptions, _mock ? _serviceAgent : null);
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

            var lgResolver = new LanguageGenerationResolver(_languageGenerationApplication, _languageGenerationOptions, _mock ? _serviceAgent : null);
            var metaData = new Dictionary<string, object>()
            {
                { "userName", "Amr" },
            };
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Welcome Amr , How can I help you?", activity.Text);
        }

        [TestMethod]
        public async Task TestEndToEnd_MultipleTemplateModifyActivitySpeakAndText_ValidAsync()
        {
            var activity = new Activity
            {
                Text = "[wPhrase]",
                Speak = "[welcomeUser]",
            };

            var lgResolver = new LanguageGenerationResolver(_languageGenerationApplication, _languageGenerationOptions, _mock ? _serviceAgent : null);
            var metaData = new Dictionary<string, object>()
            {
                { "userName", "Amr" },
            };
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Hello", activity.Text);
            Assert.AreEqual("Welcome Amr", activity.Speak);
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

            var lgResolver = new LanguageGenerationResolver(_languageGenerationApplication, _languageGenerationOptions, _mock ? _serviceAgent : null);
            var metaData = new Dictionary<string, object>()
            {
                { "userName", "Amr" },
            };
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Hello", activity.Text);
            Assert.AreEqual("Welcome Amr", activity.Speak);
            Assert.IsNotNull(activity.SuggestedActions);
            Assert.IsNotNull(activity.SuggestedActions.Actions);

            var cardActions = activity.SuggestedActions.Actions;
            Assert.AreEqual("How can I help you?", cardActions[0].Text);
            Assert.AreEqual("How can I help you? my friend", cardActions[0].DisplayText);
        }

        private string LoadResourceFile(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullResourceName = _assemblyName + "." + resourceName;

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                return result;
            }
        }
    }
}
