// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai
{
    public class TranslationMiddleware : IMiddleware
    {
        private readonly string[] _nativeLanguages;
        private readonly Translator _translator;
        private readonly Dictionary<string,List<string>> _patterns;
        private readonly Func<IBotContext, string> _getUserLanguage;
        private readonly Func<IBotContext, Task<bool>> _setUserLanguage;

        /// <summary>
        /// Constructor for automatic detection of user messages
        /// </summary>
        /// <param name="nativeLanguages">List of languages supported by your app</param>
        /// <param name="translatorKey"></param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey)
        {
            this._nativeLanguages = nativeLanguages;
            this._translator = new Translator(translatorKey);
            _patterns = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Constructor for automatic of user messages and using templates
        /// </summary>
        /// <param name="nativeLanguages">List of languages supported by your app</param>
        /// <param name="translatorKey"></param>
        /// <param name="patterns">Dictionary with language as a key and list of patterns as value</param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, Dictionary<string, List<string>> patterns)
        {
            this._nativeLanguages = nativeLanguages;
            this._translator = new Translator(translatorKey);
            this._patterns = patterns;
        }

        /// <summary>
        /// Constructor for developer defined detection of user messages and using templates
        /// </summary>
        /// <param name="nativeLanguages">List of languages supported by your app</param>
        /// <param name="translatorKey"></param>
        /// <param name="patterns">Dictionary with language as a key and list of patterns as value</param>
        /// <param name="getUserLanguage">Delegate for getting the user language</param>
        /// <param name="setUserLanguage">Delegate for setting the user language, returns true if the language was changed (implements logic to change language by intercepting the message)</param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, Dictionary<string, List<string>> patterns, Func<IBotContext, string> getUserLanguage, Func<IBotContext, Task<bool>> setUserLanguage)
        {
            this._nativeLanguages = nativeLanguages;
            this._translator = new Translator(translatorKey);
            this._patterns = patterns;
            _getUserLanguage = getUserLanguage;
            _setUserLanguage = setUserLanguage;
        }

        /// <summary>
        /// Incoming activity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>       
        public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            IMessageActivity message = context.Request.AsMessageActivity();
            if (message != null)
            {
                if (!String.IsNullOrWhiteSpace(message.Text))
                {

                    var languageChanged = false;

                    if (_setUserLanguage != null)
                    {
                        languageChanged = await _setUserLanguage(context);
                    }

                    if (!languageChanged)
                    {
                        // determine the language we are using for this conversation
                        var sourceLanguage = "";
                        if (_getUserLanguage == null)
                            sourceLanguage = await Task.Run(() => _translator.Detect(message.Text)); // context.Conversation.Data["Language"]?.ToString() ?? this._nativeLanguages.FirstOrDefault() ?? "en";
                        else
                        {
                            sourceLanguage = _getUserLanguage(context);
                        } 
                        if (_patterns.ContainsKey(sourceLanguage) && _patterns[sourceLanguage].Count>0)
                        {
                            this._translator.SetPostProcessorTemplate(_patterns[sourceLanguage]);
                        }
                        if (!_nativeLanguages.Contains(sourceLanguage))
                        {
                            var translationContext = new TranslationContext
                            {
                                SourceText = message.Text,
                                SourceLanguage = sourceLanguage,
                                TargetLanguage = (this._nativeLanguages.Contains(sourceLanguage)) ? sourceLanguage : this._nativeLanguages.FirstOrDefault() ?? "en"
                            };
                            ((BotContext)context).Set("Microsoft.API.Translation", translationContext);

                            // translate to bots language
                            if (translationContext.SourceLanguage != translationContext.TargetLanguage)
                                await TranslateMessageAsync(context, message, translationContext.SourceLanguage, translationContext.TargetLanguage).ConfigureAwait(false);
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
        /// <param name="context"/>
        /// <param name="message"></param>
        /// <param name="sourceLanguage"/>
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
                var translateResult = await this._translator.TranslateArray(lines, sourceLanguage, targetLanguage).ConfigureAwait(false);
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
    }
}