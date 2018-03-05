// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.Cognitive.LUIS;

namespace Microsoft.Bot.Builder.Ai
{
    public class TranslationMiddleware : IReceiveActivity, ISendActivity
    {
        private readonly string[] nativeLanguages;
        private Translator translator;
        private readonly string templatesDir;
        private readonly Func<IBotContext, string> _getUserLanguage;
        private readonly Func<IBotContext, Task<bool>> _setUserLanguage;

        // Constructor for automatic detection of user messages
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey)
        {
            this.nativeLanguages = nativeLanguages;
            this.translator = new Translator(translatorKey);
            templatesDir = "";
        }

        // Constructor for automatic detection of user messages and using templates
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, string templatesDir)
        {
            this.nativeLanguages = nativeLanguages;
            this.translator = new Translator(translatorKey);
            this.templatesDir = templatesDir;
        }

        // Constructor for developer defined detection of user messages and using templates
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, string templatesDir, Func<IBotContext, string> getUserLanguage, Func<IBotContext, Task<bool>> setUserLanguage)
        {
            this.nativeLanguages = nativeLanguages;
            this.translator = new Translator(translatorKey);
            this.templatesDir = templatesDir;
            _getUserLanguage = getUserLanguage;
            _setUserLanguage = setUserLanguage;
        }

        /// <summary>
        /// Incoming activity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>       
        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            IMessageActivity message = context.Request.AsMessageActivity();
            if (message != null)
            {
                if (!String.IsNullOrWhiteSpace(message.Text))
                {
                    // determine the language we are using for this conversation
                    var sourceLanguage = "";
                    if (_getUserLanguage == null)
                        sourceLanguage = await Task.Run(() => translator.Detect(message.Text)); // context.Conversation.Data["Language"]?.ToString() ?? this.nativeLanguages.FirstOrDefault() ?? "en";
                    else
                    {
                        sourceLanguage =  _getUserLanguage(context);
                    }
                    var templatePath = Path.Combine(new string[] { templatesDir, sourceLanguage + ".template" });
                    if (File.Exists(templatePath))
                    {
                        this.translator.SetPostProcessorTemplate(templatePath);
                    }
                    if (!nativeLanguages.Contains(sourceLanguage))
                    {
                        var translationContext = new TranslationContext
                        {
                            SourceText = message.Text,
                            SourceLanguage = sourceLanguage,
                            TargetLanguage = (this.nativeLanguages.Contains(sourceLanguage)) ? sourceLanguage : this.nativeLanguages.FirstOrDefault() ?? "en"
                        };
                        ((BotContext)context)["Microsoft.API.Translation"] = translationContext;

                        // translate to bots language
                        if (translationContext.SourceLanguage != translationContext.TargetLanguage)
                            await TranslateMessageAsync(context, message, translationContext.SourceLanguage, translationContext.TargetLanguage).ConfigureAwait(false);
                    }
                    if (_setUserLanguage != null)
                    {
                        var languageWasChanged = await _setUserLanguage(context);
                        if (!languageWasChanged)
                        {   // if what the user said wasn't a directive to change the locale (or that directive failed), continue the pipeline
                            await next();
                        }
                    }

                }
            }
            await next().ConfigureAwait(false);
        }


        private static readonly Regex UrlRegex = new Regex(@"(https?://[^\s]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Translate .Text field of a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targetLanguage"></param>
        /// <returns></returns>
        private async Task TranslateMessageAsync(IBotContext context, IMessageActivity message, string sourceLanguage, string targetLanguage)
        {
            // if we have text and a target language
            if (!String.IsNullOrWhiteSpace(message.Text) && !String.IsNullOrEmpty(targetLanguage))
            {
                if (targetLanguage == sourceLanguage)
                    return;

                var text = message.Text.Length <= 65536 ? message.Text : message.Text.Substring(0, 65536);

                // massage mentions and urls so they don't get translated
                int i = 0;

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
                foreach (var url in urls)
                {
                    text = text.Replace($"{{{i++}}}", url);
                }

                message.Text = text;
            }
        }

        public async Task SendActivity(IBotContext context, IList<IActivity> activities, MiddlewareSet.NextDelegate next)
        {
            foreach (var activity in activities)
            {
                IMessageActivity message = context.Request.AsMessageActivity();
                if (!String.IsNullOrWhiteSpace(message?.Text))
                {
                    // translate to userslanguage
                    var translationContext = ((BotContext)context)["Microsoft.API.Translation"] as TranslationContext;
                    if (translationContext != null && translationContext.SourceLanguage != translationContext.TargetLanguage && message.Text != translationContext.SourceText)
                        await TranslateMessageAsync(context, message, translationContext.TargetLanguage, translationContext.SourceLanguage).ConfigureAwait(false);
                }
            }
            await next().ConfigureAwait(false);
        }
    }
}
