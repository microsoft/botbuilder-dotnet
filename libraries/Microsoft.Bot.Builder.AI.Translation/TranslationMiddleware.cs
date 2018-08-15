// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Translation.PostProcessor;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.Translation
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
        private readonly IStatePropertyAccessor<string> _languageStateProperty;
        private readonly bool _toUserLanguage;
        private List<IPostProcessor> attachedPostProcessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMiddleware"/> class.
        /// </summary>
        /// <param name="nativeLanguages">The languages supported by your app.</param>
        /// <param name="translatorKey">Your subscription key for the Microsoft Translator Text API.</param>
        /// <param name="toUserLanguage">Indicates whether to translate messages sent from the bot into the user's language.</param>
        /// <param name="httpClient">An alternate HTTP client to use.</param>
        /// <param name="defaultLocale">Default locale to use when underlying user locale is undefined.</param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, bool toUserLanguage = false, HttpClient httpClient = null, string defaultLocale = "en")
        {
            if (string.IsNullOrWhiteSpace(defaultLocale))
            {
                throw new ArgumentNullException(nameof(defaultLocale));
            }

            AssertValidNativeLanguages(nativeLanguages);
            this._nativeLanguages = nativeLanguages;
            if (string.IsNullOrEmpty(translatorKey))
            {
                throw new ArgumentNullException(nameof(translatorKey));
            }

            this._translator = new Translator(translatorKey, httpClient);
            _patterns = new Dictionary<string, List<string>>();
            _userCustomDictonaries = new CustomDictionary();
            _toUserLanguage = toUserLanguage;
            DefaultLocale = defaultLocale;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMiddleware"/> class.
        /// </summary>
        /// <param name="nativeLanguages">The languages supported by your app.</param>
        /// <param name="translatorKey">Your subscription key for the Microsoft Translator Text API.</param>
        /// <param name="patterns">List of regex patterns, indexed by language identifier,
        /// that can be used to flag text that should not be translated.</param>
        /// /// <param name="userCustomDictonaries">Custom languages dictionary object, used to store all the different languages dictionaries
        /// configured by the user to overwrite the translator output to certain vocab by the custom dictionary translation.</param>
        /// <param name="toUserLanguage">Indicates whether to translate messages sent from the bot into the user's language.</param>
        /// <param name="defaultLocale">Default locale to use when underlying user locale is undefined.</param>
        /// <remarks>Each pattern the <paramref name="patterns"/> describes an entity that should not be translated.
        /// For example, in French <c>je m’appelle ([a-z]+)</c>, which will avoid translation of anything coming after je m’appelle.</remarks>
        /// <param name="httpClient">An alternate HTTP client to use.</param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, Dictionary<string, List<string>> patterns, CustomDictionary userCustomDictonaries, bool toUserLanguage = false, HttpClient httpClient = null, string defaultLocale = "en")
            : this(nativeLanguages, translatorKey, toUserLanguage, httpClient, defaultLocale)
        {
            if (patterns != null)
            {
                this._patterns = patterns;
            }

            if (userCustomDictonaries != null)
            {
                this._userCustomDictonaries = userCustomDictonaries;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMiddleware"/> class.
        /// </summary>
        /// <param name="nativeLanguages">List of languages supported by your app.</param>
        /// <param name="translatorKey">Your subscription key for the Microsoft Translator Text API.</param>
        /// <param name="patterns">List of regex patterns, indexed by language identifier,
        /// that can be used to flag text that should not be translated.</param>
        /// <param name="userCustomDictonaries">Custom languages dictionary object, used to store all the different languages dictionaries
        /// configured by the user to overwrite the translator output to certain vocab by the custom dictionary translation.</param>
        /// <param name="languageStateProperty">A Property Accessor for for getting the users active language.</param>
        /// <param name="toUserLanguage">Indicates whether to translate messages sent from the bot into the user's language.</param>
        /// <param name="defaultLocale">Default locale to use when underlying user locale is undefined.</param>
        /// <remarks>Each pattern the <paramref name="patterns"/> describes an entity that should not be translated.
        /// For example, in French <c>je m’appelle ([a-z]+)</c>, which will avoid translation of anything coming after je m’appelle.</remarks>
        /// <param name="httpClient">An alternate HTTP client to use.</param>
        public TranslationMiddleware(string[] nativeLanguages, string translatorKey, Dictionary<string, List<string>> patterns, CustomDictionary userCustomDictonaries, IStatePropertyAccessor<string> languageStateProperty, bool toUserLanguage = false, HttpClient httpClient = null, string defaultLocale = "en")
            : this(nativeLanguages, translatorKey, patterns, userCustomDictonaries, toUserLanguage, httpClient, defaultLocale)
        {
            _languageStateProperty = languageStateProperty ?? throw new ArgumentNullException(nameof(languageStateProperty));
        }

        /// <summary>
        /// Gets the default locale to use when underlying user locale is undefined.
        /// </summary>
        /// <value>The default locale that will be used when the underlying user locale is undefined.</value>
        public virtual string DefaultLocale { get; }

        /// <summary>
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                IMessageActivity message = context.Activity.AsMessageActivity();
                if (message != null)
                {
                    if (!string.IsNullOrWhiteSpace(message.Text))
                    {
                        // determine the language we are using for this conversation
                        var sourceLanguage = string.Empty;
                        var targetLanguage = string.Empty;
                        if (_languageStateProperty == null)
                        {
                            sourceLanguage = await _translator.DetectAsync(message.Text).ConfigureAwait(false); // awaiting user language detection using Microsoft Translator API.
                        }
                        else
                        {
                            sourceLanguage = await _languageStateProperty.GetAsync(context, () => DefaultLocale).ConfigureAwait(false) ?? "en";
                        }

                        targetLanguage = _nativeLanguages.Contains(sourceLanguage) ? sourceLanguage : _nativeLanguages.FirstOrDefault() ?? "en";
                        await TranslateMessageAsync(context, message, sourceLanguage, targetLanguage, _nativeLanguages.Contains(sourceLanguage)).ConfigureAwait(false);

                        context.OnSendActivities(async (newContext, activities, nextSend) =>
                        {
                            // Translate messages sent to the user to user language
                            if (_toUserLanguage)
                            {
                                List<Task> tasks = new List<Task>();
                                foreach (Activity currentActivity in activities.Where(a => a.Type == ActivityTypes.Message))
                                {
                                    tasks.Add(TranslateMessageAsync(newContext, currentActivity.AsMessageActivity(), targetLanguage, sourceLanguage, false));
                                }

                                if (tasks.Any())
                                {
                                    await Task.WhenAll(tasks).ConfigureAwait(false);
                                }
                            }

                            return await nextSend().ConfigureAwait(false);
                        });

                        context.OnUpdateActivity(async (newContext, activity, nextUpdate) =>
                        {
                            // Translate messages sent to the user to user language
                            if (activity.Type == ActivityTypes.Message)
                            {
                                if (_toUserLanguage)
                                {
                                    await TranslateMessageAsync(newContext, activity.AsMessageActivity(), targetLanguage, sourceLanguage, false).ConfigureAwait(false);
                                }
                            }

                            return await nextUpdate().ConfigureAwait(false);
                        });
                    }
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        private static void AssertValidNativeLanguages(string[] nativeLanguages)
        {
            if (nativeLanguages == null)
            {
                throw new ArgumentNullException(nameof(nativeLanguages));
            }
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
        /// <param name="translatedDocuments">List of <see cref="TranslatedDocument"/> represent the output of the translator module.</param>
        /// <param name="languageId">Current language id.</param>
        private void PostProcesseDocuments(List<TranslatedDocument> translatedDocuments, string languageId)
        {
            if (attachedPostProcessors == null)
            {
                InitializePostProcessors();
            }

            foreach (var translatedDocument in translatedDocuments)
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
        /// <param name="inNativeLanguages">should only use native langauges.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>When the task completes successfully, the <see cref="Activity.Text"/> property
        /// of the message contains the translated text.</remarks>
        private async Task TranslateMessageAsync(ITurnContext context, IMessageActivity message, string sourceLanguage, string targetLanguage, bool inNativeLanguages)
        {
            if (!inNativeLanguages && sourceLanguage != targetLanguage)
            {
                // if we have text and a target language
                if (!string.IsNullOrWhiteSpace(message.Text) && !string.IsNullOrEmpty(targetLanguage))
                {
                    if (targetLanguage == sourceLanguage)
                    {
                        return;
                    }

                    var text = message.Text;
                    string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var translateResult = await this._translator.TranslateArrayAsync(lines, sourceLanguage, targetLanguage).ConfigureAwait(false);

                    // post process all translated documents
                    PostProcesseDocuments(translateResult, sourceLanguage);
                    text = string.Empty;
                    foreach (var translatedDocument in translateResult)
                    {
                        text += string.Join("\n", translatedDocument.TargetMessage);
                    }

                    message.Text = text;
                }
            }
        }
    }
}
