using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// Class to modify/substitute a <see cref="Activity"/> object for template references in <see cref="Activity.Attachments"/>. 
    /// </summary>
    internal class ActivityAttachmentsModifier : IActivityComponentModifier
    {
        /// <summary>
        /// Modify/substitute  a <see cref="Activity"/> object for template references in <see cref="Activity.Attachments"/>. 
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be modified.</param>
        /// <param name="response">The <see cref="ICompositeResponse"/> object that carries the tempolate resolution values, which will be used to modify the activity.</param>
        public void Modify(Activity activity, ICompositeResponse response)
        {
            if (activity.Attachments != null && activity.Attachments.Count > 0)
            {
                foreach (var attachment in activity.Attachments)
                {
                    if (attachment.ContentType == HeroCard.ContentType)
                    {
                        var content = (attachment.Content) as HeroCard;
                        content.Title = Resolve(content.Title, response);
                        content.Subtitle = Resolve(content.Subtitle, response);
                        content.Text = Resolve(content.Text, response);
                    }
                    else if (attachment.ContentType == ReceiptCard.ContentType)
                    {
                        var content = (attachment.Content) as ReceiptCard;
                        content.Title = Resolve(content.Title, response);

                        var receiptCardItems = content.Items;
                        if (receiptCardItems != null && receiptCardItems.Count > 0)
                        {
                            foreach (var item in receiptCardItems)
                            {
                                item.Text = Resolve(item.Text, response);
                                item.Title = Resolve(item.Title, response);
                                item.Subtitle = Resolve(item.Subtitle, response);
                            }
                        }
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            ModifyCardActions(content.Buttons, response);
                        }
                    }
                    else if (attachment.ContentType == SigninCard.ContentType)
                    {
                        var content = (attachment.Content) as SigninCard;
                        content.Text = Resolve(content.Text, response);
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            ModifyCardActions(content.Buttons, response);
                        }
                    }
                    else if (attachment.ContentType == AnimationCard.ContentType)
                    {
                        var content = (attachment.Content) as AnimationCard;
                        content.Title = Resolve(content.Title, response);
                        content.Subtitle = Resolve(content.Subtitle, response);
                        content.Text = Resolve(content.Text, response);
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            ModifyCardActions(content.Buttons, response);
                        }
                    }
                    else if (attachment.ContentType == VideoCard.ContentType)
                    {
                        var content = (attachment.Content) as VideoCard;
                        content.Title = Resolve(content.Title, response);
                        content.Subtitle = Resolve(content.Subtitle, response);
                        content.Text = Resolve(content.Text, response);
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            ModifyCardActions(content.Buttons, response);
                        }
                    }
                    else if (attachment.ContentType == OAuthCard.ContentType)
                    {
                        var content = (attachment.Content) as OAuthCard;
                        content.Text = Resolve(content.Text, response);
                        if (content.Buttons != null && content.Buttons.Count > 0)
                        {
                            ModifyCardActions(content.Buttons, response);
                        }
                    }
                }
            }
        }

        private void ModifyCardActions(IList<CardAction> cardActions, ICompositeResponse response)
        {
            foreach (var action in cardActions)
            {
                action.Text = Resolve(action.Text, response);
                action.Title = Resolve(action.Title, response);
                action.DisplayText = Resolve(action.DisplayText, response);
            }
        }

        private string Resolve(string utrance, ICompositeResponse response)
        {
            if (utrance != null)
            {
                var recognizedPatterns = PatternRecognizer.Recognize(utrance);
                foreach (var pattern in recognizedPatterns)
                {
                    var normalizedMatch = pattern.Substring(1);
                    normalizedMatch = normalizedMatch.Substring(0, normalizedMatch.Length - 1);
                    utrance = utrance.Replace(pattern, response.TemplateResolutions[normalizedMatch]);
                }
            }
            return utrance;
        }
    }
}
