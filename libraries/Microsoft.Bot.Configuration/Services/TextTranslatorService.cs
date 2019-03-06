using System;
using Microsoft.Bot.Configuration.Encryption;
using Newtonsoft.Json;

namespace Microsoft.Bot.Configuration
{
    public enum TranslatorEngine
    {
        /// <summary>
        /// Microsoft Translator API
        /// </summary>
        MicrosoftTranslator,

        /// <summary>
        /// Use of Deepl Translator
        /// </summary>
        Deepl,
    }

    public class TextTranslatorService : ConnectedService
    {
        public TextTranslatorService()
            : base(ServiceTypes.TextTranslator)
        {
        }

        [JsonProperty("engine")]
        public TranslatorEngine Engine { get; set; }

        [JsonProperty("subscriptionKey")]
        public string SubscriptionKey { get; set; }

        /// <inheritdoc/>
        public override void Encrypt(string secret)
        {
            base.Encrypt(secret);

            if (!string.IsNullOrEmpty(this.SubscriptionKey))
            {
                this.SubscriptionKey = this.SubscriptionKey.Encrypt(secret);
            }
        }

        /// <inheritdoc/>
        public override void Decrypt(string secret)
        {
            base.Decrypt(secret);

            if (!string.IsNullOrEmpty(this.SubscriptionKey))
            {
                this.SubscriptionKey = this.SubscriptionKey.Decrypt(secret);
            }
        }
    }
}
