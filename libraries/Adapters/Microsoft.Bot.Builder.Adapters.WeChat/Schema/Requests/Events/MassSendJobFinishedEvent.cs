// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    /// <summary>
    /// Group message send finish event.
    /// </summary>
    [XmlRoot("xml")]
    public class MassSendJobFinishedEvent : RequestEvent
    {
        /// <summary>
        /// Gets event EventType: MASSSENDJOBFINISH.
        /// </summary>
        /// <value>
        /// EventType: MASSSENDJOBFINISH.
        /// </value>
        public override string EventType => EventTypes.MassSendJobFinished;

        /// <summary>
        /// Gets or sets status code.
        /// </summary>
        /// <value>
        /// Status code.
        /// </value>
        [XmlElement(ElementName = "Status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets TotalCount.
        /// </summary>
        /// <value>
        /// Number of subscribers under group_id or openid_list.
        /// </value>
        [XmlElement(ElementName = "TotalCount")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets FilterCount.
        /// </summary>
        /// <value>
        /// Number of subscribers that message is going to be sent
        /// FilterCount = SentCount + ErrorCount.
        /// </value>
        [XmlElement(ElementName = "FilterCount")]
        public int FilterCount { get; set; }

        /// <summary>
        /// Gets or sets SentCount.
        /// </summary>
        /// <value>
        /// Number of subscribers that message is successfully sent.
        /// </value>
        [XmlElement(ElementName = "SentCount")]
        public int SentCount { get; set; }

        /// <summary>
        /// Gets or sets ErrorCount.
        /// </summary>
        /// <value>
        /// Number of subscribers that message is not sent.
        /// </value>
        [XmlElement(ElementName = "ErrorCount")]
        public int ErrorCount { get; set; }

        /// <summary>
        /// Gets or sets MsgID.
        /// </summary>
        /// <value>
        /// Group message id.
        /// </value>
        [XmlElement(ElementName = "MsgID")]
        public long MsgID { get; set; }

        // [XmlElement(ElementName = "CopyrightCheckResult")]
        public CopyrightCheckResult CopyrightCheckResult { get; set; }
    }
}
