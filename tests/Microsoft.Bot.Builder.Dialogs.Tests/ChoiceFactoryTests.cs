// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [Trait("TestCategory", "Prompts")]
    [Trait("TestCategory", "Choice Tests")]
    public class ChoiceFactoryTests
    {
        private static List<Choice> colorChoices = new List<Choice> { new Choice("red"), new Choice("green"), new Choice("blue") };
        private static List<Choice> extraChoices = new List<Choice> { new Choice("red"), new Choice("green"), new Choice("blue"), new Choice("alpha") };
        private static List<Choice> choicesWithActions = new List<Choice>
        {
            new Choice("ImBack") { Action = new CardAction(ActionTypes.ImBack, "ImBack Action", value: "ImBack Value") },
            new Choice("MessageBack") { Action = new CardAction(ActionTypes.MessageBack, "MessageBack Action", value: "MessageBack Value") },
            new Choice("PostBack") { Action = new CardAction(ActionTypes.PostBack, "PostBack Action", value: "PostBack Value") },
        };

        [Fact]
        public void ShouldRenderChoicesInline()
        {
            var activity = ChoiceFactory.Inline(colorChoices, "select from:");
            Assert.Equal("select from: (1) red, (2) green, or (3) blue", activity.Text);
        }

        [Fact]
        public void ShouldRenderChoicesAsAList()
        {
            var activity = ChoiceFactory.List(colorChoices, "select from:");
            Assert.Equal("select from:\n\n   1. red\n   2. green\n   3. blue", activity.Text);
        }

        [Fact]
        public void ShouldRenderUnincludedNumbersChoicesAsAList()
        {
            var activity = ChoiceFactory.List(colorChoices, "select from:", options: new ChoiceFactoryOptions { IncludeNumbers = false });
            Assert.Equal("select from:\n\n   - red\n   - green\n   - blue", activity.Text);
        }

        [Fact]
        public void ShouldRenderChoicesAsSuggestedActions()
        {
            var activity = ChoiceFactory.SuggestedAction(colorChoices, "select from:", null, null);
            Assert.Equal("select from:", activity.Text);
            Assert.NotNull(activity.SuggestedActions);
            Assert.Equal(3, activity.SuggestedActions.Actions.Count);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[0].Type);
            Assert.Equal("red", activity.SuggestedActions.Actions[0].Value);
            Assert.Equal("red", activity.SuggestedActions.Actions[0].Title);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[1].Type);
            Assert.Equal("green", activity.SuggestedActions.Actions[1].Value);
            Assert.Equal("green", activity.SuggestedActions.Actions[1].Title);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[2].Type);
            Assert.Equal("blue", activity.SuggestedActions.Actions[2].Value);
            Assert.Equal("blue", activity.SuggestedActions.Actions[2].Title);
        }

        [Fact]
        public void ShouldRenderChoicesAsHeroCard()
        {
            var activity = ChoiceFactory.HeroCard(colorChoices, "select from:");

            Assert.NotNull(activity.Attachments);

            var heroCard = (HeroCard)activity.Attachments.First().Content;

            Assert.Equal(3, heroCard.Buttons.Count);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[0].Type);
            Assert.Equal("red", heroCard.Buttons[0].Value);
            Assert.Equal("red", heroCard.Buttons[0].Title);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[1].Type);
            Assert.Equal("green", heroCard.Buttons[1].Value);
            Assert.Equal("green", heroCard.Buttons[1].Title);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[2].Type);
            Assert.Equal("blue", heroCard.Buttons[2].Value);
            Assert.Equal("blue", heroCard.Buttons[2].Title);
        }

        [Fact]
        public void ShouldAutomaticallyChooseRenderStyleBasedOnChannelType()
        {
            var activity = ChoiceFactory.ForChannel(Channels.Emulator, colorChoices, "select from:", null, null);
            Assert.Equal("select from:", activity.Text);
            Assert.NotNull(activity.SuggestedActions);
            Assert.Equal(3, activity.SuggestedActions.Actions.Count);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[0].Type);
            Assert.Equal("red", activity.SuggestedActions.Actions[0].Value);
            Assert.Equal("red", activity.SuggestedActions.Actions[0].Title);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[1].Type);
            Assert.Equal("green", activity.SuggestedActions.Actions[1].Value);
            Assert.Equal("green", activity.SuggestedActions.Actions[1].Title);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[2].Type);
            Assert.Equal("blue", activity.SuggestedActions.Actions[2].Value);
            Assert.Equal("blue", activity.SuggestedActions.Actions[2].Title);
        }

        [Fact]
        public void ShouldChooseCorrectStylesForCortana()
        {
            var activity = ChoiceFactory.ForChannel(Channels.Cortana, colorChoices, "select from:", null, null);

            Assert.NotNull(activity.Attachments);

            var heroCard = (HeroCard)activity.Attachments.First().Content;

            Assert.Equal(3, heroCard.Buttons.Count);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[0].Type);
            Assert.Equal("red", heroCard.Buttons[0].Value);
            Assert.Equal("red", heroCard.Buttons[0].Title);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[1].Type);
            Assert.Equal("green", heroCard.Buttons[1].Value);
            Assert.Equal("green", heroCard.Buttons[1].Title);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[2].Type);
            Assert.Equal("blue", heroCard.Buttons[2].Value);
            Assert.Equal("blue", heroCard.Buttons[2].Title);
        }

        [Fact]
        public void ShouldChooseCorrectStylesForTeamsPersonalChat()
        {
            var recipientsList = new List<string>() { "UserId" };
            var activity = ChoiceFactory.ForChannel(Channels.Msteams, colorChoices, "select from:", conversationType: "personal", toList: recipientsList);

            Assert.Equal("select from:", activity.Text);
            Assert.NotNull(activity.SuggestedActions);
            Assert.Equal(3, activity.SuggestedActions.Actions.Count);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[0].Type);
            Assert.Equal("red", activity.SuggestedActions.Actions[0].Value);
            Assert.Equal("red", activity.SuggestedActions.Actions[0].Title);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[1].Type);
            Assert.Equal("green", activity.SuggestedActions.Actions[1].Value);
            Assert.Equal("green", activity.SuggestedActions.Actions[1].Title);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[2].Type);
            Assert.Equal("blue", activity.SuggestedActions.Actions[2].Value);
            Assert.Equal("blue", activity.SuggestedActions.Actions[2].Title);
            Assert.Equal("UserId", activity.SuggestedActions.To[0]);
        }

        [Fact]
        public void ShouldChooseCorrectStylesForTeamsGroupChat()
        {
            var activity = ChoiceFactory.ForChannel(Channels.Msteams, colorChoices, "select from:", conversationType: "groupChat");

            Assert.NotNull(activity.Attachments);

            var heroCard = (HeroCard)activity.Attachments.First().Content;

            Assert.Equal(3, heroCard.Buttons.Count);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[0].Type);
            Assert.Equal("red", heroCard.Buttons[0].Value);
            Assert.Equal("red", heroCard.Buttons[0].Title);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[1].Type);
            Assert.Equal("green", heroCard.Buttons[1].Value);
            Assert.Equal("green", heroCard.Buttons[1].Title);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[2].Type);
            Assert.Equal("blue", heroCard.Buttons[2].Value);
            Assert.Equal("blue", heroCard.Buttons[2].Title);
        }

        [Fact]
        public void ShouldIncludeChoiceActionsInSuggestedActions()
        {
            var activity = ChoiceFactory.SuggestedAction(choicesWithActions, "select from:", null, null);
            Assert.Equal("select from:", activity.Text);
            Assert.NotNull(activity.SuggestedActions);
            Assert.Equal(3, activity.SuggestedActions.Actions.Count);
            Assert.Equal(ActionTypes.ImBack, activity.SuggestedActions.Actions[0].Type);
            Assert.Equal("ImBack Value", activity.SuggestedActions.Actions[0].Value);
            Assert.Equal("ImBack Action", activity.SuggestedActions.Actions[0].Title);
            Assert.Equal(ActionTypes.MessageBack, activity.SuggestedActions.Actions[1].Type);
            Assert.Equal("MessageBack Value", activity.SuggestedActions.Actions[1].Value);
            Assert.Equal("MessageBack Action", activity.SuggestedActions.Actions[1].Title);
            Assert.Equal(ActionTypes.PostBack, activity.SuggestedActions.Actions[2].Type);
            Assert.Equal("PostBack Value", activity.SuggestedActions.Actions[2].Value);
            Assert.Equal("PostBack Action", activity.SuggestedActions.Actions[2].Title);
        }

        [Fact]
        public void ShouldIncludeChoiceActionsInHeroCards()
        {
            var activity = ChoiceFactory.HeroCard(choicesWithActions, "select from:");

            Assert.NotNull(activity.Attachments);

            var heroCard = (HeroCard)activity.Attachments.First().Content;

            Assert.Equal(3, heroCard.Buttons.Count);
            Assert.Equal(ActionTypes.ImBack, heroCard.Buttons[0].Type);
            Assert.Equal("ImBack Value", heroCard.Buttons[0].Value);
            Assert.Equal("ImBack Action", heroCard.Buttons[0].Title);
            Assert.Equal(ActionTypes.MessageBack, heroCard.Buttons[1].Type);
            Assert.Equal("MessageBack Value", heroCard.Buttons[1].Value);
            Assert.Equal("MessageBack Action", heroCard.Buttons[1].Title);
            Assert.Equal(ActionTypes.PostBack, heroCard.Buttons[2].Type);
            Assert.Equal("PostBack Value", heroCard.Buttons[2].Value);
            Assert.Equal("PostBack Action", heroCard.Buttons[2].Title);
        }
    }
}
