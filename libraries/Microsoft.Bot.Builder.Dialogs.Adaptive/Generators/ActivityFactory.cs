// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Xml;
using AdaptiveCards;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Connector;
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
            { nameof(HeroCard).ToLower(), HeroCard.ContentType },
            { nameof(ThumbnailCard).ToLower(), ThumbnailCard.ContentType },
            { nameof(AudioCard).ToLower(), AudioCard.ContentType },
            { nameof(VideoCard).ToLower(), VideoCard.ContentType },
            { nameof(AnimationCard).ToLower(), AnimationCard.ContentType },
            { nameof(SigninCard).ToLower(), SigninCard.ContentType },
            { nameof(OAuthCard).ToLower(), OAuthCard.ContentType }
        };

        /// <summary>
        /// Generate the activity. 
        /// </summary>
        /// <param name="lgStringResult">string result from languageGenerator.</param>
        /// <returns>activity.</returns>
        public static Activity CreateActivity(string lgStringResult)
        {
            JObject lgStructuredResult;
            try
            {
                lgStructuredResult = JObject.Parse(lgStringResult);
            }
            catch
            {
                return BuildActivityFromText(lgStringResult?.ToString()?.Trim());
            }

            return BuildActivityFromLGStructuredResult(lgStructuredResult);
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
            Activity activity;
            var type = GetStructureType(lgJObj);

            if (GenericCardTypeMapping.ContainsKey(type) && GetAttachment(lgJObj, out var attachment))
            {
                activity = MessageFactory.Attachment(attachment) as Activity;
            }
            else
            {
                if (type == nameof(Activity).ToLower())
                {
                    activity = BuildActivityFromObject(lgJObj);
                }
                else
                {
                    throw new Exception($"type {type} is not support currently.");
                }
            }

            return activity;
        }

        private static Activity BuildActivityFromObject(JObject lgJObj)
        {
            Activity activity;

            // Currently Event and Message type are supported.
            if (lgJObj["type"]?.ToString() == ActivityTypes.Event)
            {
                activity = BuildEventActivity(lgJObj) as Activity;
            }
            else
            {
                activity = BuildMessageActivity(lgJObj) as Activity;
            }

            return activity;
        }

        private static IEventActivity BuildEventActivity(JObject lgJObj)
        {
            var activity = Activity.CreateEventActivity();
            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                var value = item.Value;

                switch (property.ToLower())
                {
                    case "$type":
                        break;

                    case "name":
                        activity.Name = value.ToString();
                        break;

                    case "value":
                        activity.Value = value.ToString();
                        break;

                    default:
                        Debug.WriteLine(string.Format("Skipping unknown activity property {0}", property));
                        break;
                }
            }

            return activity;
        }

        private static IMessageActivity BuildMessageActivity(JObject lgJObj)
        {
            var activity = Activity.CreateMessageActivity();
            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                var value = item.Value;

                switch (property.ToLower())
                {
                    case "$type":
                        break;

                    case "text":
                        activity.Text = value.ToString();
                        break;

                    case "speak":
                        activity.Speak = value.ToString();
                        break;

                    case "inputhint":
                        activity.InputHint = value.ToString();
                        break;

                    case "attachments":
                        activity.Attachments = GetAttachments(value);
                        break;

                    case "suggestedactions":
                        activity.SuggestedActions = GetSuggestions(value);
                        break;

                    case "attachmentlayout":
                        activity.AttachmentLayout = value.ToString();
                        break;

                    default:
                        Debug.WriteLine(string.Format("Skipping unknown activity property {0}", property));
                        break;
                }
            }

            return activity;
        }

        private static SuggestedActions GetSuggestions(JToken value)
        {
            var suggestionActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
            };

            var actions = NormalizedToList(value);

            foreach (var action in actions)
            {
                if (action is JValue jValue && jValue.Type == JTokenType.String)
                {
                    var actionStr = jValue.ToObject<string>().Trim();
                    suggestionActions.Actions.Add(new CardAction(type: ActionTypes.MessageBack, title: actionStr, displayText: actionStr, text: actionStr));
                }
                else if (action is JObject actionJObj && GetCardAction(string.Empty, actionJObj, out var cardAction))
                {
                    suggestionActions.Actions.Add(cardAction);
                }
            }

            return suggestionActions;
        }

        private static List<CardAction> GetButtons(string cardType, JToken value)
        {
            var buttons = new List<CardAction>();
            var actions = NormalizedToList(value);

            foreach (var action in actions)
            {
                if (action is JValue jValue && jValue.Type == JTokenType.String)
                {
                    var actionStr = jValue.ToObject<string>().Trim();
                    if (cardType == SigninCard.ContentType || cardType == OAuthCard.ContentType)
                    {
                        buttons.Add(new CardAction(type: ActionTypes.Signin, title: actionStr, value: actionStr));
                    }
                    else
                    {
                        buttons.Add(new CardAction(type: ActionTypes.ImBack, title: actionStr, value: actionStr));
                    }
                }
                else if (action is JObject actionJObj && GetCardAction(cardType, actionJObj, out var cardAction))
                {
                    buttons.Add(cardAction);
                }
            }

            return buttons;
        }

        private static bool GetCardAction(string cardType, JObject cardActionJObj, out CardAction cardAction)
        {
            var type = GetStructureType(cardActionJObj);
            cardAction = new CardAction();
            if (cardType == SigninCard.ContentType || cardType == OAuthCard.ContentType)
            {
                cardAction.Type = ActionTypes.Signin;
            }
            else
            {
                cardAction.Type = ActionTypes.ImBack;
            }

            var isCardAction = true;
            if (type == nameof(CardAction).ToLower())
            {
                foreach (var item in cardActionJObj)
                {
                    var property = item.Key.Trim();
                    var value = item.Value;

                    switch (property.ToLower())
                    {
                        case "type":
                            cardAction.Type = value.ToString();
                            break;

                        case "title":
                            cardAction.Title = value.ToString();
                            break;

                        case "value":
                            cardAction.Value = value.ToString();
                            break;

                        case "displaytext":
                            cardAction.DisplayText = value.ToString();
                            break;

                        case "text":
                            cardAction.Text = value.ToString();
                            break;

                        case "image":
                            cardAction.Image = value.ToString();
                            break;

                        default:
                            Debug.WriteLine(string.Format("Skipping unknown activity property {0}", property));
                            break;
                    }
                }
            }
            else
            {
                isCardAction = false;
            }

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

            return type.ToLower() ?? string.Empty;
        }

        private static List<Attachment> GetAttachments(JToken value)
        {
            var attachments = new List<Attachment>();
            var attachmentsJsonList = NormalizedToList(value);

            foreach (var attachmentsJson in attachmentsJsonList)
            {
                if (attachmentsJson is JObject attachmentsJsonJObj && GetAttachment(attachmentsJsonJObj, out var attachment))
                {
                    attachments.Add(attachment);
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
            else if (type == nameof(AdaptiveCard).ToLower())
            {
                attachment = new Attachment(AdaptiveCard.ContentType, content: lgJObj);
            }
            else
            {
                isAttachment = false;
            }

            return isAttachment;
        }

        private static Attachment GetCardAtttachment(string type, JObject lgJObj)
        {
            var attachment = new Attachment(type, content: new JObject());
            BuildGenericCard(attachment.Content, type, lgJObj);
            return attachment;
        }

        private static void BuildGenericCard(dynamic card, string type, JObject lgJObj)
        {
            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim().ToLower();
                var value = item.Value;

                switch (property)
                {
                    case "title":
                    case "subtitle":
                    case "text":
                    case "aspect":
                    case "value":
                    case "connectionname":
                        card[property] = value;
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

                        GetButtons(type, value).ForEach(u => ((JArray)card[property]).Add(JObject.FromObject(u)));
                        break;

                    case "autostart":
                    case "shareable":
                    case "autoloop":
                        if (value.ToString().ToLower() == "true")
                        {
                            card[property] = true;
                        }
                        else if (value.ToString().ToLower() == "false")
                        {
                            card[property] = false;
                        }

                        break;
                    case "":
                        break;
                    default:
                        Debug.WriteLine(string.Format("Skipping unknown card property {0}", property));
                        break;
                }
            }
        }

        private static List<JToken> NormalizedToList(JToken item)
        {
            return item == null ? 
                new List<JToken>() :
                item is JArray array ? array.ToList() : new List<JToken>() { item };
        }
    }
}
