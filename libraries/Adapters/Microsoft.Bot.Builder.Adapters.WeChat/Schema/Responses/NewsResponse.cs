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
        /// <summary>
        /// Initializes a new instance of the <see cref="NewsResponse"/> class.
        /// </summary>
        public NewsResponse()
            : base()
        {
            Articles = new List<Article>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsResponse"/> class.
        /// </summary>
        /// <param name="senderId">The sender's id.</param>
        /// <param name="recipientId">The recipient id.</param>
        /// <param name="articles">The article list in news response.</param>
        public NewsResponse(string senderId, string recipientId, List<Article> articles)
            : base(senderId, recipientId)
        {
            Articles = articles;
        }

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
