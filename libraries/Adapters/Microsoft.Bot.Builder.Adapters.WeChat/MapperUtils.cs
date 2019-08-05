// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.MarkedNet;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public static class MapperUtils
    {
        /// <summary>
        /// Default word break when join two word.
        /// </summary>
        public const string WordBreak = "  ";

        /// <summary>
        /// New line string.
        /// </summary>
        public const string NewLine = "\r\n";

        /// <summary>
        /// Add new line and append new text.
        /// </summary>
        /// <param name="text">The text itself.</param>
        /// <param name="newText">Text need to be attached.</param>
        /// <returns>Combined new text string.</returns>
        public static string AddLine(this string text, string newText)
        {
            if (string.IsNullOrEmpty(newText))
            {
                return text;
            }

            if (string.IsNullOrEmpty(text))
            {
                return newText;
            }

            return text + NewLine + newText;
        }

        /// <summary>
        /// Add text break and append the new text.
        /// </summary>
        /// <param name="text">The text itself.</param>
        /// <param name="newText">Text need to be attached.</param>
        /// <returns>Combined new text string.</returns>
        public static string AddText(this string text, string newText)
        {
            if (string.IsNullOrEmpty(newText))
            {
                return text;
            }

            if (string.IsNullOrEmpty(text))
            {
                return newText;
            }

            return text + WordBreak + newText;
        }

        public static string GetMediaExtension(string link, string mimeType, string type)
        {
            var ext = MimeTypesMap.GetExtension(mimeType);
            if (ext == MimeTypesMap.DefaultExtension)
            {
                mimeType = MimeTypesMap.GetMimeType(link);
                ext = MimeTypesMap.GetExtension(mimeType);
            }

            if (ext == MimeTypesMap.DefaultExtension)
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
        /// Get Marded instance to help parse markdown text.
        /// </summary>
        /// <returns>Marked instance.</returns>
        public static Marked GetMarked()
        {
            var marked = new Marked
            {
                Options =
                {
                    Sanitize = false,
                    Mangle = false,
                },
            };
            marked.Options.Renderer = new TextMarkdownRenderer(marked.Options);
            return marked;
        }
    }
}
