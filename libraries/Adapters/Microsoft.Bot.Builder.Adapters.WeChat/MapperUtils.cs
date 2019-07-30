using Microsoft.Bot.Builder.Adapters.WeChat.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public static class MapperUtils
    {
        public const string WordBreak = "  ";

        public static string AddLine(this string body, string newText)
        {
            if (string.IsNullOrEmpty(newText))
            {
                return body;
            }

            if (string.IsNullOrEmpty(body))
            {
                return newText;
            }

            return body + Constants.NewLine + newText;
        }

        public static string AddText(this string body, string newText, string wordBreak = null)
        {
            if (string.IsNullOrEmpty(newText))
            {
                return body;
            }

            if (string.IsNullOrEmpty(body))
            {
                return newText;
            }

            if (string.IsNullOrEmpty(wordBreak))
            {
                wordBreak = WordBreak;
            }

            return $"{body}{wordBreak}{newText}";
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
    }
}
