// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Templates;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class AttachmentPayload
    {
        /// <summary>
        /// Gets or sets the url of the attachment.
        /// </summary>
        /// <value>Url of the attachment.</value>
        [JsonProperty(PropertyName = "url")]
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attachment is reusable or not. Default false.
        /// </summary>
        /// <value>Indicates the reusable condition.</value>
        [JsonProperty(PropertyName = "is_reusable")]
        public bool IsReusable { get; set; }

        /// <summary>
        /// Gets or sets the Id of the attachment (for reusable attachments).
        /// </summary>
        /// <value>The id of the saved attachment.</value>
        [JsonProperty(PropertyName = "attachment_id")]
        public string AttachmentId { get; set; }

        /// <summary>
        /// Gets or sets the type of the template attached.
        /// </summary>
        /// <value>The type of template attached.</value>
        [JsonProperty(PropertyName = "template_type")]
        public string TemplateType { get; set; }

        /// <summary>
        /// Gets or sets the text of the template.
        /// </summary>
        /// <value>The text of the template.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets a list of buttons of the template.
        /// </summary>
        /// <value>The list of buttons of the template.</value>
        [JsonProperty(PropertyName = "buttons")]
        public List<Button> Buttons { get; } = new List<Button>();

        /// <summary>
        /// Gets a list of media elements of the template.
        /// </summary>
        /// <value>The list of media elements of the template.</value>
        [JsonProperty(PropertyName = "elements")]
        public List<Element> Elements { get; } = new List<Element>();

        /// <summary>
        /// Newtonsoft Json method for conditionally serializing IsReusable property.
        /// </summary>
        /// <returns>A boolean with the value.</returns>
        public bool ShouldSerializeIsReusable()
        {
            return IsReusable;
        }

        /// <summary>
        /// Newtonsoft Json method for conditionally serializing Buttons property.
        /// </summary>
        /// <returns>A boolean with the value.</returns>
        public bool ShouldSerializeButtons()
        {
            return Buttons.Count > 0;
        }

        /// <summary>
        /// Newtonsoft Json method for conditionally serializing Elements property.
        /// </summary>
        /// <returns>A boolean with the value.</returns>
        public bool ShouldSerializeElements()
        {
            return Elements.Count > 0;
        }
    }
}
