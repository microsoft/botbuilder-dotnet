using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.TextTranslator.Deepl;
using Microsoft.Bot.Builder.AI.TextTranslator.MsTranslator;
using Microsoft.Bot.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.TextTranslator
{
    public class TextTranslator
    {
        private const string MsTranslatorUri = "https://api.cognitive.microsofttranslator.com";
        private const string DeeplRequestUri = "https://api.deepl.com/v1/translate";

        private static readonly HttpClient DefaultHttpClient = new HttpClient();

        private string _subscriptionKey;
        private TranslatorEngine _engine;
        private HttpClient _httpClient;

        public TextTranslator(TextTranslatorService textTranslatorService, HttpClient httpClient = null)
        {
            _subscriptionKey = textTranslatorService.SubscriptionKey;
            _engine = textTranslatorService.Engine;
            _httpClient = httpClient ?? DefaultHttpClient;
        }

        public async Task<TranslatorResult> TranslateAsync(ITurnContext turnContext, string targetLanguage, string sourceLanguage = null)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity == null)
            {
                throw new ArgumentNullException(nameof(turnContext.Activity));
            }

            var messageActivity = turnContext.Activity.AsMessageActivity();
            if (messageActivity == null)
            {
                throw new ArgumentException("Activity type is not a message");
            }

            if (string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                throw new ArgumentException("Null or empty text");
            }

            return await TranslateAsync(turnContext.Activity.Text, targetLanguage, sourceLanguage).ConfigureAwait(false);
        }

        public async Task<TranslatorResult> TranslateAsync(string text, string targetLanguage, string sourceLanguage = null)
        {
            switch (_engine)
            {
                case TranslatorEngine.MicrosoftTranslator:
                    return await TranslateMicrosoftTranslatorAsync(text, targetLanguage, sourceLanguage).ConfigureAwait(false);
                case TranslatorEngine.Deepl:
                    return await TranslateDeeplAsync(text, targetLanguage, sourceLanguage).ConfigureAwait(false);
                default:
                    throw new InvalidOperationException();
            }
        }

        internal async Task<TranslatorResult> TranslateDeeplAsync(string text, string targetLanguage, string sourceLanguage = null)
        {
            var param = new Dictionary<string, string>()
            {
                { "auth_key", _subscriptionKey },
                { "source_lang", sourceLanguage?.ToUpper() },
                { "target_lang", targetLanguage.ToUpper() },
                { "text", text },
            };
            var content = new FormUrlEncodedContent(param);

            var response = await _httpClient.PostAsync(DeeplRequestUri, content).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new Exception("Translation request forbidden.");
            }

            var translationResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var results = JsonConvert.DeserializeObject<DeeplTranslationResults>(translationResponse);
            return TranslatorResult.Create(results, targetLanguage);
        }

        internal async Task<TranslatorResult> TranslateMicrosoftTranslatorAsync(string text, string targetLanguage, string sourceLanguage = null)
        {
            var body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            var parameters = $"/translate?api-version=3.0&from={sourceLanguage ?? string.Empty}&to={targetLanguage}";

            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(MsTranslatorUri + parameters);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                var translationResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var results = JsonConvert.DeserializeObject<MsTranslatorTranslationResults[]>(translationResponse);

                return TranslatorResult.Create(results.First(), targetLanguage);
            }
        }
    }
}
