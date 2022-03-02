// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// An update to a payment request.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentRequestUpdate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequestUpdate"/> class.
        /// </summary>
        /// <param name="id">ID for the payment request to update.</param>
        /// <param name="details">Update payment details.</param>
        /// <param name="shippingAddress">Updated shipping address.</param>
        /// <param name="shippingOption">Updated shipping options.</param>
        public PaymentRequestUpdate(string id = default, PaymentDetails details = default, PaymentAddress shippingAddress = default, string shippingOption = default)
        {
            Id = id;
            Details = details;
            ShippingAddress = shippingAddress;
            ShippingOption = shippingOption;
        }

        /// <summary>
        /// Gets or sets ID for the payment request to update.
        /// </summary>
        /// <value>The ID for the payment request to update.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets update payment details.
        /// </summary>
        /// <value>The update payment details.</value>
        [JsonProperty(PropertyName = "details")]
        public PaymentDetails Details { get; set; }

        /// <summary>
        /// Gets or sets updated shipping address.
        /// </summary>
        /// <value>The updated shipping address.</value>
        [JsonProperty(PropertyName = "shippingAddress")]
        public PaymentAddress ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets updated shipping options.
        /// </summary>
        /// <value>The updated shipping options.</value>
        [JsonProperty(PropertyName = "shippingOption")]
        public string ShippingOption { get; set; }
    }
}
