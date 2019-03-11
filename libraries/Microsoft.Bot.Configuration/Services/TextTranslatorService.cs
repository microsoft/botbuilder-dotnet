using System;
using Microsoft.Bot.Configuration.Encryption;
using Newtonsoft.Json;

namespace Microsoft.Bot.Configuration
{
    public class TextTranslatorService : ConnectedService
    {
        /// <summary>Initializes a new instance of the <see cref="TextTranslatorService"/> class.</summary>
        public TextTranslatorService()
            : base(ServiceTypes.TextTranslator)
        {
        }

        /// <summary>
        ///   <para>
        /// Engine to use: </para>
        ///   <para></para>
        ///   <list type="table">
        ///     <item>
        ///       <description>Microsoft Translator</description>
        ///       <description>microsoftranslator</description>
        ///     </item>
        ///     <item>
        ///       <description>Deepl</description>
        ///       <description>deepl</description>
        ///     </item>
        ///   </list>
        ///   <para></para>
        /// </summary>
        [JsonProperty("engine")]
        public string Engine { get; set; }

        /// <summary>Gets or sets the subscription key.</summary>
        /// <value>The subscription key.</value>
        [JsonProperty("subscriptionKey")]
        public string SubscriptionKey { get; set; }

        /// <summary>Gets or sets the host.</summary>
        /// <value>The host.</value>
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
