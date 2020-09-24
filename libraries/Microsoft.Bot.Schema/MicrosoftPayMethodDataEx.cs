// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Pay method data for Microsoft Payment.
    /// </summary>
    public partial class MicrosoftPayMethodData
    {
        /// <summary>
        /// The pay method name.
        /// </summary>
        public const string MethodName = "https://pay.microsoft.com/microsoftpay";

        private const string TestModeValue = "TEST";

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
        /// Gets or sets payment method mode.
        /// </summary>
        /// <value>
        /// Payment method mode.
        /// </value>
        [JsonProperty(PropertyName = "mode", NullValueHandling = NullValueHandling.Ignore)]
        public string Mode { get; set; }

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
