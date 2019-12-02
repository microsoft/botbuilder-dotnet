// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Action type names for payment requests.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentRequest
    {
        /// <summary>
        /// Action type for Payment action.
        /// </summary>
        public const string PaymentActionType = "payment";

        /// <summary>
        /// Content-type for Payment card.
        /// </summary>
        public const string PaymentContentType = "application/vnd.microsoft.card.payment";
    }
}
