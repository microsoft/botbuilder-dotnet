// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class NewsResponse : ResponseMessage
    {
        [XmlIgnore]
        public override string MsgType => ResponseMessageTypes.News;

        [XmlElement(ElementName = "MsgType")]
        public XmlCDataSection MsgTypeCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(MsgType);
            }

            set
            {
                MsgType = value.Value;
            }
        }

        [XmlElement(ElementName = "ArticleCount")]
        public int ArticleCount
        {
            get
            {
                return Articles == null ? 0 : Articles.Count;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets Articles.
        /// </summary>
        /// <value>
        /// Article list, can only show up to 10 article.
        /// </value>
        [XmlElement(ElementName = "item")]
        public List<Article> Articles { get; set; }
    }
}
