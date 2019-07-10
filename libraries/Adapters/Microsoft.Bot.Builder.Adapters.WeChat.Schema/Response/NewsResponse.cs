using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public class NewsResponse : ResponseMessage
    {
        public override ResponseMessageType MsgType => ResponseMessageType.News;

        /// <summary>
        /// Gets or sets Articles.
        /// </summary>
        /// <value>
        /// Article list, can only show up to 10 article.
        /// </value>
        public List<Article> Articles { get; set; }

        public int ArticleCount
        {
            get
            {
                return Articles == null ? 0 : Articles.Count;
            }
        }
    }
}
