// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    public partial class PaymentRequest
    {
        /// <summary>
        /// Action type for Payment action
        /// </summary>
        public const string PaymentActionType = "payment";

        /// <summary>
        /// Content-type for Payment card
        /// </summary>
        public const string PaymentContentType = "application/vnd.microsoft.card.payment";
    }
}
