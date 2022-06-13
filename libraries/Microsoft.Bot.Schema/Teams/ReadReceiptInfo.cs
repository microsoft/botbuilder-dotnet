// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// General information about a read receipt.
    /// </summary>
    public partial class ReadReceiptInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadReceiptInfo"/> class.
        /// </summary>
        public ReadReceiptInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadReceiptInfo"/> class.
        /// </summary>
        /// <param name="lastReadMessageId">The id of the last read message.</param>
        public ReadReceiptInfo(string lastReadMessageId)
        {
            LastReadMessageId = lastReadMessageId;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the id of the last read message.
        /// </summary>
        /// <value>
        /// The id of the last read message.
        /// </value>
        [JsonProperty(PropertyName = "lastReadMessageId")]
        public string LastReadMessageId { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
