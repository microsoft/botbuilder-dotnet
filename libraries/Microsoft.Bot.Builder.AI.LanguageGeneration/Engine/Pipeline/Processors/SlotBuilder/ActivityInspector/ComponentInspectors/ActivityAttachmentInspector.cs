using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// Class to inspect/search a <see cref="Activity"/> object for template references in <see cref="Activity.Attachments"/>. 
    /// </summary>
    internal class ActivityAttachmentInspector : IActivityComponentInspector
    {
        /// <summary>
        /// Inspect/search a <see cref="Activity"/> object for template references in <see cref="Activity.Attachments"/>. 
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be searched.</param>
        /// <returns>A <see cref="IList{string}"/> containing all the referenced templates in <see cref="Activity.Attachments"/>.</returns>
        public IList<string> Inspect(Activity activity)
        {
            if (activity.Attachments == null || activity.Attachments.Count == 0)
            {
                return new List<string>();
            }
            else
            {
                var referencedTemplates = new List<string>();
                foreach (var attachment in activity.Attachments)
                {
                    if (attachment.ContentType == HeroCard.ContentType)
                    {
                        var content = (attachment.Content) as HeroCard;
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Title));
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Subtitle));
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Text));
                    }
                    else if (attachment.ContentType == ReceiptCard.ContentType)
                    {
                        var content = (attachment.Content) as ReceiptCard;
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Title));

                        var receiptCardItems = content.Items;
                        if (receiptCardItems != null && receiptCardItems.Count > 0)
                        {
                            foreach (var item in receiptCardItems)
                            {
                                referencedTemplates.AddRange(PatternRecognizer.Recognize(item.Text));
                                referencedTemplates.AddRange(PatternRecognizer.Recognize(item.Title));
                                referencedTemplates.AddRange(PatternRecognizer.Recognize(item.Subtitle));
                            }
                        }

                        var buttons = content.Buttons;
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            referencedTemplates.AddRange(InspectCardActions(content.Buttons));
                        }
                    }
                    else if (attachment.ContentType == SigninCard.ContentType)
                    {
                        var content = (attachment.Content) as SigninCard;
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Text));
                        var buttons = content.Buttons;
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            referencedTemplates.AddRange(InspectCardActions(content.Buttons));
                        }
                    }
                    else if (attachment.ContentType == AnimationCard.ContentType)
                    {
                        var content = (attachment.Content) as AnimationCard;
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Text));
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Subtitle));
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Title));
                        var buttons = content.Buttons;
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            referencedTemplates.AddRange(InspectCardActions(content.Buttons));
                        }
                    }
                    else if (attachment.ContentType == VideoCard.ContentType)
                    {
                        var content = (attachment.Content) as VideoCard;
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Text));
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Subtitle));
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Title));
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            referencedTemplates.AddRange(InspectCardActions(content.Buttons));
                        }
                    }
                    else if (attachment.ContentType == OAuthCard.ContentType)
                    {
                        var content = (attachment.Content) as OAuthCard;
                        referencedTemplates.AddRange(PatternRecognizer.Recognize(content.Text));
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            referencedTemplates.AddRange(InspectCardActions(content.Buttons));
                        }
                    }
                }
                return referencedTemplates;
            }
        }

        private IList<string> InspectCardActions(IList<CardAction> cardActions)
        {
            var referencedTemplates = new List<string>();
            foreach (var action in cardActions)
            {
                referencedTemplates.AddRange(PatternRecognizer.Recognize(action.Text));
                referencedTemplates.AddRange(PatternRecognizer.Recognize(action.Title));
                referencedTemplates.AddRange(PatternRecognizer.Recognize(action.DisplayText));
            }
            return referencedTemplates;
        }
    }
}
