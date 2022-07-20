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
        /// Helper method useful for determining if a message has been read. This method
        /// converts the strings to longs. If the compareMessageId is less than or equal to
        /// the lastReadMessageId, then the message has been read.
        /// </summary>
        /// <param name="compareMessageId">The id of the message to compare.</param>
        /// <param name="lastReadMessageId">The id of the last message read by the user.</param>
        /// <returns>True if the compareMessageId is less than or equal to the lastReadMessageId.</returns>
        public static bool IsMessageRead(string compareMessageId, string lastReadMessageId)
        {
            if (string.IsNullOrEmpty(compareMessageId) || string.IsNullOrEmpty(lastReadMessageId))
            {
                return false;
            }

            if (long.TryParse(compareMessageId, out long compareMessageIdLong)
                && long.TryParse(lastReadMessageId, out long lastReadMessageIdLong))
            {
                // if compareMessageId is smaller than lastReadMessageId, it means the user read the bot's message. 
                return (compareMessageIdLong.CompareTo(lastReadMessageIdLong) <= 0) ? true : false;
            }

            return false;
        }

        /// <summary>
        /// Helper method useful for determining if a message has been read.
        /// If the compareMessageId is less than or equal to the LastReadMessageId,
        /// then the message has been read.
        /// </summary>
        /// <param name="compareMessageId">The id of the message to compare.</param>
        /// <returns>True if the compareMessageId is less than or equal to the lastReadMessageId.</returns>
        public bool IsMessageRead(string compareMessageId)
        {
            return IsMessageRead(compareMessageId, LastReadMessageId);
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
