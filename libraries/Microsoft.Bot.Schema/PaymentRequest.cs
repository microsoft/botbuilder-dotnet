// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A request to make a payment.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentRequest
    {
        /// <summary>
        /// Action type for Payment action.
        /// </summary>
        public const string PaymentActionType = "payment";

        /// <summary>
        /// Content-type for Payment card.
        /// </summary>
        public const string PaymentContentType = "application/vnd.microsoft.card.payment";

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequest"/> class.
        /// </summary>
        public PaymentRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequest"/> class.
        /// </summary>
        /// <param name="id">ID of this payment request.</param>
        /// <param name="methodData">Allowed payment methods for this
        /// request.</param>
        /// <param name="details">Details for this request.</param>
        /// <param name="options">Provides information about the options
        /// desired for the payment request.</param>
        /// <param name="expires">Expiration for this request, in ISO 8601
        /// duration format (e.g., 'P1D').</param>
        public PaymentRequest(string id = default, IList<PaymentMethodData> methodData = default, PaymentDetails details = default, PaymentOptions options = default, string expires = default)
        {
            Id = id;
            MethodData = methodData ?? new List<PaymentMethodData>();
            Details = details;
            Options = options;
            Expires = expires;
        }

        /// <summary>
        /// Gets or sets ID of this payment request.
        /// </summary>
        /// <value>The id of this payment request.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets allowed payment methods for this request.
        /// </summary>
        /// <value>The payment methods for this request.</value>
        [JsonProperty(PropertyName = "methodData")]
        public IList<PaymentMethodData> MethodData { get; private set; } = new List<PaymentMethodData>();

        /// <summary>
        /// Gets or sets details for this request.
        /// </summary>
        /// <value>The details for this request.</value>
        [JsonProperty(PropertyName = "details")]
        public PaymentDetails Details { get; set; }

        /// <summary>
        /// Gets or sets provides information about the options desired for the
        /// payment request.
        /// </summary>
        /// <value>The options desired for the payment request.</value>
        [JsonProperty(PropertyName = "options")]
        public PaymentOptions Options { get; set; }

        /// <summary>
        /// Gets or sets expiration for this request in ISO 8601 duration
        /// format (e.g., 'P1D').
        /// </summary>
        /// <value>The expiration for the request in ISO 8601 duration format (.e.g 'P1D').</value>
        [JsonProperty(PropertyName = "expires")]
        public string Expires { get; set; }
    }
}
