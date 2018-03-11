// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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

namespace Microsoft.Bot.Builder.Ai
{
    /// <summary>
    /// PostProcessTranslator  is used to handle translation errors while translating numbers
    /// and to handle words that needs to be kept same as source language from provided template each line having a regex
    /// having first group matching the words that needs to be kept
    /// </summary>
    internal class PostProcessTranslator
    {
        HashSet<string> noTranslatePatterns; 

        //Constructor that indexes input template for source language
        internal PostProcessTranslator(string noTranslateTemplatePath)
        {
            noTranslatePatterns = new HashSet<string>();
            foreach (string line in File.ReadLines(noTranslateTemplatePath))
            {
                string processedLine = line.Trim();
                if (!line.Contains('('))
                {
                    processedLine = '(' + processedLine + ')';
                }
                noTranslatePatterns.Add(processedLine);
            }
        }

        //Constructor for postprocessor that fixes numbers only
        internal PostProcessTranslator()
        {
            noTranslatePatterns = new HashSet<string>();
        }

        //parsing alignment information onto a dictionary
        //dictionary key is character index start of source word  : character index end of source word
        //value is character index start of target word : length of target word to use substring directly
        private Dictionary<string, string> wordAlignmentParse(string alignment, string source, string target)
        {
            Dictionary<string, string> alignMap = new Dictionary<string, string>();
            if (alignment.Trim() == "")
                return alignMap;
            string[] alignments = alignment.Trim().Split(' ');
            foreach (string alignData in alignments)
            {
                    string[] wordIndexes = alignData.Split('-');
                    int trgstartIndex = Int32.Parse(wordIndexes[1].Split(':')[0]);
                    int trgLength = Int32.Parse(wordIndexes[1].Split(':')[1]) - trgstartIndex + 1;
                    alignMap[wordIndexes[0]] = trgstartIndex + ":" + trgLength;
            }
            return alignMap;
        }

        //use alignment information source sentence and target sentence 
        //to keep a specific word from the source onto target translation
        private string keepSrcWrdInTranslation(Dictionary<string, string> alignment, string source, string target, string srcWrd)
        {
            string processedTranslation = target;
            int wrdStartIndex = source.IndexOf(srcWrd);
            int wrdEndIndex = wrdStartIndex + srcWrd.Count() - 1;
            string wrdIndexesString = wrdStartIndex + ":" + wrdEndIndex;
            if (alignment.ContainsKey(wrdIndexesString))
            {
                string[] trgWrdLocation = alignment[wrdIndexesString].Split(':');
                string targetWrd = target.Substring(Int32.Parse(trgWrdLocation[0]), Int32.Parse(trgWrdLocation[1]));
                if (targetWrd.Trim().Length == Int32.Parse(trgWrdLocation[1]) && targetWrd != srcWrd)
                    processedTranslation = processedTranslation.Replace(targetWrd, srcWrd);
            }
            return processedTranslation;
        }

        //Fixing translation  
        //used to handle numbers and no translate list
        internal string FixTranslation(string sourceMessage, string alignment, string targetMessage)
        {
            string processedTranslation = targetMessage;
            bool containsNum = Regex.IsMatch(sourceMessage, @"\d");

            if (noTranslatePatterns.Count == 0 && !containsNum)
                return processedTranslation;

            var toBeReplaced = from result in noTranslatePatterns
                               where Regex.IsMatch(sourceMessage, result, RegexOptions.Singleline | RegexOptions.IgnoreCase)
                               select result;
            Dictionary<string, string> alignMap = wordAlignmentParse(alignment, sourceMessage, targetMessage);
            if (toBeReplaced.Count() > 0)
            {
                foreach (string pattern in toBeReplaced)
                {
                    Match matchNoTranslate = Regex.Match(sourceMessage, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    string[] wrdNoTranslate = matchNoTranslate.Groups[1].Value.Split(' ');
                    foreach (string srcWrd in wrdNoTranslate)
                    {
                        processedTranslation = keepSrcWrdInTranslation(alignMap, sourceMessage, processedTranslation, srcWrd);
                    }

                }
            }
            MatchCollection numericMatches = Regex.Matches(sourceMessage, @"\d+", RegexOptions.Singleline);
            foreach (Match numericMatch in numericMatches)
            {
                processedTranslation = keepSrcWrdInTranslation(alignMap, sourceMessage, processedTranslation, numericMatch.Groups[0].Value);
            }

            return processedTranslation;
        } 

    }

    /// <summary>
    /// Translator class 
    /// contains machine translation APIs 
    /// uses api key and detect input language translate single sentence or array of sentences then apply translation post processing fix
    /// </summary>
    public class Translator
    {
        AzureAuthToken authToken;
        PostProcessTranslator postProcessor;

        public Translator(string apiKey)
        {
            authToken = new AzureAuthToken(apiKey);
            postProcessor = new PostProcessTranslator();
        }

        //used to set no translate template for post processor
        public void SetPostProcessorTemplate(string templatePath)
        {
            postProcessor = new PostProcessTranslator(templatePath);
        }

        //detects language of input text 
        public async Task<string> Detect(string textToDetect)
        {
            string url = "http://api.microsofttranslator.com/v2/Http.svc/Detect";
            string query = $"?text={System.Net.WebUtility.UrlEncode(textToDetect)}";

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                var accessToken = await authToken.GetAccessTokenAsync().ConfigureAwait(false);
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

        //translate single message from source lang to target lang
        public async Task<string> Translate(string textToTranslate, string from, string to)
        {
            string url = "http://api.microsofttranslator.com/v2/Http.svc/Translate";
            string query = $"?text={System.Net.WebUtility.UrlEncode(textToTranslate)}" +
                                 $"&from={from}" +
                                 $"&to={to}";

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                var accessToken = await authToken.GetAccessTokenAsync().ConfigureAwait(false);
                request.Headers.Add("Authorization", accessToken);
                request.RequestUri = new Uri(url + query);
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return "ERROR: " + result;

                var translatedText = XElement.Parse(result).Value;

                return translatedText;
            }
        }

        //translate array of strings from source language to target language
        public async Task<string[]> TranslateArray(string[] translateArraySourceTexts, string from, string to)
        {
            var uri = "https://api.microsofttranslator.com/v2/Http.svc/TranslateArray2";
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

            var accessToken = await authToken.GetAccessTokenAsync().ConfigureAwait(false);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
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

                            string translation = xe.Element(ns + "TranslatedText").Value;
                            translation = postProcessor.FixTranslation(translateArraySourceTexts[sentIndex], xe.Element(ns + "Alignment").Value, translation);
                            results.Add(translation);
                        }
                        return results.ToArray();

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
                client.Timeout = TimeSpan.FromSeconds(2);
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