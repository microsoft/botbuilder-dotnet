using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Engine;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Tests.TestData.Mocks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }


        [TestMethod]
        [TestCategory("SlotBuilder")]
        public void TestSlotBuilder_BuildSlots_Valid()
        {
            var slotsDictionary = new Dictionary<string, object>()
            {
                {"GetStateName", "wPhrase" },
                {"customerName", "Amr" },
                {"currency", "Dollar" },
                {"purchaseValue", 15 },
                {"discountValue", 3.5f },
                {"isFirstTime", true},
                {"timeOfBurchase", DateTime.ParseExact("2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture)},
            };

            var slotBuilder = new SlotBuilder();
            var slots = slotBuilder.BuildSlots(new Activity(), slotsDictionary);

            Assert.IsNotNull(slots);
            Assert.AreEqual(slotsDictionary.Count, slots.Count);
            var visitedSlots = new Dictionary<string, bool>();
            foreach (var key in slotsDictionary.Keys)
            {
                visitedSlots.Add(key, false);
            }

            foreach (var slot in slots)
            {
                Assert.IsNotNull(slot);
                Assert.IsNotNull(slot.KeyValue);
                Assert.IsNotNull(slot.KeyValue.Key);
                Assert.IsNotNull(slot.KeyValue.Value);
                Assert.AreEqual(slotsDictionary[slot.KeyValue.Key], slot.KeyValue.Value);
                visitedSlots[slot.KeyValue.Key] = true;
            }

            foreach (var visitedSlotsValue in visitedSlots.Values)
            {
                Assert.IsTrue(visitedSlotsValue);
            }
        }

        [TestMethod]
        [TestCategory("SlotBuilder")]
        public void TestSlotBuilder_BuildSlots_InValid()
        {
            try
            {
                var slotsDictionary = new Dictionary<string, object>()
            {
                {"GetStateName", "wPhrase" },
                {"customerName", "Amr" },
                {null, null },
            };

                var slotBuilder = new SlotBuilder();
                var slots = slotBuilder.BuildSlots(new Activity(), slotsDictionary);

                Assert.IsNotNull(slots);
                Assert.AreEqual(slotsDictionary.Count, slots.Count);
                var visitedSlots = new Dictionary<string, bool>();
                foreach (var key in slotsDictionary.Keys)
                {
                    visitedSlots.Add(key, false);
                }

                foreach (var slot in slots)
                {
                    Assert.IsNotNull(slot);
                    Assert.IsNotNull(slot.KeyValue);
                    Assert.IsNotNull(slot.KeyValue.Key);
                    Assert.IsNotNull(slot.KeyValue.Value);
                    Assert.AreEqual(slotsDictionary[slot.KeyValue.Key], slot.KeyValue.Value);
                    visitedSlots[slot.KeyValue.Key] = true;
                }

                foreach (var visitedSlotsValue in visitedSlots.Values)
                {
                    Assert.IsTrue(visitedSlotsValue);
                }
            }
            catch (ArgumentException)
            {

            }
        }

        [TestMethod]
        [TestCategory("PatternRecognizer")]
        public void TestPatternRecognizer_RecognizeTemplate_Valid()
        {
            var validTemplateReference = "I guess I ran into [error], let me restart [offerHelp]";
            var recognizedTemplates = PatternRecognizer.Recognize(validTemplateReference);
            var expectedRecognizedTemplates = new List<string>()
            {
                "[error]",
                "[offerHelp]"
            };
            Assert.IsNotNull(recognizedTemplates);
            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("PatternRecognizer")]
        public void TestPatternRecognizer_RecognizeTemplate_InValid()
        {
            var validTemplateReference = "I guess I ran into \\[error\\], let me restart [offerHelp]";
            var recognizedTemplates = PatternRecognizer.Recognize(validTemplateReference);
            var expectedRecognizedTemplates = new List<string>()
            {
                "[offerHelp]"
            };
            Assert.IsNotNull(recognizedTemplates);
            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("PatternRecognizer")]
        public void TestPatternRecognizer_RecognizeTemplateNullValue_InValid()
        {
            try
            {
                var recognizedTemplates = PatternRecognizer.Recognize(null);
            }
            catch (ArgumentNullException)
            {

            }
        }

        [TestMethod]
        [TestCategory("PatternRecognizer")]
        public void TestPatternRecognizer_UnRecognizedPattern_Invalid()
        {
            var invalidTemplateReference = "I guess I ran into {error}, let me restart [offerHelp]";
            var recognizedTemplates = PatternRecognizer.Recognize(invalidTemplateReference);
            var expectedRecognizedTemplates = new List<string>()
            {
                "[offerHelp]"
            };
            Assert.IsNotNull(recognizedTemplates);
            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestEntityInspector_InspectText_Valid()
        {
            var activity = new Activity()
            {
                Text = "[wPhrase] sir, [offerHelp]"
            };

            var activityTextInspector = new ActivityTextInspector();
            var recognizedTemplates = (List<string>)activityTextInspector.Inspect(activity);
            var expectedRecognizedTemplates = new List<string>()
            {
                "[wPhrase]",
                "[offerHelp]"
            };

            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestEntityInspector_InspectText_InValid()
        {
            var activity = new Activity()
            {
                Text = null
            };

            var activityTextInspector = new ActivityTextInspector();
            var recognizedTemplates = (List<string>)activityTextInspector.Inspect(activity);
            var expectedRecognizedTemplates = new List<string>()
            {
            };
            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }


        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestEntityInspector_InspectSpeak_Valid()
        {
            var activity = new Activity()
            {
                Speak = "[wPhrase] sir, [offerHelp]"
            };

            var activitySpeechInspector = new ActivitySpeechInspector();
            var recognizedTemplates = (List<string>)activitySpeechInspector.Inspect(activity);
            var expectedRecognizedTemplates = new List<string>()
            {
                "[wPhrase]",
                "[offerHelp]"
            };

            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestEntityInspector_InspectSpeak_InValid()
        {
            var activity = new Activity()
            {
                Speak = null
            };

            var activitySpeechInspector = new ActivitySpeechInspector();
            var recognizedTemplates = (List<string>)activitySpeechInspector.Inspect(activity);
            var expectedRecognizedTemplates = new List<string>()
            {
            };
            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestEntityInspector_InspectSuggestedActions_Valid()
        {
            var activity = new Activity()
            {
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Text = "[wPhrase] sir, [offerHelp]",
                            DisplayText = "[wPhrase] sir, [offerHelp]"
                        }
                    }
                }
            };

            var activitySuggestedActionsInspector = new ActivitySuggestedActionsInspector();
            var recognizedTemplates = (List<string>)activitySuggestedActionsInspector.Inspect(activity);
            var expectedRecognizedTemplates = new List<string>()
            {
                "[wPhrase]",
                "[offerHelp]",
                "[wPhrase]",
                "[offerHelp]"
            };

            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestEntityInspector_InspectSuggestedActionsNullText_Valid()
        {
            var activity = new Activity()
            {
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Text = null,
                            DisplayText = "[wPhrase] sir, [offerHelp]"
                        }
                    }
                }
            };


            var activitySuggestedActionsInspector = new ActivitySuggestedActionsInspector();
            var recognizedTemplates = (List<string>)activitySuggestedActionsInspector.Inspect(activity);
            var expectedRecognizedTemplates = new List<string>()
            {
                "[wPhrase]",
                "[offerHelp]"
            };

            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestEntityInspector_InspectSuggestedActionsNullAll_InValid()
        {
            var activity = new Activity()
            {
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Text = null,
                            DisplayText = null
                        }
                    }
                }
            };


            var activitySuggestedActionsInspector = new ActivitySuggestedActionsInspector();
            var recognizedTemplates = (List<string>)activitySuggestedActionsInspector.Inspect(activity);
            var expectedRecognizedTemplates = new List<string>()
            {
            };

            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestEntityInspector_InspectAllNoDublicates_Valid()
        {
            var activity = new Activity()
            {
                Text = "[hello]",
                Speak = "[welcome]",
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Text = "[wPhrase] sir",
                            DisplayText = "[offerHelp]"
                        }
                    }
                }
            };

            var activityInspector = new ActivityInspector();
            var recognizedTemplates = (List<string>)activityInspector.Inspect(activity);
            var expectedRecognizedTemplates = new List<string>()
            {
                "[hello]",
                "[offerHelp]",
                "[wPhrase]",
                "[welcome]"
            };

            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestEntityInspector_InspectAllWithDublicates_Valid()
        {
            var activity = new Activity()
            {
                Text = "[wPhrase]",
                Speak = "[offerHelp]",
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Text = "[wPhrase] sir, [offerHelp]",
                            DisplayText = "[wPhrase] sir, [offerHelp]"
                        }
                    }
                }
            };

            var activityInspector = new ActivityInspector();
            var recognizedTemplates = (List<string>)activityInspector.Inspect(activity);
            var expectedRecognizedTemplates = new List<string>()
            {
                "[wPhrase]",
                "[offerHelp]",
            };

            CollectionAssert.AreEquivalent(expectedRecognizedTemplates, recognizedTemplates);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestActivityModifier_ModifyText_Valid()
        {
            var activity = new Activity()
            {
                Text = "[wPhrase] sir, [offerHelp]"
            };

            var compositeResponseMock = new CompositeResponseMock()
            {
                TemplateResolutions = new Dictionary<string, string>()
                {
                    {"wPhrase", "Hello" },
                    {"offerHelp", "How can I help you Today" },
                }
            };

            var activityTextModifier = new ActivityTextModifier();
            activityTextModifier.Modify(activity, compositeResponseMock);
            Assert.IsNotNull(activity.Text);
            Assert.AreEqual("Hello sir, How can I help you Today", activity.Text);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestActivityModifier_ModifySpeech_Valid()
        {
            var activity = new Activity()
            {
                Speak = "[wPhrase] sir, [offerHelp]"
            };

            var compositeResponseMock = new CompositeResponseMock()
            {
                TemplateResolutions = new Dictionary<string, string>()
                {
                    {"wPhrase", "Hello" },
                    {"offerHelp", "How can I help you Today" },
                }
            };

            var activitySpeechModifier = new ActivitySpeechModifier();
            activitySpeechModifier.Modify(activity, compositeResponseMock);
            Assert.IsNotNull(activity.Speak);
            Assert.AreEqual("Hello sir, How can I help you Today", activity.Speak);
        }

        [TestMethod]
        [TestCategory("ActivityInspector")]
        public void TestActivityModifier_ModifySuggestedActions_Valid()
        {
            var activity = new Activity()
            {
                SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Text = "[wPhrase] sir",
                            DisplayText = "[offerHelp]"
                        }
                    }
                }
            };

            var compositeResponseMock = new CompositeResponseMock()
            {
                TemplateResolutions = new Dictionary<string, string>()
                {
                    {"wPhrase", "Hello" },
                    {"offerHelp", "How can I help you Today" },
                }
            };

            var activitySuggestedActionsModifier = new ActivitySuggestedActionsModifier();
            activitySuggestedActionsModifier.Modify(activity, compositeResponseMock);


            var expectedSuggestedActions = new List<CardAction>()
            {
                new CardAction()
                {
                    Text = "Hello sir",
                    DisplayText = "How can I help you Today",
                }
            };

            var actualSuggestedActions = (List<CardAction>)activity.SuggestedActions.Actions;

            Assert.IsNotNull(activity.SuggestedActions);
            Assert.IsNotNull(activity.SuggestedActions.Actions);
            Assert.IsNotNull(expectedSuggestedActions[0].DisplayText);
            Assert.IsNotNull(expectedSuggestedActions[0].Text);
            Assert.AreEqual(expectedSuggestedActions[0].DisplayText, actualSuggestedActions[0].DisplayText);
            Assert.AreEqual(expectedSuggestedActions[0].Text, actualSuggestedActions[0].Text);
        }
    }
}
