// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// The ActivityFactory
    /// to generate text and then uses simple markdown semantics like chatdown to create Activity.
    /// </summary>
    public class ActivityFactory
    {
        public static readonly Dictionary<string, string> GenericCardTypeMapping = new Dictionary<string, string>
        {
            { nameof(HeroCard).ToLowerInvariant(), HeroCard.ContentType },
            { nameof(ThumbnailCard).ToLowerInvariant(), ThumbnailCard.ContentType },
            { nameof(AudioCard).ToLowerInvariant(), AudioCard.ContentType },
            { nameof(VideoCard).ToLowerInvariant(), VideoCard.ContentType },
            { nameof(AnimationCard).ToLowerInvariant(), AnimationCard.ContentType },
            { nameof(SigninCard).ToLowerInvariant(), SigninCard.ContentType },
            { nameof(OAuthCard).ToLowerInvariant(), OAuthCard.ContentType }
        };

        private static readonly string AdaptiveCardType = "application/vnd.microsoft.card.adaptive";

        /// <summary>
        /// Generate the activity.
        /// </summary>
        /// <param name="lgStringResult">string result from languageGenerator.</param>
        /// <returns>activity.</returns>
        public static Activity CreateActivity(string lgStringResult)
        {
            var diagnostics = ActivityChecker.Check(lgStringResult);
            var errors = diagnostics.Where(u => u.Severity == DiagnosticSeverity.Error);
            if (errors.Any())
            {
                throw new Exception(string.Join("\n", errors));
            }

            var isStructuredLG = ParseStructuredLGResult(lgStringResult, out var lgStructuredResult);
            return isStructuredLG ? BuildActivityFromLGStructuredResult(lgStructuredResult)
                : BuildActivityFromText(lgStringResult?.Trim());
        }

        /// <summary>
        /// Given a lg result, create a text activity.
        /// </summary>
        /// This method will create a MessageActivity from text.
        /// <param name="text">lg text output.</param>
        /// <returns>activity with text.</returns>
        private static Activity BuildActivityFromText(string text)
        {
            return MessageFactory.Text(text, text);
        }

        /// <summary>
        /// Given a structured lg result, create an activity.
        /// </summary>
        /// This method will create an MessageActivity from JToken
        /// <param name="lgJObj">lg output.</param>
        /// <returns>Activity for it.</returns>
        private static Activity BuildActivityFromLGStructuredResult(JObject lgJObj)
        {
            var activity = new Activity();
            var type = GetStructureType(lgJObj);

            if (GenericCardTypeMapping.ContainsKey(type)
                || type == nameof(Attachment).ToLowerInvariant())
            {
                if (GetAttachment(lgJObj, out var attachment))
                {
                    activity = MessageFactory.Attachment(attachment) as Activity;
                }
            }
            else if (type == nameof(Activity).ToLowerInvariant())
            {
                activity = BuildActivity(lgJObj);
            }

            return activity;
        }

        private static Activity BuildActivity(JObject lgJObj)
        {
            var activity = new JObject
            {
                ["Type"] = ActivityTypes.Message
            };
            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                var value = item.Value;

                switch (property.ToLowerInvariant())
                {
                    case "attachments":
                        activity["Attachments"] = JArray.FromObject(GetAttachments(value));
                        break;
                    case "suggestedactions":
                        activity["SuggestedActions"] = JObject.FromObject(GetSuggestions(value));
                        break;
                    default:
                        activity[property] = value;
                        break;
                }
            }

            return activity.ToObject<Activity>();
        }

        private static SuggestedActions GetSuggestions(JToken value)
        {
            var actions = NormalizedToList(value);

            var suggestedActions = new SuggestedActions()
            {
                Actions = GetCardActions(actions)
            };

            return suggestedActions;
        }

        private static bool IsStringValue(JToken value, out string stringValue)
        {
            stringValue = string.Empty;
            if (value is JValue jValue && jValue.Type == JTokenType.String)
            {
                stringValue = jValue.ToObject<string>().Trim();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static IList<CardAction> GetCardActions(IList<JToken> actions)
        {
            var cardActions = new List<CardAction>();
            foreach (var action in actions)
            {
                if (IsStringValue(action, out var actionStr))
                {
                    cardActions.Add(new CardAction(type: ActionTypes.ImBack, value: actionStr, title: actionStr));
                }
                else if (action is JObject actionJObj && GetCardAction(actionJObj, out var cardAction))
                {
                    cardActions.Add(cardAction);
                }
            }

            return cardActions;
        }

        private static IList<CardAction> GetButtons(JToken value)
        {
            var actions = NormalizedToList(value);
            return GetCardActions(actions);
        }

        private static bool GetCardAction(JObject cardActionJObj, out CardAction cardAction)
        {
            var type = GetStructureType(cardActionJObj);
            var cardActionJson = new JObject()
            {
                ["Type"] = ActionTypes.ImBack
            };

            var isCardAction = true;
            if (type == nameof(CardAction).ToLowerInvariant())
            {
                foreach (var item in cardActionJObj)
                {
                    cardActionJson[item.Key.Trim()] = item.Value;
                }
            }
            else
            {
                isCardAction = false;
            }

            cardAction = cardActionJson.ToObject<CardAction>();
            return isCardAction;
        }

        private static string GetStructureType(JObject jObj)
        {
            if (jObj == null)
            {
                return string.Empty;
            }

            var type = jObj["$type"]?.ToString()?.Trim();
            if (string.IsNullOrEmpty(type))
            {
                // Adaptive card type
                type = jObj["type"]?.ToString()?.Trim();
            }

            return type.ToLowerInvariant() ?? string.Empty;
        }

        private static IList<Attachment> GetAttachments(JToken value)
        {
            var attachments = new List<Attachment>();
            var attachmentsJsonList = NormalizedToList(value);

            foreach (var attachmentsJson in attachmentsJsonList)
            {
                if (attachmentsJson is JObject attachmentsJsonJObj)
                {
                    if (GetAttachment(attachmentsJsonJObj, out var attachment))
                    {
                        attachments.Add(attachment);
                    }
                }
            }

            return attachments;
        }

        private static bool GetAttachment(JObject lgJObj, out Attachment attachment)
        {
            attachment = new Attachment();
            var isAttachment = true;

            var type = GetStructureType(lgJObj);

            if (GenericCardTypeMapping.ContainsKey(type))
            {
                attachment = GetCardAtttachment(GenericCardTypeMapping[type], lgJObj);
            }
            else if (type == "adaptivecard")
            {
                attachment = new Attachment(AdaptiveCardType, content: lgJObj);
            }
            else if (type == nameof(Attachment).ToLowerInvariant())
            {
                attachment = GetNormalAttachment(lgJObj);
            }
            else
            {
                isAttachment = false;
            }

            return isAttachment;
        }

        private static Attachment GetNormalAttachment(JObject lgJObj)
        {
            var attachmentJson = new JObject();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                var value = item.Value;

                switch (property.ToLowerInvariant())
                {
                    case "contenttype":
                        {
                            var type = value.ToString().ToLowerInvariant();
                            if (GenericCardTypeMapping.ContainsKey(type))
                            {
                                attachmentJson["ContentType"] = GenericCardTypeMapping[type];
                            }
                            else if (type == "adaptivecard")
                            {
                                attachmentJson["ContentType"] = AdaptiveCardType;
                            }
                            else
                            {
                                attachmentJson["ContentType"] = type;
                            }

                            break;
                        }

                    default:
                        attachmentJson[property] = value;
                        break;
                }
            }

            return attachmentJson.ToObject<Attachment>();
        }

        private static Attachment GetCardAtttachment(string type, JObject lgJObj)
        {
            var card = new JObject();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim().ToLowerInvariant();
                var value = item.Value;

                switch (property)
                {
                    case "image":
                    case "images":
                        if (type == HeroCard.ContentType || type == ThumbnailCard.ContentType)
                        {
                            // then it's images
                            if (card["images"] == null)
                            {
                                card["images"] = new JArray();
                            }

                            var imageList = NormalizedToList(value).Select(u => u.ToString()).ToList();
                            imageList.ForEach(u => ((JArray)card["images"]).Add(new JObject() { { "url", u } }));
                        }
                        else
                        {
                            // then it's image
                            var urlObj = new JObject() { { "url", value.ToString() } };
                            card["image"] = urlObj;
                        }

                        break;

                    case "media":
                        if (card[property] == null)
                        {
                            card[property] = new JArray();
                        }

                        var mediaList = NormalizedToList(value).Select(u => u.ToString()).ToList();

                        mediaList.ForEach(u => ((JArray)card[property]).Add(new JObject() { { "url", u } }));
                        break;

                    case "buttons":
                        if (card[property] == null)
                        {
                            card[property] = new JArray();
                        }

                        GetButtons(value).ToList().ForEach(u => ((JArray)card[property]).Add(JObject.FromObject(u)));
                        break;

                    case "autostart":
                    case "shareable":
                    case "autoloop":
                        if (IsValidBooleanValue(value.ToString(), out var result))
                        {
                            card[property] = result;
                        }

                        break;
                    default:
                        card[property] = value;
                        break;
                }
            }

            return new Attachment(type, content: card);
        }

        private static bool IsValidBooleanValue(string boolValue, out bool boolResult)
        {
            boolResult = false;
            if (string.IsNullOrWhiteSpace(boolValue))
            {
                return false;
            }

            if (boolValue.ToLowerInvariant() == "true")
            {
                boolResult = true;
                return true;
            }
            else if (boolValue.ToLowerInvariant() == "false")
            {
                boolResult = false;
                return true;
            }

            return false;
        }

        private static IList<JToken> NormalizedToList(JToken item)
        {
            return item == null ? 
                new List<JToken>() :
                item is JArray array ? array.ToList() : new List<JToken>() { item };
        }

        /// <summary>
        /// parse the lg string output. If the output is structured result, get the object result and return true.
        /// </summary>
        /// <param name="lgStringResult">lg string output.</param>
        /// <param name="lgStructuredResult">lg json object result.</param>
        /// <returns>judge if the lg string output is structured result.</returns>
        private static bool ParseStructuredLGResult(string lgStringResult, out JObject lgStructuredResult)
        {
            lgStructuredResult = new JObject();
            lgStringResult = lgStringResult?.Trim();

            if (string.IsNullOrWhiteSpace(lgStringResult)
                || !lgStringResult.StartsWith("{") || !lgStringResult.EndsWith("}"))
            {
                return false;
            }

            try
            {
                lgStructuredResult = JObject.Parse(lgStringResult);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
