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
        public MicrosoftPayMethodData(string merchantId = default(string), IList<string> supportedNetworks = default(IList<string>), IList<string> supportedTypes = default(IList<string>))
        {
            MerchantId = merchantId;
            SupportedNetworks = supportedNetworks;
            SupportedTypes = supportedTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftPayMethodData"/> class.
        /// </summary>
        /// <param name="merchantId">merchant Id.</param>
        /// <param name="supportedNetworks">supported networks.</param>
        /// <param name="supportedTypes">supported types.</param>
        /// <param name="testMode">test mode.</param>
        public MicrosoftPayMethodData(string merchantId = default(string), IList<string> supportedNetworks = default(IList<string>), IList<string> supportedTypes = default(IList<string>), bool testMode = false)
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
        /// Gets or sets supported payment networks (e.g., "visa" and
        /// "mastercard").
        /// </summary>
        /// <value>The supported payment networks.</value>
        [JsonProperty(PropertyName = "supportedNetworks")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<string> SupportedNetworks { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets supported payment types (e.g., "credit").
        /// </summary>
        /// <value>The supported payment types.</value>
        [JsonProperty(PropertyName = "supportedTypes")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<string> SupportedTypes { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Get Microsoft Pay method data.
        /// </summary>
        /// <returns>Payment method data.</returns>
        public PaymentMethodData ToPaymentMethodData()
        {
            return new PaymentMethodData
            {
                SupportedMethods = new List<string> { MethodName },
                Data = this,
            };
        }
    }
}
