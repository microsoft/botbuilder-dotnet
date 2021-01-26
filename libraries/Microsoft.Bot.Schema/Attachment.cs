// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An attachment within an activity.
    /// </summary>
    public class Attachment
    {
        /// <summary> Initializes a new instance of the <see cref="Attachment"/> class. </summary>
        public Attachment()
        {
        }

        /// <summary> Initializes a new instance of the <see cref="Attachment"/> class. </summary>
        /// <param name="contentType">mimetype/Contenttype for the file.</param>
        /// <param name="contentUrl">Content Url.</param>
        /// <param name="content">Embedded content.</param>
        /// <param name="name">(OPTIONAL) The name of the attachment.</param>
        /// <param name="thumbnailUrl">(OPTIONAL) Thumbnail associated with
        /// attachment.</param>
        public Attachment(string contentType = default(string), string contentUrl = default(string), object content = default(object), string name = default(string), string thumbnailUrl = default(string))
        {
            ContentType = contentType;
            ContentUrl = contentUrl;
            Content = content;
            Name = name;
            ThumbnailUrl = thumbnailUrl;
        }

        /// <summary>
        /// Gets or sets mimetype/Contenttype for the file.
        /// </summary>
        /// <value>The content type.</value>
        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets content Url.
        /// </summary>
        /// <value>The content URL.</value>
        [JsonProperty(PropertyName = "contentUrl")]
#pragma warning disable CA1056 // Uri properties should not be strings (we can't change this without breaking compat).
        public string ContentUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets embedded content.
        /// </summary>
        /// <value>The embedded content.</value>
        [JsonProperty(PropertyName = "content")]
        public object Content { get; set; }

        /// <summary>
        /// Gets or sets (OPTIONAL) The name of the attachment.
        /// </summary>
        /// <value>The name of the attachment.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets properties that are not otherwise defined by the <see cref="Attachment"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only  (we can't change this without breaking binary compat)
        public JObject Properties { get; set; } = new JObject();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets (OPTIONAL) Thumbnail associated with attachment.
        /// </summary>
        /// <value>The thumbnail URL associated with attachment.</value>
        [JsonProperty(PropertyName = "thumbnailUrl")]
#pragma warning disable CA1056 // Uri properties should not be strings (we can't change this without breaking compat).
        public string ThumbnailUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
    }
}
