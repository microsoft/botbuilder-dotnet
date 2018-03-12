// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Note: Commenting out until integration with revised LUIS Recognizer is completed. 

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using Microsoft.Bot.Builder.Middleware;
//using Microsoft.Bot.Schema;
//using Microsoft.Cognitive.LUIS;

//namespace Microsoft.Bot.Builder.Ai
//{
//    public class TranslationMiddleware : IMiddleware
//    {
//        private LuisClient luisClient;
//        private string[] nativeLanguages;
//        private Translator translator;        

//        public TranslationMiddleware(HttpClient httpClient, string[] nativeLanguages, string translatorKey, string luisAppId, string luisAccessKey)
//        {
//            this.nativeLanguages = nativeLanguages;
//            this.luisClient = new LuisClient(luisAppId, luisAccessKey);
//            this.translator = new Translator(translatorKey, httpClient);
//        }

//        /// <summary>
//        /// Incoming activity
//        /// </summary>
//        /// <param name="context"></param>
//        /// <param name="token"></param>
//        /// <returns></returns>       
//        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
//        {
//            IMessageActivity message = context.Request.AsMessageActivity();
//            if (message != null)
//            {
//                if (!String.IsNullOrWhiteSpace(message.Text))
//                {
//                    // determine the language we are using for this conversation
//                    var sourceLanguage = "en"; // context.Conversation.Data["Language"]?.ToString() ?? this.nativeLanguages.FirstOrDefault() ?? "en";

//                    var translationContext = new TranslationContext
//                    {
//                        SourceText = message.Text,
//                        SourceLanguage = sourceLanguage,
//                        TargetLanguage = (this.nativeLanguages.Contains(sourceLanguage)) ? sourceLanguage : this.nativeLanguages.FirstOrDefault() ?? "en"
//                    };
//                    context.Set(translationContext);

//                    // translate to bots language
//                    if (translationContext.SourceLanguage != translationContext.TargetLanguage)
//                        await TranslateMessageAsync(context, message, translationContext.SourceLanguage, translationContext.TargetLanguage).ConfigureAwait(false);

//                }
//            }
//            await next().ConfigureAwait(false);
//        }

//        /// <summary>
//        /// outgoing activities
//        /// </summary>
//        /// <param name="context"></param>
//        /// <param name="activities"></param>
//        /// <param name="token"></param>
//        /// <returns></returns>
//        public async Task SendActivity(IBotContext context, IList<Activity> activities, MiddlewareSet.NextDelegate next)
//        {
//            foreach (var activity in activities)
//            {
//                IMessageActivity message = context.Request.AsMessageActivity();
//                if (!String.IsNullOrWhiteSpace(message?.Text))
//                {
//                    // translate to userslanguage
//                    var translationContext = context.Get<TranslationContext>();
//                    if (translationContext.SourceLanguage != translationContext.TargetLanguage)
//                        await TranslateMessageAsync(context, message, translationContext.TargetLanguage, translationContext.SourceLanguage).ConfigureAwait(false);
//                }
//            }
//            await next().ConfigureAwait(false);
//        }
        
//        private static readonly Regex UrlRegex = new Regex(@"(https?://[^\s]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

//        /// <summary>
//        /// Translate .Text field of a message
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="targetLanguage"></param>
//        /// <returns></returns>
//        private async Task TranslateMessageAsync(IBotContext context, IMessageActivity message, string sourceLanguage, string targetLanguage)
//        {
//            // if we have text and a target language
//            if (!String.IsNullOrWhiteSpace(message.Text) && !String.IsNullOrEmpty(targetLanguage))
//            {
//                if (targetLanguage == sourceLanguage)
//                    return;

//                var text = message.Text.Length <= 65536 ? message.Text : message.Text.Substring(0, 65536);

//                // massage mentions and urls so they don't get translated
//                int i = 0;
//                //foreach (var mention in message.Mentions)
//                //{
//                //    text = text.Replace(mention.Text, $"{{{i++}}}");
//                //}

//                var urls = new List<string>();
//                while (UrlRegex.IsMatch(text))
//                {
//                    var match = UrlRegex.Match(text);
//                    urls.Add(match.Groups[0].Value);
//                    text = text.Replace(match.Groups[0].Value, $"{{{i++}}}");
//                }

//                string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
//                var translateResult = await this.translator.TranslateArray(lines, sourceLanguage, targetLanguage).ConfigureAwait(false);
//                text = String.Join("\n", translateResult);

//                // restore mentions and urls 
//                i = 0;
//                //foreach (var mention in message.Mentions)
//                //{
//                //    text = text.Replace($"{{{i++}}}", mention.Text);
//                //}
//                foreach (var url in urls)
//                {
//                    text = text.Replace($"{{{i++}}}", url);
//                }

//                message.Text = text;
//            }
//        }        
//    }
//}
