using Microsoft.Bot.Builder.Adapters.WeChat.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public static class MapperUtils
    {
        public static string AddLine(this string body, string newtext)
        {
            if (string.IsNullOrEmpty(newtext))
            {
                return body;
            }

            if (string.IsNullOrEmpty(body))
            {
                return newtext;
            }

            return $"{body}\n\n{newtext}";
        }

        public static string AddText(this string body, string newtext, string wordbreak = "  ")
        {
            if (string.IsNullOrEmpty(newtext))
            {
                return body;
            }

            if (string.IsNullOrEmpty(body))
            {
                return newtext;
            }

            return $"{body}{wordbreak}{newtext}";
        }

        public static string GetMediaExtension(string link, string mimeType, string type)
        {
            var ext = MimeTypesMap.GetExtension(mimeType);
            ext = ext ?? MimeTypesMap.GetExtension(ext);
            if (string.IsNullOrEmpty(ext))
            {
                switch (type)
                {
                    case UploadMediaType.Image:
                    case UploadMediaType.Thumb:
                        ext = "jpg";
                        break;
                    case UploadMediaType.Video:
                        ext = "mp4";
                        break;
                    case UploadMediaType.Voice:
                        ext = "mp3";
                        break;
                }
            }

            return $".{ext}";
        }

        /// <summary>
        /// Render an Adaptive Card to AttachmentData in memory
        /// </summary>

        // public static async Task<AttachmentData> RenderAdaptiveCardToAttachmentDataAsync(
        //    AdaptiveCard card,
        //    AdaptiveHostConfig hostConfig,
        //    string stylePath,
        //    int width = 400)
        // {
        //    var renderer = new AdaptiveCardRenderer(hostConfig)
        //    {
        //        ResourcesPath = stylePath
        //    };

        // var renderedCard = await renderer.RenderCardToImageAsync(card, createStaThread: true, width: width).ConfigureAwait(false);
        //    var imageStream = renderedCard.ImageStream;

        // return new AttachmentData()
        //    {
        //        Type = "image/png",
        //        OriginalBase64 = ((MemoryStream)imageStream).ToArray()
        //    };
        // }
    }
}
