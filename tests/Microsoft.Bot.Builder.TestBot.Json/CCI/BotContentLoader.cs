using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Composition;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Builder.Dialogs.Flow;
using Microsoft.Bot.Schema;
using Microsoft.CCI.Content;
using Microsoft.CCI.Content.Entities;
using Microsoft.CCI.Content.EntityProperties;
using Microsoft.CCI.Content.EntityProperties.DialogNodes;
using Microsoft.CCI.Content.Enums;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.TestBot.Json.CCI
{
    public class BotContentLoader
    {
        private readonly BotContent _botContent;
        private readonly Dictionary<string, Microsoft.CCI.Content.Entities.Dialog> _dialogs;
        private readonly Dictionary<string, ContextVariable> _contextVariables;
        private readonly Dictionary<string, BotMessage> _messages;
        private readonly Dictionary<string, NamedEntity> _namedEntities;
        private readonly Dictionary<string, IDialog> _dialogsCache;

        private BotContentLoader(string contents)
        {
            _botContent = JsonConvert.DeserializeObject<BotContent>(contents);
            _dialogs = _botContent.Dialogs.ToDictionary(d => d.Id);
            _contextVariables = _botContent.ContextVariables.ToDictionary(cv => cv.Id);
            _messages = _botContent.BotMessages.ToDictionary(m => m.Id);
            _namedEntities = _botContent.NamedEntities.ToDictionary(e => e.Id);

            _dialogsCache = new Dictionary<string, IDialog>();
        }

        public static IntentDialog Load(string contents)
        {
            var contentLoader = new BotContentLoader(contents);
            return new IntentDialog()
            {
                Id = contentLoader._botContent.Version,
                Recognizer = new ExactMatchRecognizer(contentLoader._botContent.Intents),
                Routes = contentLoader._botContent.Intents.ToDictionary(
                    i => i.Id,
                    i => contentLoader.GetDialog(i.DialogId))
            };
        }

        private IDialog GetDialog(string dialogId)
        {
            if (!_dialogsCache.TryGetValue(dialogId, out IDialog dialog))
            {
                _dialogsCache[dialogId] = new SequenceDialog(dialogId)
                {
                    InitialDialogId = _dialogs[dialogId].RootNodeId,
                    Sequence = CreateSequence(_dialogs[dialogId])
                };
            }

            return _dialogsCache[dialogId];
        }

        internal Sequence CreateSequence(Microsoft.CCI.Content.Entities.Dialog dialog)
        {
            Sequence sequence = new Sequence();
            sequence.Add(new GotoStep()
            {
                Id = $"start_{dialog.Id}",
                CommandId = dialog.RootNodeId
            });

            foreach (var item in dialog.MessageNodes)
            {
                var messageContent = _messages[item.BotMessageId].ChannelContent["web"];

                sequence.Add(new SendActivityTemplateStep()
                {
                    Id = item.Id,
                    Activity = GetActivity(messageContent)
                });

                if (!string.IsNullOrEmpty(item.DefaultTargetNodeId))
                {
                    sequence.Add(new GotoStep()
                    {
                        Id = $"goto{item.Id}",
                        CommandId = item.DefaultTargetNodeId
                    });
                }
            }

            foreach (var item in dialog.QuestionNodes)
            {
                ContextVariable outputVariable = _contextVariables[item.ContextVariableId];
                BotMessageContent questionContent = _messages[outputVariable.Messages[VariableMessageType.GetValue][0]].ChannelContent["web"];
                string questionText = _messages[outputVariable.Messages[VariableMessageType.GetValue][0]].ChannelContent["web"].Content;

                if (!outputVariable.NamedEntityOptions.Any())
                {
                    outputVariable.NamedEntityOptions = new List<string>()
                    {
                        "2b0a33c4-5420-4719-9852-9502708ab461",
                        "c45bff0e-28fa-48a4-919a-a99e91eecc68"
                    };
                }

                sequence.Add(new CallDialog()
                {
                    Id = item.Id,
                    Dialog = new TextPrompt($"{item.Id}_Prompt")
                    {
                        InitialPrompt = GetActivity(questionContent, outputVariable.NamedEntityOptions),
                        Property = item.ContextVariableId
                    }
                });
                SwitchStep2 switchStep = new SwitchStep2()
                {
                    Id = $"{item.Id}_Switch",
                    Routes = new List<Tuple<IExpressionEval, IStep>>()
                };
                sequence.Add(switchStep);

                foreach (var route in item.Routes)
                {
                    GotoStep gotoStep = new GotoStep()
                    {
                        Id = $"goto{item.Id}",
                        CommandId = route.TargetNodeId
                    };

                    sequence.Add(gotoStep);
                    switchStep.Routes.Add(Tuple.Create<IExpressionEval, IStep>(new CCIExpression(route.Expression), gotoStep));
                }
            }

            foreach (var item in dialog.DialogChangeNodes)
            {
                sequence.Add(new CallDialog()
                {
                    Id = item.Id,
                    Dialog = GetDialog(item.TargetDialogId)
                });
            }

            return sequence;
        }


        private ActivityTemplate GetActivity(BotMessageContent messageContent, IList<string> namedEntitites = null)
        {
            SuggestedActions suggestedActions = namedEntitites == null ? null : new SuggestedActions(
                actions: namedEntitites
                    .Select(e => new CardAction
                    {
                        Type = ActionTypes.ImBack,
                        Title = _namedEntities[e].DisplayName,
                        Value = _namedEntities[e].Id,
                        Text = _namedEntities[e].DisplayName,
                        DisplayText = _namedEntities[e].DisplayName,
                    })
                    .ToList());

            if (messageContent.ContentFormat == BotMessageContentFormat.AdaptiveCard)
            {
                return new ActivityTemplate(
                    new Attachment(contentType: "application/vnd.microsoft.card.adaptive", content: messageContent.Content),
                    suggestedActions);
            }

            return new ActivityTemplate(
                messageContent.Content,
                messageContent.ContentFormat.ToString(),
                suggestedActions);
        }
    }
}
