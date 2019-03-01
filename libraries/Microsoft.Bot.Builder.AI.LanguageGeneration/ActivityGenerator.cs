using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class AttachmentGenerationConfig
    {
        public string AttachementTemplateId { set; get; }

        // Default non adaptive card.
        public bool IsAdaptiveCard { set; get; } = false;
    }

    public class ActivityGenerationConfig
    {
        public string TextSpeakTemplateId { set; get; }

        public string TextSpeakSeperator { set; get; } = "||";

        public List<AttachmentGenerationConfig> Attachments { set; get; }

        public string AttachmentLayoutType { set; get; } = AttachmentLayoutTypes.Carousel;
    }


    /// <summary>
    /// This ActivityGenrator will help you generate a full activity 
    /// with text, speak, and card (both adaptive and non-adaptive cards)
    /// </summary>
    public class ActivityGenerator
    {
        private TemplateEngine templateEngine;

        public ActivityGenerator(TemplateEngine templateEngine)
        {
            this.templateEngine = templateEngine;
        }

        public Activity Generate(ActivityGenerationConfig options, object data)
        {
            var activity = new Activity();
            activity.Type = ActivityTypes.Message;

            (activity.Text, activity.Speak) = GenerateTextAndSpeak(options.TextSpeakTemplateId, options.TextSpeakSeperator, data);
            activity.Attachments = GenerateAttachments(options.Attachments, data);
            activity.AttachmentLayout = options.AttachmentLayoutType;

            return activity;
        }

        private (string, string) GenerateTextAndSpeak(string textSpeakTemplateId, string textSpeakSepartor, object data)
        {
            string text = null;
            string speak = null;
            if (!string.IsNullOrEmpty(textSpeakTemplateId) && !string.IsNullOrEmpty(textSpeakSepartor))
            {
                var value = this.templateEngine.EvaluateTemplate(textSpeakTemplateId, data);
                var valueList = value.Split(textSpeakSepartor);

                if (valueList.Length == 1)
                {
                    text = valueList[0];
                    speak = valueList[0];
                }
                else if (valueList.Length == 2)
                {
                    text = valueList[0].Length > 0 && valueList[0].Last().Equals(' ') ? valueList[0].Remove(valueList[0].Length - 1) : valueList[0];
                    speak = valueList[1].TrimStart();
                }
                else
                {
                    throw new Exception(string.Format("The format of LG template {0} is wrong.", textSpeakTemplateId));
                }
            }

            return (text, speak);
        }

        private List<Attachment> GenerateAttachments(List<AttachmentGenerationConfig> attachmentGenerationConfigs, object data)
        {
            var attachments = new List<Attachment>();
            if (attachmentGenerationConfigs != null)
            {
                foreach (var attachmentGenerationConfig in attachmentGenerationConfigs)
                {
                    try
                    {
                        if (attachmentGenerationConfig.IsAdaptiveCard)
                        {
                            attachments.Add(GenerateAdaptiveCardAttachment(attachmentGenerationConfig.AttachementTemplateId, data));
                        }
                        else
                        {
                            attachments.Add(GenerateNonAdaptiveCardAttachment(attachmentGenerationConfig.AttachementTemplateId, data));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("The format of LG template {0} is wrong with the exception: {1}.", attachmentGenerationConfig.AttachementTemplateId, ex.Message));
                    }
                }
            }

            return attachments;
        }

        private Attachment GenerateAdaptiveCardAttachment(string adaptiveCardTemplateId, object data)
        {
            var cardValue = this.templateEngine.EvaluateTemplate(adaptiveCardTemplateId, data);
            var card = AdaptiveCard.FromJson(cardValue).Card;
            var cardObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card));
            var attachment = new Attachment(AdaptiveCard.ContentType, content: cardObj);
            return attachment;
        }

        private Attachment GenerateNonAdaptiveCardAttachment(string nonAdaptiveCardTemplateId, object data)
        {
            var card = this.templateEngine.EvaluateTemplate(nonAdaptiveCardTemplateId, data);
            card = card.Trim();
            card = card.Substring(1, card.Length - 2);
            var splits = card.Split("\r\n");
            var lines = splits.OfType<string>().ToList();
            var cardType = lines[0].Trim();
            lines.RemoveAt(0);
            var cardObj = new JObject();
            foreach (var line in lines)
            {
                var start = line.IndexOf('=');
                var property = line.Substring(0, start).Trim().ToLower();
                var value = line.Substring(start + 1).Trim();
                switch (property)
                {
                    case "title":
                    case "subtitle":
                    case "text":
                    case "aspect":
                    case "value":
                    case "connectionName":
                        cardObj[property] = value;
                        break;
                    case "image":
                        var urlObj = new JObject() { { "url", value } };
                        cardObj.Add(property, urlObj);
                        break;
                    case "images":
                        if (cardObj[property] == null)
                        {
                            cardObj[property] = new JArray();
                        }

                        urlObj = new JObject() { { "url", value } };
                        ((JArray)cardObj[property]).Add(urlObj);
                        break;
                    case "media":
                        if (cardObj[property] == null)
                        {
                            cardObj[property] = new JArray();
                        }
                        var mediaObj = new JObject() { { "url", value } };
                        ((JArray)cardObj[property]).Add(mediaObj);
                        break;
                    case "buttons":
                        if (cardObj[property] == null)
                        {
                            cardObj[property] = new JArray();
                        }
                        foreach (var button in value.Split('|'))
                        {
                            var buttonObj = new JObject() { { "title", button.Trim() }, { "type", "imBack" }, { "value", button.Trim() } };
                            ((JArray)cardObj[property]).Add(buttonObj);
                        }
                        break;
                    case "autostart":
                    case "sharable":
                    case "autoloop":
                        cardObj[property] = value.ToLower() == "true";
                        break;
                    case "":
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(string.Format("Skipping unknown card property {0}", property));
                        break;
                }
            }

            // ToDo: generate Card Type
            Attachment attachment;
            switch (cardType)
            {
                case "Herocard":
                    attachment = new Attachment(HeroCard.ContentType, content: cardObj);
                    break;
                case "Thumbnailcard":
                    attachment = new Attachment(ThumbnailCard.ContentType, content: cardObj);
                    break;
                case "Audiocard":
                    attachment = new Attachment(AudioCard.ContentType, content: cardObj);
                    break;
                case "Videocard":
                    attachment = new Attachment(VideoCard.ContentType, content: cardObj);
                    break;
                case "Animationcard":
                    attachment = new Attachment(AnimationCard.ContentType, content: cardObj);
                    break;
                case "Signincard":
                    attachment = new Attachment(SigninCard.ContentType, content: cardObj);
                    break;
                case "Oauthcard":
                    attachment = new Attachment(OAuthCard.ContentType, content: cardObj);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine(string.Format("Card type {0} is not support!", cardType));
                    attachment = new Attachment();
                    break;
            }

            return attachment;
        }
    }
}
