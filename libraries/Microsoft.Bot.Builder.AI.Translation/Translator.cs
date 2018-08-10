// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Translation.Model;
using Microsoft.Bot.Builder.AI.Translation.PostProcessor;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Translation
{
    /// <summary>
    /// Provides access to the Microsoft Translator Text API.
    /// Uses api key and detect input language translate single sentence or array of sentences then apply translation post processing fix.
    /// </summary>
    public class Translator
    {
        private const string DetectUrl = "https://api.cognitive.microsofttranslator.com/detect?api-version=3.0";
        private const string TranslateUrl = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&includeAlignment=true&includeSentenceLength=true";

        private static readonly HttpClient DefaultHttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) };
        private readonly string _apiKey;
        private HttpClient _httpClient = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Translator"/> class.
        /// </summary>
        /// <param name="apiKey">Your subscription key for the Microsoft Translator Text API.</param>
        /// <param name="httpClient">An alternate HTTP client to use.</param>
        public Translator(string apiKey, HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? DefaultHttpClient;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey));
            }

            _apiKey = apiKey;
        }

        /// <summary>
        /// Detects the language of the input text.
        /// </summary>
        /// <param name="textToDetect">The text to translate.</param>
        /// <returns>The language identifier.</returns>
        public async Task<string> DetectAsync(string textToDetect)
        {
            textToDetect = PreprocessMessage(textToDetect);

            var payload = new TranslatorRequestModel[] { new TranslatorRequestModel { Text = textToDetect } };

            using (var request = GetDetectRequestMessage(payload))
            {
                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        var detectedLanguages = JsonConvert.DeserializeObject<IEnumerable<DetectedLanguageModel>>(result);
                        var detectedLang = detectedLanguages.First().Language;
                        return detectedLang;
                    }
                    else
                    {
                        var errorResult = JsonConvert.DeserializeObject<ErrorModel>(result);
                        throw new ArgumentException(errorResult.Error.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Translates a single message from a source language to a target language.
        /// </summary>
        /// <param name="textToTranslate">The text to translate.</param>
        /// <param name="from">The language code of the translation text. For example, "en" for English.</param>
        /// <param name="to">The language code to translate the text into.</param>
        /// <returns>The translated document.</returns>
        public async Task<TranslatedDocument> TranslateAsync(string textToTranslate, string from, string to)
        {
            var results = await TranslateArrayAsync(new string[] { textToTranslate }, from, to).ConfigureAwait(false);
            return results.First();
        }

        /// <summary>
        /// Translates an array of strings from a source language to a target language.
        /// </summary>
        /// <param name="translateArraySourceTexts">The strings to translate.</param>
        /// <param name="from">The language code of the translation text. For example, "en" for English.</param>
        /// <param name="to">The language code to translate the text into.</param>
        /// <returns>An array of the translated documents.</returns>
        public async Task<List<TranslatedDocument>> TranslateArrayAsync(string[] translateArraySourceTexts, string from, string to)
        {
            var translatedDocuments = new List<TranslatedDocument>();
            for (var srcTxtIndx = 0; srcTxtIndx < translateArraySourceTexts.Length; srcTxtIndx++)
            {
                // Check for literal tag in input user message
                var currentTranslatedDocument = new TranslatedDocument(translateArraySourceTexts[srcTxtIndx]);
                translatedDocuments.Add(currentTranslatedDocument);
                PreprocessMessage(currentTranslatedDocument.SourceMessage, out var processedText, out var literanlNoTranslateList);
                currentTranslatedDocument.SourceMessage = processedText;
                translateArraySourceTexts[srcTxtIndx] = processedText;
                currentTranslatedDocument.LiteranlNoTranslatePhrases = literanlNoTranslateList;
            }

            // list of translation request for the service
            var payload = translateArraySourceTexts.Select(s => new TranslatorRequestModel { Text = s });

            using (var request = GetTranslateRequestMessage(from, to, payload))
            {
                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var translatedResults = JsonConvert.DeserializeObject<IEnumerable<TranslatedResult>>(responseBody);

                        var sentIndex = 0;
                        foreach (var translatedValue in translatedResults)
                        {
                            var translation = translatedValue.Translations.First();
                            var currentTranslatedDocument = translatedDocuments[sentIndex];
                            currentTranslatedDocument.RawAlignment = translation.Alignment?.Projection ?? null;
                            currentTranslatedDocument.TargetMessage = translation.Text;

                            if (!string.IsNullOrEmpty(currentTranslatedDocument.RawAlignment))
                            {
                                var alignments = currentTranslatedDocument.RawAlignment.Trim().Split(' ');
                                currentTranslatedDocument.SourceTokens = PostProcessingUtilities.SplitSentence(currentTranslatedDocument.SourceMessage, alignments);
                                currentTranslatedDocument.TranslatedTokens = PostProcessingUtilities.SplitSentence(translation.Text, alignments, false);
                                currentTranslatedDocument.IndexedAlignment = PostProcessingUtilities.WordAlignmentParse(alignments, currentTranslatedDocument.SourceTokens, currentTranslatedDocument.TranslatedTokens);
                                currentTranslatedDocument.TargetMessage = PostProcessingUtilities.Join(" ", currentTranslatedDocument.TranslatedTokens);
                            }
                            else
                            {
                                var translatedText = translation.Text;
                                currentTranslatedDocument.TargetMessage = translatedText;
                                currentTranslatedDocument.SourceTokens = new string[] { currentTranslatedDocument.SourceMessage };
                                currentTranslatedDocument.TranslatedTokens = new string[] { currentTranslatedDocument.TargetMessage };
                                currentTranslatedDocument.IndexedAlignment = new Dictionary<int, int>();
                            }

                            sentIndex++;
                        }

                        return translatedDocuments;
                    }
                    else
                    {
                        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var errorResult = JsonConvert.DeserializeObject<ErrorModel>(responseBody);
                        throw new ArgumentException(errorResult.Error.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Performs pre-processing to remove "literal" tags and flag sections of the text that will not be translated.
        /// </summary>
        /// <param name="textToTranslate">The text to translate.</param>
        /// <param name="processedTextToTranslate">The processed text after removing the literal tags and other unwanted characters.</param>
        /// <param name="noTranslatePhrases">The extracted no translate phrases.</param>
        private void PreprocessMessage(string textToTranslate, out string processedTextToTranslate, out HashSet<string> noTranslatePhrases)
        {
            textToTranslate = Regex.Replace(textToTranslate, @"\s+", " "); // used to remove multiple spaces in input user message
            var literalPattern = "<literal>(.*)</literal>";
            noTranslatePhrases = new HashSet<string>();
            var literalMatches = Regex.Matches(textToTranslate, literalPattern);
            if (literalMatches.Count > 0)
            {
                foreach (Match literalMatch in literalMatches)
                {
                    if (literalMatch.Groups.Count > 1)
                    {
                        noTranslatePhrases.Add("(" + literalMatch.Groups[1].Value + ")");
                    }
                }

                textToTranslate = Regex.Replace(textToTranslate, "</?literal>", " ");
            }

            textToTranslate = Regex.Replace(textToTranslate, @"\s+", " ");
            processedTextToTranslate = textToTranslate;
        }

        /// <summary>
        /// Performs pre-processing to remove "literal" tags .
        /// </summary>
        /// <param name="textToTranslate">The text to translate.</param>
        private string PreprocessMessage(string textToTranslate)
        {
            textToTranslate = Regex.Replace(textToTranslate, @"\s+", " "); // used to remove multiple spaces in input user message
            var literalPattern = "<literal>(.*)</literal>";
            var literalMatches = Regex.Matches(textToTranslate, literalPattern);
            if (literalMatches.Count > 0)
            {
                textToTranslate = Regex.Replace(textToTranslate, "</?literal>", " ");
            }

            return Regex.Replace(textToTranslate, @"\s+", " ");
        }

        private HttpRequestMessage GetTranslateRequestMessage(string from, string to, IEnumerable<TranslatorRequestModel> translatorRequests)
        {
            var query = $"&from={from}&to={to}";
            var requestUri = new Uri(TranslateUrl + query);
            return GetRequestMessage(requestUri, translatorRequests);
        }

        private HttpRequestMessage GetDetectRequestMessage(IEnumerable<TranslatorRequestModel> translatorRequests)
        {
            var requestUri = new Uri(DetectUrl);
            return GetRequestMessage(requestUri, translatorRequests);
        }

        private HttpRequestMessage GetRequestMessage(Uri requestUri, IEnumerable<TranslatorRequestModel> translatorRequests)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);
            request.Content = new StringContent(JsonConvert.SerializeObject(translatorRequests), Encoding.UTF8, "application/json");
            return request;
        }
    }
}
