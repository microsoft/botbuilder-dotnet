﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// A result object from a Payment Request Update invoke operation
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentRequestUpdateResult
    {
        /// <summary>
        /// Initializes a new instance of the PaymentRequestUpdateResult class.
        /// </summary>
        public PaymentRequestUpdateResult()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the PaymentRequestUpdateResult class.
        /// </summary>
        /// <param name="details">Update payment details</param>
        public PaymentRequestUpdateResult(PaymentDetails details = default(PaymentDetails))
        {
            Details = details;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets update payment details
        /// </summary>
        [JsonProperty(PropertyName = "details")]
        public PaymentDetails Details { get; set; }

    }
}
