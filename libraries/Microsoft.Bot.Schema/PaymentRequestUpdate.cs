﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// An update to a payment request
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentRequestUpdate
    {
        /// <summary>
        /// Initializes a new instance of the PaymentRequestUpdate class.
        /// </summary>
        public PaymentRequestUpdate()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the PaymentRequestUpdate class.
        /// </summary>
        /// <param name="id">ID for the payment request to update</param>
        /// <param name="details">Update payment details</param>
        /// <param name="shippingAddress">Updated shipping address</param>
        /// <param name="shippingOption">Updated shipping options</param>
        public PaymentRequestUpdate(string id = default(string), PaymentDetails details = default(PaymentDetails), PaymentAddress shippingAddress = default(PaymentAddress), string shippingOption = default(string))
        {
            Id = id;
            Details = details;
            ShippingAddress = shippingAddress;
            ShippingOption = shippingOption;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets ID for the payment request to update
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets update payment details
        /// </summary>
        [JsonProperty(PropertyName = "details")]
        public PaymentDetails Details { get; set; }

        /// <summary>
        /// Gets or sets updated shipping address
        /// </summary>
        [JsonProperty(PropertyName = "shippingAddress")]
        public PaymentAddress ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets updated shipping options
        /// </summary>
        [JsonProperty(PropertyName = "shippingOption")]
        public string ShippingOption { get; set; }

    }
}
