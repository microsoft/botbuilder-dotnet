// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Classic.ConnectorEx
{
    /// <summary>
    /// Card representing a keyboard
    /// </summary>
    /// <remarks>
    /// This will be mapped to <see cref="HeroCard"/> for all channels 
    /// except Facebook. For Facebook, <see cref="KeyboardCardMapper"/> maps it 
    /// to <see cref="FacebookQuickReply"/>
    /// </remarks>
    [System.Obsolete("Please use SuggestedActions instead.")]
    public partial class KeyboardCard
    {
        /// <summary>
        /// Content type of keyboard card for <see cref="Attachment.ContentType"/>.
        /// </summary>
        public const string ContentType = "application/vnd.microsoft.card.keyboard";

        /// <summary>
        /// Constructs an instance of the keyboard card.
        /// </summary>
        /// <param name="text"> The keyboard text.</param>
        /// <param name="buttons"> The buttons in keyboard.</param>
        public KeyboardCard(string text, IList<CardAction> buttons)
        {
            Text = text;
            Buttons = buttons;
        }

        /// <summary>
        /// The keyboard text. 
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// The buttons in the keyboard.
        /// </summary>
        [JsonProperty(PropertyName = "buttons")]
        public IList<CardAction> Buttons { get; set; }
    }

    /// <summary>
    /// Facebook quick reply. See https://developers.facebook.com/docs/messenger-platform/send-api-reference/quick-replies. 
    /// </summary>
    public sealed class FacebookQuickReply
    {
        public sealed class ContentTypes
        {
            public const string Text = "text";
            public const string Location = "location";
        }

        public FacebookQuickReply(string contentType, string title, string payload, string image = default(string))
        {
            ContentType = contentType;
            Title = title;
            Payload = payload;
            Image = image;
        }

        [JsonProperty(PropertyName = "content_type")]
        public string ContentType { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "payload")]
        public string Payload { get; set; }

        [JsonProperty(PropertyName = "image_url")]
        public string Image { get; set; }
    }

    /// <summary>
    /// Facebook message format for quick reply.
    /// </summary>
    public sealed class FacebookMessage
    {
        public FacebookMessage(string text, IList<FacebookQuickReply> quickReplies = default(IList<FacebookQuickReply>))
        {
            Text = text;
            QuickReplies = quickReplies;
        }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "quick_replies")]
        public IList<FacebookQuickReply> QuickReplies { get; set; }
    }

    /// <summary>
    /// Extension methods for <see cref="KeyboardCard"/>
    /// </summary>
    public static partial class KeyboardCardEx
    {
#pragma warning disable CS0618
        public static Attachment ToAttachment(this KeyboardCard keyboard)
        {
            return new Attachment
            {
                ContentType = KeyboardCard.ContentType,
                Content = keyboard
            };
        }

        /// <summary>
        /// Maps a <see cref="KeyboardCard"/> to a <see cref="HeroCard"/>
        /// </summary>
        /// <param name="keyboard"> The keyboard card.</param>
        public static HeroCard ToHeroCard(this KeyboardCard keyboard)
        {
            return new HeroCard(text: keyboard.Text, buttons: keyboard.Buttons);
        }

        /// <summary>
        /// Maps a <see cref="KeyboardCard"/> to a <see cref="FacebookMessage"/>
        /// </summary>
        /// <param name="keyboard"> The keyboard card.</param>
        public static FacebookMessage ToFacebookMessage(this KeyboardCard keyboard)
        {
            return new FacebookMessage(text: keyboard.Text, quickReplies: keyboard.Buttons.Select(b => b.ToFacebookQuickReply()).ToList());
        }
#pragma warning restore CS0618

        internal static FacebookQuickReply ToFacebookQuickReply(this CardAction button)
        {
            return new FacebookQuickReply(contentType: FacebookQuickReply.ContentTypes.Text, title: button.Title, payload: (string)button.Value, image: button.Image);
        }
    }
}
