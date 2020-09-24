// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// The ActivityFactory
    /// to generate text and then uses simple markdown semantics like chatdown to create Activity.
    /// </summary>
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class ActivityFactory
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        private const string LGType = "lgType";
        private const string AdaptiveCardType = "application/vnd.microsoft.card.adaptive";

        private static readonly Dictionary<string, string> GenericCardTypeMapping = new Dictionary<string, string>
        {
            { nameof(HeroCard).ToLowerInvariant(), HeroCard.ContentType },
            { nameof(ThumbnailCard).ToLowerInvariant(), ThumbnailCard.ContentType },
            { nameof(AudioCard).ToLowerInvariant(), AudioCard.ContentType },
            { nameof(VideoCard).ToLowerInvariant(), VideoCard.ContentType },
            { nameof(AnimationCard).ToLowerInvariant(), AnimationCard.ContentType },
            { nameof(SigninCard).ToLowerInvariant(), SigninCard.ContentType },
            { nameof(OAuthCard).ToLowerInvariant(), OAuthCard.ContentType },
            { nameof(ReceiptCard).ToLowerInvariant(), ReceiptCard.ContentType },
        };

        /// <summary>
        /// Generate the activity.
        /// Support Both string LG result and structured LG result.
        /// </summary>
        /// <param name="lgResult">lg result from languageGenerator.</param>
        /// <returns>activity.</returns>
        public static Activity FromObject(object lgResult)
        {
            if (lgResult is string lgStringResult)
            {
                return BuildActivityFromText(lgStringResult?.Trim());
            }

            try
            {
                var lgJsonResult = JObject.FromObject(lgResult);
                return BuildActivityFromLGStructuredResult(lgJsonResult);
            }
#pragma warning disable CA1031 // Do not catch general exception types (we should narrow down the exception being caught but for now we just attempt to build the activity from the text property)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return BuildActivityFromText(lgResult?.ToString()?.Trim());
            }
        }

        /// <summary>
        /// Given a lg result, create a text activity.
        /// </summary>
        /// This method will create a MessageActivity from text.
        /// <param name="text">lg text output.</param>
        /// <returns>activity with text.</returns>
        private static Activity BuildActivityFromText(string text)
        {
            var ma = Activity.CreateMessageActivity();
            ma.Text = !string.IsNullOrWhiteSpace(text) ? text : null;
            ma.Speak = !string.IsNullOrWhiteSpace(text) ? text : null;
            return ma as Activity;
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
                activity = MessageFactory.Attachment(GetAttachment(lgJObj)) as Activity;
            }
            else if (type == nameof(Activity).ToLowerInvariant())
            {
                activity = BuildActivity(lgJObj);
            }
            else
            {
                activity = BuildActivityFromText(lgJObj?.ToString()?.Trim());
            }

            return activity;
        }

        private static Activity BuildActivity(JObject lgJObj)
        {
            var activity = new JObject
            {
                ["type"] = ActivityTypes.Message
            };

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                if (property == LGType)
                {
                    continue;
                }

                var value = item.Value;

                switch (property.ToLowerInvariant())
                {
                    case "attachments":
                        activity["attachments"] = JArray.FromObject(GetAttachments(value));
                        break;
                    case "suggestedactions":
                        activity["suggestedActions"] = JObject.FromObject(GetSuggestions(value));
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

        private static IList<CardAction> GetButtons(JToken value)
        {
            var actions = NormalizedToList(value);
            return GetCardActions(actions);
        }

        private static IList<CardAction> GetCardActions(IList<JToken> actions)
        {
            return actions.Select(u => GetCardAction(u)).ToList();
        }

        private static CardAction GetCardAction(JToken cardActionJtoken)
        {
            var cardAction = new CardAction();
            if (IsStringValue(cardActionJtoken, out var actionStr))
            {
                cardAction = new CardAction(type: ActionTypes.ImBack, value: actionStr, title: actionStr);
            }
            else if (cardActionJtoken is JObject actionJObj)
            {
                var type = GetStructureType(actionJObj);
                var cardActionJson = new JObject()
                {
                    ["type"] = ActionTypes.ImBack
                };

                if (type == nameof(CardAction).ToLowerInvariant())
                {
                    foreach (var item in actionJObj)
                    {
                        cardActionJson[item.Key.Trim()] = item.Value;
                    }

                    cardAction = cardActionJson.ToObject<CardAction>();
                }
            }

            return cardAction;
        }

        private static IList<Attachment> GetAttachments(JToken value)
        {
            var attachments = new List<Attachment>();
            var attachmentsJsonList = NormalizedToList(value);

            foreach (var attachmentsJson in attachmentsJsonList)
            {
                if (attachmentsJson is JObject attachmentsJsonJObj)
                {
                    attachments.Add(GetAttachment(attachmentsJsonJObj));
                }
            }

            return attachments;
        }

        private static Attachment GetAttachment(JObject lgJObj)
        {
            Attachment attachment;

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
                attachment = new Attachment(type, content: lgJObj);
            }

            return attachment;
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
                                attachmentJson["contentType"] = GenericCardTypeMapping[type];
                            }
                            else if (type == "adaptivecard")
                            {
                                attachmentJson["contentType"] = AdaptiveCardType;
                            }
                            else
                            {
                                attachmentJson["contentType"] = type;
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
                    case "tap":
                        card[property] = JToken.FromObject(GetCardAction(value));
                        break;

                    case "image":
                    case "images":
                        if (type == HeroCard.ContentType || type == ThumbnailCard.ContentType)
                        {
                            // then it's images
                            if (card["images"] == null)
                            {
                                card["images"] = new JArray();
                            }

                            var imageList = NormalizedToList(value).ToList();
                            imageList.ForEach(u => ((JArray)card["images"]).Add(NormalizedToMediaOrImage(u)));
                        }
                        else
                        {
                            // then it's image
                            card["image"] = NormalizedToMediaOrImage(value);
                        }

                        break;

                    case "media":
                        if (card[property] == null)
                        {
                            card[property] = new JArray();
                        }

                        var mediaList = NormalizedToList(value).ToList();

                        mediaList.ForEach(u => ((JArray)card[property]).Add(NormalizedToMediaOrImage(u)));
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
                        if (IsValidBooleanValue(value, out var result))
                        {
                            card[property] = result;
                        }
                        else
                        {
                            card[property] = value;
                        }

                        break;
                    default:
                        card[property] = value;
                        break;
                }
            }

            return new Attachment(type, content: card);
        }

        private static bool IsValidBooleanValue(JToken value, out bool boolResult)
        {
            boolResult = false;

            if (value is JValue jValue && jValue.Type == JTokenType.Boolean)
            {
                boolResult = jValue.ToObject<bool>();
                return true;
            }

            if (IsStringValue(value, out var stringValue))
            {
                return bool.TryParse(stringValue, out boolResult);
            }

            return false;
        }

        private static JToken NormalizedToMediaOrImage(JToken item)
        {
            return item == null ?
                new JObject() : IsStringValue(item, out var url) ?
                new JObject() { { "url", url } } : item;
        }

        private static IList<JToken> NormalizedToList(JToken item)
        {
            return item == null ?
                new List<JToken>() :
                item is JArray array ? array.ToList() : new List<JToken>() { item };
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

        private static string GetStructureType(JObject jObj)
        {
            if (jObj == null)
            {
                return string.Empty;
            }

            var type = jObj[LGType]?.ToString()?.Trim();
            if (string.IsNullOrEmpty(type))
            {
                // Adaptive card type
                type = jObj["type"]?.ToString()?.Trim();
            }

            return type?.ToLowerInvariant() ?? string.Empty;
        }
    }
}
