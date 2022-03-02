// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Indicates a set of supported payment methods and any associated payment
    /// method specific data for those methods.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentMethodData
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
        public PaymentMethodData(IList<string> supportedMethods = default, object data = default)
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
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("supportedMethods")]
        public IList<string> SupportedMethods { get; set; }

        /// <summary>
        /// Gets or sets a JSON-serializable object that provides optional
        /// information that might be needed by the supported payment methods.
        /// </summary>
        /// <value>The JSON-serializable data object that provides optional information.</value>
        [JsonPropertyName("data")]
        public object Data { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
