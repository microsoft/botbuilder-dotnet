// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.Translation.PostProcessor;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// Middleware for translating text between the user and bot.
    /// Uses the Microsoft Translator Text API.
    /// </summary>
    public class TranslationMiddleware : IMiddleware
    {
        private readonly string[] _nativeLanguages;
        private readonly Translator _translator;
        private readonly CustomDictionary _userCustomDictonaries;
        private readonly Dictionary<string, List<string>> _patterns;
        private readonly Func<ITurnContext, string> _getUserLanguage;
        private readonly Func<ITurnContext, Task<bool>> _isUserLanguageChanged;
        private readonly bool _toUserLanguage;
        private List<IPostProcessor> attachedPostProcessors;

        /// <summary>
        /// Creates a new <see cref="TranslationMiddleware"/> object.
        /// </summary>
        /// <param name="nativeLanguages">The languages supported by your app.</param>
        /// <param name="translatorKey">Your subscription key for the Microsoft Translator Text API.</param>
        /// <param name="toUserLanguage">Indicates whether to translate messages sent from the bot into the user's language.</param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, bool toUserLanguage = false)
        {
            AssertValidNativeLanguages(nativeLanguages);
            this._nativeLanguages = nativeLanguages;
            if (string.IsNullOrEmpty(translatorKey))
                throw new ArgumentNullException(nameof(translatorKey));
            this._translator = new Translator(translatorKey);
            _patterns = new Dictionary<string, List<string>>();
            _userCustomDictonaries = new CustomDictionary();
            _toUserLanguage = toUserLanguage;
        }


        /// <summary>
        /// Creates a new <see cref="TranslationMiddleware"/> object.
        /// </summary>
        /// <param name="nativeLanguages">The languages supported by your app.</param>
        /// <param name="translatorKey">Your subscription key for the Microsoft Translator Text API.</param>
        /// <param name="patterns">List of regex patterns, indexed by language identifier, 
        /// that can be used to flag text that should not be translated.</param>
        /// /// <param name="userCustomDictonaries">Custom languages dictionary object, used to store all the different languages dictionaries
        /// configured by the user to overwrite the translator output to certain vocab by the custom dictionary translation.</param>
        /// <param name="toUserLanguage">Indicates whether to translate messages sent from the bot into the user's language.</param>
        /// <remarks>Each pattern the <paramref name="patterns"/> describes an entity that should not be translated.
        /// For example, in French <c>je m’appelle ([a-z]+)</c>, which will avoid translation of anything coming after je m’appelle.</remarks>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, Dictionary<string, List<string>> patterns, CustomDictionary userCustomDictonaries, bool toUserLanguage = false) : this(nativeLanguages, translatorKey, toUserLanguage)
        {
            if (patterns != null)
                this._patterns = patterns;
            if (userCustomDictonaries != null)
                this._userCustomDictonaries = userCustomDictonaries;
        }

        /// <summary>
        /// Creates a new <see cref="TranslationMiddleware"/> object.
        /// </summary>
        /// <param name="nativeLanguages">List of languages supported by your app</param>
        /// <param name="translatorKey">Your subscription key for the Microsoft Translator Text API.</param>
        /// <param name="patterns">List of regex patterns, indexed by language identifier, 
        /// that can be used to flag text that should not be translated.</param>
        /// <param name="userCustomDictonaries">Custom languages dictionary object, used to store all the different languages dictionaries
        /// configured by the user to overwrite the translator output to certain vocab by the custom dictionary translation.</param>
        /// <param name="getUserLanguage">A delegate for getting the user language, 
        /// to use in place of the Detect method of the Microsoft Translator Text API.</param>
        /// <param name="isUserLanguageChanged">A delegate for checking whether the user requested to change their language.</param>
        /// <param name="toUserLanguage">Indicates whether to translate messages sent from the bot into the user's language.</param>
        /// <remarks>Each pattern the <paramref name="patterns"/> describes an entity that should not be translated.
        /// For example, in French <c>je m’appelle ([a-z]+)</c>, which will avoid translation of anything coming after je m’appelle.</remarks>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, Dictionary<string, List<string>> patterns, CustomDictionary userCustomDictonaries, Func<ITurnContext, string> getUserLanguage, Func<ITurnContext, Task<bool>> isUserLanguageChanged, bool toUserLanguage = false) : this(nativeLanguages, translatorKey, patterns, userCustomDictonaries, toUserLanguage)
        {
            this._getUserLanguage = getUserLanguage ?? throw new ArgumentNullException(nameof(getUserLanguage));
            this._isUserLanguageChanged = isUserLanguageChanged ?? throw new ArgumentNullException(nameof(isUserLanguageChanged));
        }

        private static void AssertValidNativeLanguages(string[] nativeLanguages)
        {
            if (nativeLanguages == null)
                throw new ArgumentNullException(nameof(nativeLanguages));
        }

        /// <summary>
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        public virtual async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                IMessageActivity message = context.Activity.AsMessageActivity();
                if (message != null)
                {
                    if (!String.IsNullOrWhiteSpace(message.Text))
                    {

                        var languageChanged = false;

                        if (_isUserLanguageChanged != null)
                        {
                            languageChanged = await _isUserLanguageChanged(context);
                        }

                        if (!languageChanged)
                        {
                            // determine the language we are using for this conversation
                            var sourceLanguage = "";
                            var targetLanguage = "";
                            if (_getUserLanguage == null)
                                sourceLanguage = await _translator.Detect(message.Text); //awaiting user language detection using Microsoft Translator API.
                            else
                            {
                                sourceLanguage = _getUserLanguage(context);
                            }

                            targetLanguage = (_nativeLanguages.Contains(sourceLanguage)) ? sourceLanguage : this._nativeLanguages.FirstOrDefault() ?? "en";
                            await TranslateMessageAsync(context, message, sourceLanguage, targetLanguage, _nativeLanguages.Contains(sourceLanguage)).ConfigureAwait(false);

                            if (_toUserLanguage)
                            {
                                context.OnSendActivities(async (newContext, activities, nextSend) =>
                                {
                                    //Translate messages sent to the user to user language
                                    List<Task> tasks = new List<Task>();
                                    foreach (Activity currentActivity in activities.Where(a => a.Type == ActivityTypes.Message))
                                    {
                                        tasks.Add(TranslateMessageAsync(newContext, currentActivity.AsMessageActivity(), targetLanguage, sourceLanguage, false));
                                    }
                                    if (tasks.Any())
                                        await Task.WhenAll(tasks).ConfigureAwait(false);

                                    return await nextSend();
                                });

                                context.OnUpdateActivity(async (newContext, activity, nextUpdate) =>
                                {
                                    //Translate messages sent to the user to user language
                                    if (activity.Type == ActivityTypes.Message)
                                    {
                                        await TranslateMessageAsync(newContext, activity.AsMessageActivity(), targetLanguage, sourceLanguage, false).ConfigureAwait(false);
                                    }

                                    return await nextUpdate();
                                });
                            }
                        }
                        else
                        {
                            // skip routing in case of user changed the language
                            return;

                        }
                    }
                }
            }
            await next().ConfigureAwait(false);
        }

        /// <summary>
        /// Initialize attached post processors according to what the user sent in the middle ware constructor.
        /// </summary>
        private void InitializePostProcessors()
        {
            attachedPostProcessors = new List<IPostProcessor>();
            if (_patterns != null && _patterns.Count > 0)
            {
                attachedPostProcessors.Add(new PatternsPostProcessor(_patterns));
            }
            if (_userCustomDictonaries != null && !_userCustomDictonaries.IsEmpty())
            {
                attachedPostProcessors.Add(new CustomDictionaryPostProcessor(_userCustomDictonaries));
            }
        }

        /// <summary>
        /// Applies all the attached post processors to the translated messages.
        /// </summary>
        /// <param name="translatedDocuments">List of <see cref="TranslatedDocument"/> represent the output of the translator module</param>
        /// <param name="languageId">Current language id</param>
        private void PostProcesseDocuments(List<TranslatedDocument> translatedDocuments, string languageId)
        {
            if (attachedPostProcessors == null)
            {
                InitializePostProcessors();
            }
            foreach (TranslatedDocument translatedDocument in translatedDocuments)
            {
                foreach (IPostProcessor postProcessor in attachedPostProcessors)
                {
                    translatedDocument.TargetMessage = postProcessor.Process(translatedDocument, languageId).PostProcessedMessage;
                }
            }
        }

        /// <summary>
        /// Translates the <see cref="Activity.Text"/> of a message.
        /// </summary>
        /// <param name="context">The current turn context.</param>
        /// <param name="message">The activity containing the text to translate.</param>
        /// <param name="sourceLanguage">An identifier for the language to translate from.</param>
        /// <param name="targetLanguage">An identifier for the language to translate to.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>When the task completes successfully, the <see cref="Activity.Text"/> property
        /// of the message contains the translated text.</remarks>
        private async Task TranslateMessageAsync(ITurnContext context, IMessageActivity message, string sourceLanguage, string targetLanguage, bool InNativeLanguages)
        {
            if (!InNativeLanguages && sourceLanguage != targetLanguage)
            {
                // if we have text and a target language
                if (!String.IsNullOrWhiteSpace(message.Text) && !String.IsNullOrEmpty(targetLanguage))
                {
                    if (targetLanguage == sourceLanguage)
                        return;

                    var text = message.Text;
                    string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var translateResult = await this._translator.TranslateArray(lines, sourceLanguage, targetLanguage).ConfigureAwait(false);

                    // post process all translated documents
                    PostProcesseDocuments(translateResult, sourceLanguage);
                    text = string.Empty;
                    foreach (TranslatedDocument translatedDocument in translateResult)
                    {
                        text += string.Join("\n", translatedDocument.TargetMessage);
                    }
                    message.Text = text;
                }
            }
        }
    }
}
