// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// W3C Payment Method Data for Microsoft Pay.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class MicrosoftPayMethodData
    {
        /// <summary>
        /// The pay method name.
        /// </summary>
        public const string MethodName = "https://pay.microsoft.com/microsoftpay";

        private const string TestModeValue = "TEST";

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftPayMethodData"/> class.
        /// </summary>
        public MicrosoftPayMethodData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftPayMethodData"/> class.
        /// </summary>
        /// <param name="merchantId">Microsoft Pay Merchant ID.</param>
        /// <param name="supportedNetworks">Supported payment networks (e.g.,
        /// "visa" and "mastercard").</param>
        /// <param name="supportedTypes">Supported payment types (e.g.,
        /// "credit").</param>
        public MicrosoftPayMethodData(string merchantId = default, IList<string> supportedNetworks = default, IList<string> supportedTypes = default)
        {
            MerchantId = merchantId;
            SupportedNetworks = supportedNetworks ?? new List<string>();
            SupportedTypes = supportedTypes ?? new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftPayMethodData"/> class.
        /// </summary>
        /// <param name="merchantId">merchant Id.</param>
        /// <param name="supportedNetworks">supported networks.</param>
        /// <param name="supportedTypes">supported types.</param>
        /// <param name="testMode">test mode.</param>
        public MicrosoftPayMethodData(string merchantId = default, IList<string> supportedNetworks = default, IList<string> supportedTypes = default, bool testMode = false)
            : this(merchantId, supportedNetworks, supportedTypes)
        {
            Mode = testMode ? TestModeValue : null;
        }

        /// <summary>
        /// Gets or sets Microsoft Pay Merchant ID.
        /// </summary>
        /// <value>The Microsoft Pay Merchant ID.</value>
        [JsonProperty(PropertyName = "merchantId")]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets payment method mode.
        /// </summary>
        /// <value>
        /// Payment method mode.
        /// </value>
        [JsonProperty(PropertyName = "mode", NullValueHandling = NullValueHandling.Ignore)]
        public string Mode { get; set; }

        /// <summary>
        /// Gets supported payment networks (e.g., "visa" and
        /// "mastercard").
        /// </summary>
        /// <value>The supported payment networks.</value>
        [JsonProperty(PropertyName = "supportedNetworks")]
        public IList<string> SupportedNetworks { get; private set; } = new List<string>();

        /// <summary>
        /// Gets supported payment types (e.g., "credit").
        /// </summary>
        /// <value>The supported payment types.</value>
        [JsonProperty(PropertyName = "supportedTypes")]
        public IList<string> SupportedTypes { get; private set; } = new List<string>();

        /// <summary>
        /// Get Microsoft Pay method data.
        /// </summary>
        /// <returns>Payment method data.</returns>
        public PaymentMethodData ToPaymentMethodData()
        {
            var supportedMethods = new List<string> { MethodName };
            return new PaymentMethodData(supportedMethods: supportedMethods)
            {
                Data = this,
            };
        }
    }
}
