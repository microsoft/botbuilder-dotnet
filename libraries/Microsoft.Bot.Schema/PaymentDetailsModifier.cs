﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides details that modify the PaymentDetails based on payment method
    /// identifier
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentDetailsModifier
    {
        /// <summary>
        /// Initializes a new instance of the PaymentDetailsModifier class.
        /// </summary>
        public PaymentDetailsModifier()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the PaymentDetailsModifier class.
        /// </summary>
        /// <param name="supportedMethods">Contains a sequence of payment
        /// method identifiers</param>
        /// <param name="total">This value overrides the total field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field</param>
        /// <param name="additionalDisplayItems">Provides additional display
        /// items that are appended to the displayItems field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field</param>
        /// <param name="data">A JSON-serializable object that provides
        /// optional information that might be needed by the supported payment
        /// methods</param>
        public PaymentDetailsModifier(IList<string> supportedMethods = default(IList<string>), PaymentItem total = default(PaymentItem), IList<PaymentItem> additionalDisplayItems = default(IList<PaymentItem>), object data = default(object))
        {
            SupportedMethods = supportedMethods;
            Total = total;
            AdditionalDisplayItems = additionalDisplayItems;
            Data = data;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets contains a sequence of payment method identifiers
        /// </summary>
        [JsonProperty(PropertyName = "supportedMethods")]
        public IList<string> SupportedMethods { get; set; }

        /// <summary>
        /// Gets or sets this value overrides the total field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field
        /// </summary>
        [JsonProperty(PropertyName = "total")]
        public PaymentItem Total { get; set; }

        /// <summary>
        /// Gets or sets provides additional display items that are appended to
        /// the displayItems field in the PaymentDetails dictionary for the
        /// payment method identifiers in the supportedMethods field
        /// </summary>
        [JsonProperty(PropertyName = "additionalDisplayItems")]
        public IList<PaymentItem> AdditionalDisplayItems { get; set; }

        /// <summary>
        /// Gets or sets a JSON-serializable object that provides optional
        /// information that might be needed by the supported payment methods
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

    }
}
