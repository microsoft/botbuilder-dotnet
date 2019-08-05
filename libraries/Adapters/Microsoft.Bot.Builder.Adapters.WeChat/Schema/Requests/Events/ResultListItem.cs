// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    [XmlRoot("item")]
    public class ResultListItem
    {
        /// <summary>
        /// Gets or sets ArticleIdx.
        /// </summary>
        /// <value>
        /// Index of group sending artiles, start from 1.
        /// </value>
        [XmlElement(ElementName = "ArticleIdx")]
        public int ArticleIdx { get; set; }

        /// <summary>
        /// Gets or sets UserDeclareState.
        /// </summary>
        /// <value>
        /// User declares state of artile.
        /// </value>
        [XmlElement(ElementName = "UserDeclareState")]
        public int UserDeclareState { get; set; }

        /// <summary>
        /// Gets or sets AuditState.
        /// </summary>
        /// <value>
        /// State of system check.
        /// </value>
        [XmlElement(ElementName = "AuditState")]
        public int AuditState { get; set; }

        /// <summary>
        /// Gets or sets OriginalArticleUrl.
        /// </summary>
        /// <value>
        /// Url of Similar orginal artile.
        /// </value>
        [XmlElement(ElementName = "OriginalArticleUrl")]
        public string OriginalArticleUrl { get; set; }

        /// <summary>
        /// Gets or sets OriginalArticleType.
        /// </summary>
        /// <value>
        /// Type of Similar original artile.
        /// </value>
        [XmlElement(ElementName = "OriginalArticleType")]
        public int OriginalArticleType { get; set; }

        /// <summary>
        /// Gets or sets CanReprint.
        /// </summary>
        /// <value>
        /// Reprint or not.
        /// </value>
        [XmlElement(ElementName = "CanReprint")]
        public int CanReprint { get; set; }

        /// <summary>
        /// Gets or sets NeedReplaceContent.
        /// </summary>
        /// <value>
        /// Replace by original content or not.
        /// </value>
        [XmlElement(ElementName = "NeedReplaceContent")]
        public int NeedReplaceContent { get; set; }

        /// <summary>
        /// Gets or sets NeedShowReprintSource.
        /// </summary>
        /// <value>
        /// Show reprint source or not.
        /// </value>
        [XmlElement(ElementName = "NeedShowReprintSource")]
        public int NeedShowReprintSource { get; set; }
    }
}
