// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary>
    /// Type of Service invoked from the bot.
    /// Default is QnAMaker.
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// Service type is Language with Custom Question Answering enabled.
        /// </summary>
        Language,

        /// <summary>
        /// Service type is QnAMaker.
        /// </summary>
        QnAMaker
    }
}
