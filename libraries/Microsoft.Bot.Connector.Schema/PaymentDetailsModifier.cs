// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Provides details that modify the PaymentDetails based on payment method
    /// identifier.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentDetailsModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetailsModifier"/> class.
        /// </summary>
        public PaymentDetailsModifier()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetailsModifier"/> class.
        /// </summary>
        /// <param name="supportedMethods">Contains a sequence of payment
        /// method identifiers.</param>
        /// <param name="total">This value overrides the total field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field.</param>
        /// <param name="additionalDisplayItems">Provides additional display
        /// items that are appended to the displayItems field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field.</param>
        /// <param name="data">A JSON-serializable object that provides
        /// optional information that might be needed by the supported payment
        /// methods.</param>
        public PaymentDetailsModifier(IList<string> supportedMethods = default, PaymentItem total = default, IList<PaymentItem> additionalDisplayItems = default, object data = default)
        {
            SupportedMethods = supportedMethods;
            Total = total;
            AdditionalDisplayItems = additionalDisplayItems;
            Data = data;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets contains a sequence of payment method identifiers.
        /// </summary>
        /// <value>The supported method identifiers.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("supportedMethods")]
        public IList<string> SupportedMethods { get; set; }

        /// <summary>
        /// Gets or sets this value overrides the total field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field.
        /// </summary>
        /// <value>The total.</value>
        [JsonPropertyName("total")]
        public PaymentItem Total { get; set; }

        /// <summary>
        /// Gets or sets provides additional display items that are appended to
        /// the displayItems field in the PaymentDetails dictionary for the
        /// payment method identifiers in the supportedMethods field.
        /// </summary>
        /// <value>The additional display items that are appended to the displayItems field in PaymentDetails.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("additionalDisplayItems")]
        public IList<PaymentItem> AdditionalDisplayItems { get; set; }

        /// <summary>
        /// Gets or sets a JSON-serializable object that provides optional
        /// information that might be needed by the supported payment methods.
        /// </summary>
        /// <value>The JSON-serializable object that provides optional information.</value>
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
