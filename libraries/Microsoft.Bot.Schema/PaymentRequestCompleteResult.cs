// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Result from a completed payment request.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentRequestCompleteResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequestCompleteResult"/> class.
        /// </summary>
        /// <param name="result">Result of the payment request completion.</param>
        public PaymentRequestCompleteResult(string result = default)
        {
            Result = result;
        }

        /// <summary>
        /// Gets or sets result of the payment request completion.
        /// </summary>
        /// <value>The result of the payment request completion.</value>
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }
    }
}
