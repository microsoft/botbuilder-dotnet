// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
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
        public MicrosoftPayMethodData(string merchantId = default, IList<string> supportedNetworks = default, IList<string> supportedTypes = default, bool testMode = false)
            : this(merchantId, supportedNetworks, supportedTypes)
        {
            Mode = testMode ? TestModeValue : null;
        }

        /// <summary>
        /// Gets or sets Microsoft Pay Merchant ID.
        /// </summary>
        /// <value>The Microsoft Pay Merchant ID.</value>
        [JsonPropertyName("merchantId")]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets payment method mode.
        /// </summary>
        /// <value>
        /// Payment method mode.
        /// </value>
        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets supported payment networks (e.g., "visa" and
        /// "mastercard").
        /// </summary>
        /// <value>The supported payment networks.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("supportedNetworks")]
        public IList<string> SupportedNetworks { get; set; }

        /// <summary>
        /// Gets or sets supported payment types (e.g., "credit").
        /// </summary>
        /// <value>The supported payment types.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("supportedTypes")]
        public IList<string> SupportedTypes { get; set; }

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
