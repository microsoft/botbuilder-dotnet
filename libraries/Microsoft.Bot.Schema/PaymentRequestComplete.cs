// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Payload delivered when completing a payment request.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentRequestComplete
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequestComplete"/> class.
        /// </summary>
        /// <param name="id">Payment request ID.</param>
        /// <param name="paymentRequest">Initial payment request.</param>
        /// <param name="paymentResponse">Corresponding payment
        /// response.</param>
        public PaymentRequestComplete(string id = default, PaymentRequest paymentRequest = default, PaymentResponse paymentResponse = default)
        {
            Id = id;
            PaymentRequest = paymentRequest;
            PaymentResponse = paymentResponse;
        }

        /// <summary>
        /// Gets or sets payment request ID.
        /// </summary>
        /// <value>The payment request ID.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets initial payment request.
        /// </summary>
        /// <value>The initial payment request.</value>
        [JsonProperty(PropertyName = "paymentRequest")]
        public PaymentRequest PaymentRequest { get; set; }

        /// <summary>
        /// Gets or sets corresponding payment response.
        /// </summary>
        /// <value>The payment reesponse.</value>
        [JsonProperty(PropertyName = "paymentResponse")]
        public PaymentResponse PaymentResponse { get; set; }
    }
}
