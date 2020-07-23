// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Choice Tests")]
    public class ChoicesChannelTests
    {
        [TestMethod]
        public void ShouldReturnTrueForSupportsSuggestedActionsWithLineAnd13()
        {
            var supports = Channel.SupportsSuggestedActions(Channels.Line, 13);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnFalseForSupportsSuggestedActionsWithLineAnd14()
        {
            var supports = Channel.SupportsSuggestedActions(Channels.Line, 14);
            Assert.IsFalse(supports);
        }

        [TestMethod]
        public void ShouldReturnTrueForSupportsSuggestedActionsWithSkypeAnd10()
        {
            var supports = Channel.SupportsSuggestedActions(Channels.Skype, 10);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnFalseForSupportsSuggestedActionsWithSkypeAnd11()
        {
            var supports = Channel.SupportsSuggestedActions(Channels.Skype, 11);
            Assert.IsFalse(supports);
        }

        [TestMethod]
        public void ShouldReturnTrueForSupportsSuggestedActionsWithKikAnd20()
        {
            var supports = Channel.SupportsSuggestedActions(Channels.Kik, 20);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnFalseForSupportsSuggestedActionsWithKikAnd21()
        {
            var supports = Channel.SupportsSuggestedActions(Channels.Skype, 21);
            Assert.IsFalse(supports);
        }

        [TestMethod]
        public void ShouldReturnTrueForSupportsSuggestedActionsWithEmulatorAnd100()
        {
            var supports = Channel.SupportsSuggestedActions(Channels.Emulator, 100);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnFalseForSupportsSuggestedActionsWithEmulatorAnd101()
        {
            var supports = Channel.SupportsSuggestedActions(Channels.Emulator, 101);
            Assert.IsFalse(supports);
        }

        [TestMethod]
        public void ShouldReturnTrueForSupportsSuggestedActionsWithDirectLineSpeechAnd100()
        {
            var supports = Channel.SupportsSuggestedActions(Channels.DirectlineSpeech, 100);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnTrueForSupportsCardActionsWithDirectLineSpeechAnd99()
        {
            var supports = Channel.SupportsCardActions(Channels.DirectlineSpeech, 99);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnTrueForSupportsCardActionsWithLineAnd99()
        {
            var supports = Channel.SupportsCardActions(Channels.Line, 99);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnFalseForSupportsCardActionsWithLineAnd100()
        {
            var supports = Channel.SupportsCardActions(Channels.Line, 100);
            Assert.IsFalse(supports);
        }

        [TestMethod]
        public void ShouldReturnTrueForSupportsCardActionsWithCortanaAnd100()
        {
            var supports = Channel.SupportsCardActions(Channels.Cortana, 100);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnTrueForSupportsCardActionsWithSlackAnd100()
        {
            var supports = Channel.SupportsCardActions(Channels.Slack, 100);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnTrueForSupportsCardActionsWithSkypeAnd100()
        {
            var supports = Channel.SupportsCardActions(Channels.Skype, 3);
            Assert.IsTrue(supports);
        }

        [TestMethod]
        public void ShouldReturnFalseForSupportsCardActionsWithSkypeAnd5()
        {
            var supports = Channel.SupportsCardActions(Channels.Skype, 5);
            Assert.IsFalse(supports);
        }

        [TestMethod]
        public void ShouldReturnFalseForHasMessageFeedWithCortana()
        {
            var supports = Channel.HasMessageFeed(Channels.Cortana);
            Assert.IsFalse(supports);
        }

        [TestMethod]
        public void ShouldReturnChannelIdFromContextActivity()
        {
            var testActivity = new Schema.Activity() { ChannelId = Channels.Facebook };
            var testContext = new TurnContext(new BotFrameworkAdapter(new SimpleCredentialProvider()), testActivity);
            var channelId = Channel.GetChannelId(testContext);
            Assert.AreEqual(Channels.Facebook, channelId);
        }

        [TestMethod]
        public void ShouldReturnEmptyFromContextActivityMissingChannel()
        {
            var testActivity = new Schema.Activity() { ChannelId = null };
            var testContext = new TurnContext(new BotFrameworkAdapter(new SimpleCredentialProvider()), testActivity);
            var channelId = Channel.GetChannelId(testContext);
            Assert.AreEqual(channelId, string.Empty);
        }
    }
}
