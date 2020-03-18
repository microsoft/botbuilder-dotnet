// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// The ActivityFactory
    /// to generate text and then uses simple markdown semantics like chatdown to create Activity.
    /// </summary>
    public class ActivityFactory
    {
        private const string LGType = "lgType";
        private static readonly string ErrorPrefix = "[ERROR]";
        private static readonly string WarningPrefix = "[WARNING]";

        private static readonly IList<string> AllActivityTypes = GetAllPublicConstantValues<string>(typeof(ActivityTypes));
        private static readonly IList<string> AllActivityProperties = GetAllProperties(typeof(Activity));
        private static readonly IList<string> AllCardActionTypes = GetAllPublicConstantValues<string>(typeof(ActionTypes));
        private static readonly IList<string> AllCardActionProperties = GetAllProperties(typeof(CardAction));
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

        private static readonly string AdaptiveCardType = "application/vnd.microsoft.card.adaptive";

        /// <summary>
        /// Generate the activity.
        /// Support Both string LG result and structured LG result.
        /// </summary>
        /// <param name="lgResult">lg result from languageGenerator.</param>
        /// <returns>activity.</returns>
        public static Activity FromObject(object lgResult)
        {
            var diagnostics = CheckLGResult(lgResult);
            var errors = diagnostics.Where(u => u.StartsWith(ErrorPrefix));

            if (errors.Any())
            {
                throw new Exception(string.Join("\n", errors));
            }

            if (lgResult is string lgStringResult)
            {
                var isStructuredLG = ParseStructuredLGResult(lgStringResult, out var lgStructuredResult);
                return isStructuredLG ? BuildActivityFromLGStructuredResult(lgStructuredResult)
                    : BuildActivityFromText(lgStringResult?.Trim());
            }

            try
            {
                var lgJsonResult = JObject.FromObject(lgResult);
                return BuildActivityFromLGStructuredResult(lgJsonResult);
            }
            catch
            {
                return BuildActivityFromText(lgResult?.ToString()?.Trim());
            }
        }

        /// <summary>
        /// check the LG result before generate an Activity.
        /// </summary>
        /// <param name="lgResult">lg output.</param>
        /// <returns>Diagnostic list.</returns>
        public static IList<string> CheckLGResult(object lgResult)
        {
            if (lgResult is string lgStringResult)
            {
                if (string.IsNullOrWhiteSpace(lgStringResult))
                {
                    return new List<string> { BuildDiagnostic("LG output is empty", false) };
                }

                if (!lgStringResult.StartsWith("{") || !lgStringResult.EndsWith("}"))
                {
                    return new List<string> { BuildDiagnostic("LG output is not a json object, and will fallback to string format.", false) };
                }

                JObject lgStructuredResult;
                try
                {
                    lgStructuredResult = JObject.Parse(lgStringResult);
                }
                catch
                {
                    return new List<string> { BuildDiagnostic("LG output is not a json object, and will fallback to string format.", false) };
                }

                return CheckStructuredResult(lgStructuredResult);
            }
            else
            {
                JObject lgStructuredResult;
                try
                {
                    lgStructuredResult = JObject.FromObject(lgResult);
                }
                catch
                {
                    return new List<string> { BuildDiagnostic("LG output is not a json object, and will fallback to string format.", false) };
                }

                return CheckStructuredResult(lgStructuredResult);
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
                        card[property] = JObject.FromObject(GetCardAction(value));
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
                if (string.IsNullOrWhiteSpace(GetStructureType(lgStructuredResult)))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static IList<string> CheckStructuredResult(JObject lgJObj)
        {
            var result = new List<string>();
            var type = GetStructureType(lgJObj);

            //if type is empty, just parse it to text activity
            if (string.IsNullOrWhiteSpace(type))
            {
                return result;
            }

            if (GenericCardTypeMapping.ContainsKey(type)
                || type == nameof(Attachment).ToLowerInvariant())
            {
                result.AddRange(CheckAttachment(lgJObj));
            }
            else if (type == nameof(Activity).ToLowerInvariant())
            {
                result.AddRange(CheckActivity(lgJObj));
            }
            else
            {
                result.Add(BuildDiagnostic($"Type '{type}' is not supported currently.", false));
            }

            return result;
        }

        private static IList<string> CheckActivity(JObject lgJObj)
        {
            var result = new List<string>();

            var activityType = lgJObj["type"]?.ToString()?.Trim();

            result.AddRange(CheckActivityType(activityType));
            result.AddRange(CheckPropertyName(lgJObj, typeof(Activity)));
            result.AddRange(CheckActivityProperties(lgJObj));

            return result;
        }

        private static IList<string> CheckActivityType(string activityType)
        {
            var result = new List<string>();

            if (!string.IsNullOrEmpty(activityType))
            {
                if (AllActivityTypes.All(u => u.ToLowerInvariant() != activityType.ToLowerInvariant()))
                {
                    result.Add(BuildDiagnostic($"'{activityType}' is not a valid activity type."));
                }
            }

            return result;
        }

        private static IList<string> CheckActivityProperties(JObject lgJObj)
        {
            var result = new List<string>();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                var value = item.Value;

                switch (property.ToLowerInvariant())
                {
                    case "attachments":
                        result.AddRange(CheckAttachments(value));
                        break;
                    case "suggestedactions":
                        result.AddRange(CheckSuggestions(value));
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        private static IList<string> CheckSuggestions(JToken value)
        {
            var actions = NormalizedToList(value);
            return CheckCardActions(actions);
        }

        private static IList<string> CheckButtons(JToken value)
        {
            var actions = NormalizedToList(value);
            return CheckCardActions(actions);
        }

        private static IList<string> CheckCardActions(IList<JToken> actions)
        {
            var result = new List<string>();

            foreach (var action in actions)
            {
                result.AddRange(CheckCardAction(action));
            }

            return result;
        }

        private static IList<string> CheckCardAction(JToken cardActionJtoken)
        {
            var result = new List<string>();

            if (!IsStringValue(cardActionJtoken))
            {
                if (cardActionJtoken is JObject actionJObj)
                {
                    var type = GetStructureType(actionJObj);
                    if (type != nameof(CardAction).ToLowerInvariant())
                    {
                        result.Add(BuildDiagnostic($"'{type}' is not card action type.", false));
                    }
                    else
                    {
                        result.AddRange(CheckPropertyName(actionJObj, typeof(CardAction)));
                        var cardActionType = actionJObj["type"]?.ToString()?.Trim();

                        result.AddRange(CheckCardActionType(cardActionType));
                    }
                }
                else
                {
                    result.Add(BuildDiagnostic($"'{cardActionJtoken}' is not a valid card action format.", false));
                }
            }

            return result;
        }

        private static IList<string> CheckCardActionType(string cardActionType)
        {
            var result = new List<string>();

            if (!string.IsNullOrEmpty(cardActionType))
            {
                if (AllCardActionTypes.All(u => u.ToLowerInvariant() != cardActionType.ToLowerInvariant()))
                {
                    result.Add(BuildDiagnostic($"'{cardActionType}' is not a valid card action type."));
                }
            }

            return result;
        }

        private static IList<string> CheckAttachments(JToken value)
        {
            var result = new List<string>();

            var attachmentsJsonList = NormalizedToList(value);

            foreach (var attachmentsJson in attachmentsJsonList)
            {
                if (attachmentsJson is JObject attachmentsJsonJObj)
                {
                    result.AddRange(CheckAttachment(attachmentsJsonJObj));
                }
            }

            return result;
        }

        private static IList<string> CheckAttachment(JObject lgJObj)
        {
            var result = new List<string>();

            var type = GetStructureType(lgJObj);

            if (GenericCardTypeMapping.ContainsKey(type))
            {
                result.AddRange(CheckCardAtttachment(lgJObj));
            }
            else if (type == "adaptivecard")
            {
                // TODO
                // check adaptivecard format
                // it is hard to check the adaptive card without AdaptiveCards package
            }
            else if (type == nameof(Attachment).ToLowerInvariant())
            {
                // TODO
                // Check attachment format
            }
            else
            {
                result.Add(BuildDiagnostic($"'{type}' is not an attachment type.", false));
            }

            return result;
        }

        private static IList<string> CheckCardAtttachment(JObject lgJObj)
        {
            var result = new List<string>();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim().ToLowerInvariant();
                var value = item.Value;

                switch (property)
                {
                    case "buttons":
                        result.AddRange(CheckButtons(value));
                        break;

                    case "autostart":
                    case "shareable":
                    case "autoloop":
                        if (!IsValidBooleanValue(value.ToString()))
                        {
                            result.Add(BuildDiagnostic($"'{value.ToString()}' is not a boolean value."));
                        }

                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        private static IList<string> CheckPropertyName(JObject value, Type type)
        {
            var result = new List<string>();
            if (value == null)
            {
                return result;
            }

            var properties = value.Properties().Select(u => u.Name.ToLowerInvariant()).Where(u => u != LGType.ToLowerInvariant());
            IList<string> objectProperties;

            if (type == typeof(Activity))
            {
                objectProperties = AllActivityProperties;
            }
            else if (type == typeof(CardAction))
            {
                objectProperties = AllCardActionProperties;
            }
            else
            {
                objectProperties = GetAllProperties(type);
            }

            var additionalProperties = properties.Where(u => !objectProperties.Contains(u));
            if (additionalProperties.Any())
            {
                result.Add(BuildDiagnostic($"'{string.Join(",", additionalProperties)}' not support in {type.Name}.", false));
            }

            return result;
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

        private static bool IsStringValue(JToken value)
        {
            return value is JValue jValue && jValue.Type == JTokenType.String;
        }

        private static bool IsValidBooleanValue(string boolStr)
        {
            if (string.IsNullOrWhiteSpace(boolStr))
            {
                return false;
            }

            return boolStr.ToLowerInvariant() == "true" || boolStr.ToLowerInvariant() == "false";
        }

        private static string BuildDiagnostic(string message, bool isError = true)
        {
            message = message ?? string.Empty;

            return isError ? ErrorPrefix + message : WarningPrefix + message;
        }

        private static IList<T> GetAllPublicConstantValues<T>(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }

        private static IList<string> GetAllProperties(Type type)
        {
            return type.GetProperties().Select(u => u.Name.ToLowerInvariant()).ToList();
        }
    }
}
