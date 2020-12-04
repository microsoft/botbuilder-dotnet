// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Indicates a set of supported payment methods and any associated payment
    /// method specific data for those methods.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentMethodData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentMethodData"/> class.
        /// </summary>
        public PaymentMethodData()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentMethodData"/> class.
        /// </summary>
        /// <param name="supportedMethods">Required sequence of strings
        /// containing payment method identifiers for payment methods that the
        /// merchant web site accepts.</param>
        /// <param name="data">A JSON-serializable object that provides
        /// optional information that might be needed by the supported payment
        /// methods.</param>
        public PaymentMethodData(IList<string> supportedMethods = default(IList<string>), object data = default(object))
        {
            SupportedMethods = supportedMethods;
            Data = data;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets required sequence of strings containing payment method
        /// identifiers for payment methods that the merchant web site accepts.
        /// </summary>
        /// <value>The supported payment methods.</value>
        [JsonProperty(PropertyName = "supportedMethods")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<string> SupportedMethods { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets a JSON-serializable object that provides optional
        /// information that might be needed by the supported payment methods.
        /// </summary>
        /// <value>The JSON-serializable data object that provides optional information.</value>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
