// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides details that modify the PaymentDetails based on payment method
    /// identifier.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentDetailsModifier
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
        public PaymentDetailsModifier(IList<string> supportedMethods = default(IList<string>), PaymentItem total = default(PaymentItem), IList<PaymentItem> additionalDisplayItems = default(IList<PaymentItem>), object data = default(object))
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
        [JsonProperty(PropertyName = "supportedMethods")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<string> SupportedMethods { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets this value overrides the total field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field.
        /// </summary>
        /// <value>The total.</value>
        [JsonProperty(PropertyName = "total")]
        public PaymentItem Total { get; set; }

        /// <summary>
        /// Gets or sets provides additional display items that are appended to
        /// the displayItems field in the PaymentDetails dictionary for the
        /// payment method identifiers in the supportedMethods field.
        /// </summary>
        /// <value>The additional display items that are appended to the displayItems field in PaymentDetails.</value>
        [JsonProperty(PropertyName = "additionalDisplayItems")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<PaymentItem> AdditionalDisplayItems { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets a JSON-serializable object that provides optional
        /// information that might be needed by the supported payment methods.
        /// </summary>
        /// <value>The JSON-serializable object that provides optional information.</value>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
