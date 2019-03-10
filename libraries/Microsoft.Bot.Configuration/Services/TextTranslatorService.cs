using System;
using Microsoft.Bot.Configuration.Encryption;
using Newtonsoft.Json;

namespace Microsoft.Bot.Configuration
{
    public class TextTranslatorService : ConnectedService
    {
        public TextTranslatorService()
            : base(ServiceTypes.TextTranslator)
        {
        }

        /// <summary>
        /// microsofttranslator, deepl
        /// </summary>
        [JsonProperty("engine")]
        public string Engine { get; set; }

        [JsonProperty("subscriptionKey")]
        public string SubscriptionKey { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

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
