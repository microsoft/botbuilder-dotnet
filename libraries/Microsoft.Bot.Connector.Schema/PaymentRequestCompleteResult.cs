// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Result from a completed payment request.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentRequestCompleteResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequestCompleteResult"/> class.
        /// class.
        /// </summary>
        public PaymentRequestCompleteResult()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequestCompleteResult"/> class.
        /// </summary>
        /// <param name="result">Result of the payment request completion.</param>
        public PaymentRequestCompleteResult(string result = default)
        {
            Result = result;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets result of the payment request completion.
        /// </summary>
        /// <value>The result of the payment request completion.</value>
        [JsonPropertyName("result")]
        public string Result { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
