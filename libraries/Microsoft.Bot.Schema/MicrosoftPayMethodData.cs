﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// W3C Payment Method Data for Microsoft Pay
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class MicrosoftPayMethodData
    {
        /// <summary>
        /// Initializes a new instance of the MicrosoftPayMethodData class.
        /// </summary>
        public MicrosoftPayMethodData()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MicrosoftPayMethodData class.
        /// </summary>
        /// <param name="merchantId">Microsoft Pay Merchant ID</param>
        /// <param name="supportedNetworks">Supported payment networks (e.g.,
        /// "visa" and "mastercard")</param>
        /// <param name="supportedTypes">Supported payment types (e.g.,
        /// "credit")</param>
        public MicrosoftPayMethodData(string merchantId = default(string), IList<string> supportedNetworks = default(IList<string>), IList<string> supportedTypes = default(IList<string>))
        {
            MerchantId = merchantId;
            SupportedNetworks = supportedNetworks;
            SupportedTypes = supportedTypes;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets microsoft Pay Merchant ID
        /// </summary>
        [JsonProperty(PropertyName = "merchantId")]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets supported payment networks (e.g., "visa" and
        /// "mastercard")
        /// </summary>
        [JsonProperty(PropertyName = "supportedNetworks")]
        public IList<string> SupportedNetworks { get; set; }

        /// <summary>
        /// Gets or sets supported payment types (e.g., "credit")
        /// </summary>
        [JsonProperty(PropertyName = "supportedTypes")]
        public IList<string> SupportedTypes { get; set; }

    }
}
