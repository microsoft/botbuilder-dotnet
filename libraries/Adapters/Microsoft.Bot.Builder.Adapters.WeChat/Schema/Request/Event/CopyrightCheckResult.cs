using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event
{
    [XmlRoot("CopyrightCheckResult")]
    public class CopyrightCheckResult
    {
        /// <summary>
        /// Gets or sets Count.
        /// </summary>
        /// <value>
        /// Number of artiles.<
        /// </value>
        [XmlElement(ElementName = "Count")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets ResultList.
        /// </summary>
        /// <value>
        /// Single artile check result.
        /// </value>
        public ResultList ResultList { get; set; }

        /// <summary>
        /// Gets or sets CheckState.
        /// </summary>
        /// <value>
        /// Overall check result
        /// 1: not reprint, could be group sending
        /// 2: repirnt, could be group sending
        /// 3: reprint, could not send.
        /// </value>
        [XmlElement(ElementName = "CheckState")]
        public int CheckState { get; set; }
    }
}
