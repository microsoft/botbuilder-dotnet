// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// A result object from a Payment Request Update invoke operation.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentRequestUpdateResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequestUpdateResult"/> class.
        /// </summary>
        public PaymentRequestUpdateResult()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequestUpdateResult"/> class.
        /// </summary>
        /// <param name="details">Update payment details.</param>
        public PaymentRequestUpdateResult(PaymentDetails details = default)
        {
            Details = details;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets update payment details.
        /// </summary>
        /// <value>The update payment details.</value>
        [JsonPropertyName("details")]
        public PaymentDetails Details { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new NotImplementedException();
        }
    }
}
