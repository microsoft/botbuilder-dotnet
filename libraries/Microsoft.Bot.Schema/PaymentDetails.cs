// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides information about the requested transaction.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetails"/> class.
        /// </summary>
        public PaymentDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetails"/> class.
        /// </summary>
        /// <param name="total">Contains the total amount of the payment
        /// request.</param>
        /// <param name="displayItems">Contains line items for the payment
        /// request that the user agent may display.</param>
        /// <param name="shippingOptions">A sequence containing the different
        /// shipping options for the user to choose from.</param>
        /// <param name="modifiers">Contains modifiers for particular payment
        /// method identifiers.</param>
        /// <param name="error">Error description.</param>
        public PaymentDetails(PaymentItem total = default, IList<PaymentItem> displayItems = default, IList<PaymentShippingOption> shippingOptions = default, IList<PaymentDetailsModifier> modifiers = default, string error = default)
        {
            Total = total;
            DisplayItems = displayItems ?? new List<PaymentItem>();
            ShippingOptions = shippingOptions ?? new List<PaymentShippingOption>();
            Modifiers = modifiers ?? new List<PaymentDetailsModifier>();
            Error = error;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets contains the total amount of the payment request.
        /// </summary>
        /// <value>The total amount of the payment request.</value>
        [JsonProperty(PropertyName = "total")]
        public PaymentItem Total { get; set; }

        /// <summary>
        /// Gets contains line items for the payment request that the
        /// user agent may display.
        /// </summary>
        /// <value>The items for the payment request.</value>
        [JsonProperty(PropertyName = "displayItems")]
        public IList<PaymentItem> DisplayItems { get; private set; } = new List<PaymentItem>();

        /// <summary>
        /// Gets a sequence containing the different shipping options
        /// for the user to choose from.
        /// </summary>
        /// <value>The the different shipping options for the user to choose from.</value>
        [JsonProperty(PropertyName = "shippingOptions")]
        public IList<PaymentShippingOption> ShippingOptions { get; private set; } = new List<PaymentShippingOption>();

        /// <summary>
        /// Gets contains modifiers for particular payment method
        /// identifiers.
        /// </summary>
        /// <value>The modifiers for a particular payment method.</value>
        [JsonProperty(PropertyName = "modifiers")]
        public IList<PaymentDetailsModifier> Modifiers { get; private set; } = new List<PaymentDetailsModifier>();

        /// <summary>
        /// Gets or sets error description.
        /// </summary>
        /// <value>The error description.</value>
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
