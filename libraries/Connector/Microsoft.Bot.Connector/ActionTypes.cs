using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public class ActionTypes
    {
        /// <summary>
        /// Client will open given url in the built-in browser.
        /// </summary>
        public const string OpenUrl = "openUrl";

        /// <summary>
        /// Client will post message to bot, so all other participants will see that was posted to the bot and who posted this.
        /// </summary>
        public const string ImBack = "imBack";

        /// <summary>
        /// Client will post message to bot privately, so other participants inside conversation will not see that was posted. 
        /// </summary>
        public const string PostBack = "postBack";

        /// <summary>
        /// playback audio container referenced by url
        /// </summary>
        public const string PlayAudio = "playAudio";

        /// <summary>
        /// playback video container referenced by url
        /// </summary>
        public const string PlayVideo = "playVideo";

        /// <summary>
        /// show image referenced by url
        /// </summary>
        public const string ShowImage = "showImage";

        /// <summary>
        /// download file referenced by url
        /// </summary>
        public const string DownloadFile = "downloadFile";

        /// <summary>
        /// Signin button
        /// </summary>
        public const string Signin = "signin";

        /// <summary>
        /// Post message to bot
        /// </summary>
        public const string MessageBack = "messageBack";
    }
}
