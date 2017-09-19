using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Ai
{
    public class TranslationMiddleware : IMiddleware, IReceiveActivity, IPostActivity
    {
        private LuisClient luisClient;
        private string[] nativeLanguages;
        private Translator translator;

        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, string luisAppId, string luisAccessKey)
        {
            this.nativeLanguages = nativeLanguages;
            this.luisClient = new LuisClient(luisAppId, luisAccessKey);
            this.translator = new Translator(translatorKey);
        }

        /// <summary>
        /// Incoming activity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            IMessageActivity message = context.Request.AsMessageActivity();
            if (message != null)
            {
                if (!String.IsNullOrWhiteSpace(message.Text))
                {
                    // determine the language we are using for this conversation
                    var sourceLanguage = "en"; // context.Conversation.Data["Language"]?.ToString() ?? this.nativeLanguages.FirstOrDefault() ?? "en";

                    var translationContext = new TranslationContext();
                    translationContext.SourceText = message.Text;
                    translationContext.SourceLanguage = sourceLanguage;
                    translationContext.TargetLanguage = (this.nativeLanguages.Contains(sourceLanguage)) ? sourceLanguage : this.nativeLanguages.FirstOrDefault() ?? "en";
                    context["Translation"] = translationContext;

                    // translate to bots language
                    if (translationContext.SourceLanguage != translationContext.TargetLanguage)
                        await TranslateMessageAsync(context, message, translationContext.SourceLanguage, translationContext.TargetLanguage).ConfigureAwait(false);

                }
            }
            return null;
        }


        /// <summary>
        /// outgoing activities
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activities"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task PostActivity(BotContext context, IList<Activity> activities, CancellationToken token)
        {
            foreach (var activity in activities)
            {
                IMessageActivity message = context.Request.AsMessageActivity();
                if (!String.IsNullOrWhiteSpace(message?.Text))
                {
                    // translate to userslanguage
                    var translationContext = context["Translation"] as TranslationContext;
                    if (translationContext.SourceLanguage != translationContext.TargetLanguage)
                        await TranslateMessageAsync(context, message, translationContext.TargetLanguage, translationContext.SourceLanguage).ConfigureAwait(false);
                }
            }
        }

        private static readonly Regex UrlRegex = new Regex(@"(https?://[^\s]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Translate .Text field of a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targetLanguage"></param>
        /// <returns></returns>
        private async Task TranslateMessageAsync(BotContext context, IMessageActivity message, string sourceLanguage, string targetLanguage)
        {
            // if we have text and a target language
            if (!String.IsNullOrWhiteSpace(message.Text) && !String.IsNullOrEmpty(targetLanguage))
            {
                if (targetLanguage == sourceLanguage)
                    return;

                var text = message.Text.Length <= 65536 ? message.Text : message.Text.Substring(0, 65536);

                // massage mentions and urls so they don't get translated
                int i = 0;
                //foreach (var mention in message.Mentions)
                //{
                //    text = text.Replace(mention.Text, $"{{{i++}}}");
                //}

                var urls = new List<string>();
                while (UrlRegex.IsMatch(text))
                {
                    var match = UrlRegex.Match(text);
                    urls.Add(match.Groups[0].Value);
                    text = text.Replace(match.Groups[0].Value, $"{{{i++}}}");
                }

                string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                var translateResult = await this.translator.TranslateArray(lines, sourceLanguage, targetLanguage).ConfigureAwait(false);
                text = String.Join("\n", translateResult);

                // restore mentions and urls 
                i = 0;
                //foreach (var mention in message.Mentions)
                //{
                //    text = text.Replace($"{{{i++}}}", mention.Text);
                //}
                foreach (var url in urls)
                {
                    text = text.Replace($"{{{i++}}}", url);
                }

                message.Text = text;
            }
        }

    }
}
