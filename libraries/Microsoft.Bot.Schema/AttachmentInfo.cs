// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Metadata for an attachment
    /// </summary>
    public partial class AttachmentInfo
    {
        /// <summary>
        /// Initializes a new instance of the AttachmentInfo class.
        /// </summary>
        public AttachmentInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AttachmentInfo class.
        /// </summary>
        /// <param name="name">Name of the attachment</param>
        /// <param name="type">ContentType of the attachment</param>
        /// <param name="views">attachment views</param>
        public AttachmentInfo(string name = default(string), string type = default(string), IList<AttachmentView> views = default(IList<AttachmentView>))
        {
            Name = name;
            Type = type;
            Views = views;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets name of the attachment
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets contentType of the attachment
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets attachment views
        /// </summary>
        [JsonProperty(PropertyName = "views")]
        public IList<AttachmentView> Views { get; set; }

    }
}
