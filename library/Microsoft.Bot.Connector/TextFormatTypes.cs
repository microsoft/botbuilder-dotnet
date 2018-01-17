using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public class TextFormatTypes
    {
        /// <summary>
        /// Default- interpret text fields as markdown
        /// </summary>
        public const string Markdown = "markdown";

        /// <summary>
        /// Plain text (do not interpret as anything)
        /// </summary>
        public const string Plain = "plain";

        /// <summary>
        /// B, I, S, U, A NOTE: Only supported on Skype for now
        /// </summary>
        public const string Xml = "xml";
    }
}