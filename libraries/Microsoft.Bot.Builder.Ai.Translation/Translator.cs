// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Ai.Translation.PostProcessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// Provides access to the Microsoft Translator Text API.
    /// Uses api key and detect input language translate single sentence or array of sentences then apply translation post processing fix.
    /// </summary>
    public class Translator
    {
        private readonly AzureAuthToken _authToken;

        /// <summary>
        /// Creates a new <see cref="Translator"/> object.
        /// </summary>
        /// <param name="apiKey">Your subscription key for the Microsoft Translator Text API.</param>
        public Translator(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey));
            }
            _authToken = new AzureAuthToken(apiKey);
        }

        /// <summary>
        /// Performs pre-processing to remove "literal" tags and flag sections of the text that will not be translated.
        /// </summary>
        /// <param name="textToTranslate">The text to translate</param>
        /// <param name="processedTextToTranslate">The processed text after removing the literal tags and other unwanted characters</param>
        /// <param name="noTranslatePhrases">The extracted no translate phrases</param>
        private void PreprocessMessage(string textToTranslate, out string processedTextToTranslate, out HashSet<string> noTranslatePhrases)
        {
            textToTranslate = Regex.Replace(textToTranslate, @"\s+", " ");//used to remove multiple spaces in input user message
            string literalPattern = "<literal>(.*)</literal>";
            noTranslatePhrases = new HashSet<string>();
            MatchCollection literalMatches = Regex.Matches(textToTranslate, literalPattern);
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
            textToTranslate = Regex.Replace(textToTranslate, @"\s+", " ");//used to remove multiple spaces in input user message
            string literalPattern = "<literal>(.*)</literal>";
            MatchCollection literalMatches = Regex.Matches(textToTranslate, literalPattern);
            if (literalMatches.Count > 0)
            {
                textToTranslate = Regex.Replace(textToTranslate, "</?literal>", " ");
            }
            return Regex.Replace(textToTranslate, @"\s+", " ");
        }

        /// <summary>
        /// Detects the language of the input text.
        /// </summary>
        /// <param name="textToDetect">The text to translate.</param>
        /// <returns>The language identifier.</returns>
        public async Task<string> Detect(string textToDetect)
        {
            textToDetect = PreprocessMessage(textToDetect);
            string url = "http://api.microsofttranslator.com/v2/Http.svc/Detect";
            string query = $"?text={System.Net.WebUtility.UrlEncode(textToDetect)}";

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                var accessToken = await _authToken.GetAccessTokenAsync().ConfigureAwait(false);
                request.Headers.Add("Authorization", accessToken);
                request.RequestUri = new Uri(url + query);
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return "ERROR: " + result;

                var detectedLang = XElement.Parse(result).Value;
                return detectedLang;
            }
        }

        /// <summary>
        /// Translates a single message from a source language to a target language.
        /// </summary>
        /// <param name="textToTranslate">The text to translate.</param>
        /// <param name="from">The language code of the translation text. For example, "en" for English.</param>
        /// <param name="to">The language code to translate the text into.</param>
        /// <returns>The translated document.</returns>
        public async Task<TranslatedDocument> Translate(string textToTranslate, string from, string to)
        {
            TranslatedDocument currentTranslatedDocument = new TranslatedDocument(textToTranslate);
            string processedText;
            HashSet<string> literanlNoTranslateList;
            PreprocessMessage(currentTranslatedDocument.SourceMessage, out processedText, out literanlNoTranslateList);
            currentTranslatedDocument.SourceMessage = processedText;
            currentTranslatedDocument.LiteranlNoTranslatePhrases = literanlNoTranslateList;

            string url = "http://api.microsofttranslator.com/v2/Http.svc/Translate";
            string query = $"?text={System.Net.WebUtility.UrlEncode(textToTranslate)}" +
                                 $"&from={from}" +
                                 $"&to={to}";

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                var accessToken = await _authToken.GetAccessTokenAsync().ConfigureAwait(false);
                request.Headers.Add("Authorization", accessToken);
                request.RequestUri = new Uri(url + query);
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new ArgumentException(result);

                var translatedText = XElement.Parse(result).Value.Trim();

                currentTranslatedDocument.TargetMessage = translatedText;
                return currentTranslatedDocument;
            }
        }

        /// <summary>
        /// Translates an array of strings from a source language to a target language.
        /// </summary>
        /// <param name="translateArraySourceTexts">The strings to translate.</param>
        /// <param name="from">The language code of the translation text. For example, "en" for English.</param>
        /// <param name="to">The language code to translate the text into.</param>
        /// <returns>An array of the translated documents.</returns>
        public async Task<List<TranslatedDocument>> TranslateArray(string[] translateArraySourceTexts, string from, string to)
        {
            List<TranslatedDocument> translatedDocuments = new List<TranslatedDocument>();
            var uri = "https://api.microsofttranslator.com/v2/Http.svc/TranslateArray2";
            for (int srcTxtIndx = 0; srcTxtIndx < translateArraySourceTexts.Length; srcTxtIndx++)
            {
                //Check for literal tag in input user message

                TranslatedDocument currentTranslatedDocument = new TranslatedDocument(translateArraySourceTexts[srcTxtIndx]);
                translatedDocuments.Add(currentTranslatedDocument);
                string processedText;
                HashSet<string> literanlNoTranslateList;
                PreprocessMessage(currentTranslatedDocument.SourceMessage, out processedText, out literanlNoTranslateList);
                currentTranslatedDocument.SourceMessage = processedText;
                translateArraySourceTexts[srcTxtIndx] = processedText;
                currentTranslatedDocument.LiteranlNoTranslatePhrases = literanlNoTranslateList;
            }

            //body of http request
            var body = $"<TranslateArrayRequest>" +
                           "<AppId />" +
                           $"<From>{from}</From>" +
                           "<Options>" +
                           " <Category xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" >generalnn</Category>" +
                               "<ContentType xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">text/plain</ContentType>" +
                               "<ReservedFlags xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<State xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<Uri xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<User xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                           "</Options>" +
                           "<Texts>" +
                                   String.Join("", translateArraySourceTexts.Select(s => $"<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">{SecurityElement.Escape(s)}</string>\n"))
                           + "</Texts>" +
                           $"<To>{to}</To>" +
                       "</TranslateArrayRequest>";

            var accessToken = await _authToken.GetAccessTokenAsync().ConfigureAwait(false);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                client.Timeout = TimeSpan.FromSeconds(20);
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(body, Encoding.UTF8, "text/xml");
                request.Headers.Add("Authorization", accessToken);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine("Request status is OK. Result of translate array method is:");
                        var doc = XDocument.Parse(responseBody);
                        var ns = XNamespace.Get("http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2");
                        List<string> results = new List<string>();
                        int sentIndex = 0;
                        foreach (XElement xe in doc.Descendants(ns + "TranslateArray2Response"))
                        {
                            TranslatedDocument currentTranslatedDocument = translatedDocuments[sentIndex];
                            currentTranslatedDocument.TargetMessage = xe.Element(ns + "TranslatedText").Value;
                            currentTranslatedDocument.RawAlignment = xe.Element(ns + "Alignment").Value;
                            if (!string.IsNullOrEmpty(currentTranslatedDocument.RawAlignment))
                            {
                                string[] alignments = currentTranslatedDocument.RawAlignment.Trim().Split(' ');
                                currentTranslatedDocument.SourceTokens = PostProcessingUtilities.SplitSentence(currentTranslatedDocument.SourceMessage, alignments);
                                currentTranslatedDocument.TranslatedTokens = PostProcessingUtilities.SplitSentence(xe.Element(ns + "TranslatedText").Value, alignments, false);
                                currentTranslatedDocument.IndexedAlignment = PostProcessingUtilities.WordAlignmentParse(alignments, currentTranslatedDocument.SourceTokens, currentTranslatedDocument.TranslatedTokens);
                            }
                            sentIndex += 1;
                        }
                        return translatedDocuments;
                    default:
                        throw new Exception(response.ReasonPhrase);
                }
            }
        }

    }

    internal class AzureAuthToken
    {
        /// URL of the token service
        private static readonly Uri ServiceUrl = new Uri("https://api.cognitive.microsoft.com/sts/v1.0/issueToken");

        /// Name of header used to pass the subscription key to the token service
        private const string OcpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

        /// After obtaining a valid token, this class will cache it for this duration.
        /// Use a duration of 5 minutes, which is less than the actual token lifetime of 10 minutes.
        private static readonly TimeSpan TokenCacheDuration = new TimeSpan(0, 5, 0);

        /// Cache the value of the last valid token obtained from the token service.
        private string _storedTokenValue = string.Empty;

        /// When the last valid token was obtained.
        private DateTime _storedTokenTime = DateTime.MinValue;

        /// Gets the subscription key.
        internal string SubscriptionKey { get; }

        /// Gets the HTTP status code for the most recent request to the token service.
        internal HttpStatusCode RequestStatusCode { get; private set; }

        /// <summary>
        /// Creates a client to obtain an access token.
        /// </summary>
        /// <param name="key">Subscription key to use to get an authentication token.</param>
        internal AzureAuthToken(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "A subscription key is required");

            this.SubscriptionKey = key;
            this.RequestStatusCode = HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Gets a token for the specified subscription.
        /// </summary>
        /// <returns>The encoded JWT token prefixed with the string "Bearer ".</returns>
        /// <remarks>
        /// This method uses a cache to limit the number of request to the token service.
        /// A fresh token can be re-used during its lifetime of 10 minutes. After a successful
        /// request to the token service, this method caches the access token. Subsequent 
        /// invocations of the method return the cached token for the next 5 minutes. After
        /// 5 minutes, a new token is fetched from the token service and the cache is updated.
        /// </remarks>
        internal async Task<string> GetAccessTokenAsync()
        {
            if (string.IsNullOrWhiteSpace(this.SubscriptionKey))
                return string.Empty;

            // Re-use the cached token if there is one.
            if ((DateTime.Now - _storedTokenTime) < TokenCacheDuration)
                return _storedTokenValue;

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = ServiceUrl;
                request.Content = new StringContent(string.Empty);
                request.Headers.TryAddWithoutValidation(OcpApimSubscriptionKeyHeader, this.SubscriptionKey);
                client.Timeout = TimeSpan.FromSeconds(20);
                var response = await client.SendAsync(request);
                this.RequestStatusCode = response.StatusCode;
                response.EnsureSuccessStatusCode();
                var token = await response.Content.ReadAsStringAsync();
                _storedTokenTime = DateTime.Now;
                _storedTokenValue = "Bearer " + token;
                return _storedTokenValue;
            }
        }
    }

}
