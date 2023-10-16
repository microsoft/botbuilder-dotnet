// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Media type for SelectMediaAction.
    /// </summary>
    [Flags]
    public enum MediaTypes
    {
        /// <summary>
        /// Image media type.
        /// </summary>
        Image = 1,

        /// <summary>
        /// Audio media type.
        /// </summary>
        Audio = 4,

        /// <summary>
        /// Document media type.
        /// </summary>
        Document = 8
    }

    /// <summary>
    /// SharePoint action button parameters.
    /// </summary>
    public class ActionParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionParameters"/> class.
        /// </summary>
        public ActionParameters()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the view of type <see cref="string"/>.
        /// </summary>
        /// <remarks>Use this property if you are working with QuickView action type.</remarks>
        /// <value>This value is the view of the action parameter.</value>
        [JsonProperty(PropertyName = "view")]
        public string View { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates whether the target is a Teams deep link.
        /// </summary>
        /// <remarks>Use this property if you are working with ExternalLink action type.</remarks>
        /// <value>Indicates whether this is a Teams Deep Link.</value>
        [JsonProperty(PropertyName = "isTeamsDeepLink")]
        public bool? IsTeamsDeepLink { get; set; }

        /// <summary>
        /// Gets or sets the target URL for the ExternalLink action.
        /// </summary>
        /// <remarks>Use this property if you are working with ExternalLink action type.</remarks>
        /// <value>The values is the URL.</value>
        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }

        /// <summary>
        /// Gets key-value pair properties that can be defined for Submit and Execute actions.
        /// </summary>
        /// <remarks>Use this property if you are working with Submit or Execute action type.</remarks>
        /// <value>key-value pairs for Submit or Execute actions.</value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> SubmitParameters { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the specific media type that should be selected.
        /// </summary>
        /// <remarks>Use this property if you are working with VivaAction.SelectMedia action type.</remarks>
        /// <value>The specific media type that should be selected.</value>
        [JsonProperty(PropertyName = "mediaType")]
        public MediaTypes? MediaType { get; set; }

        /// <summary>
        /// Gets or sets a flag to specify if multiple files can be selected.
        /// </summary>
        /// <remarks>Use this property if you are working with VivaAction.SelectMedia action type.</remarks>
        /// <value>Specifies if multiple files can be selected.</value>
        [JsonProperty(PropertyName = "allowMultipleCapture")]
        public bool? AllowMultipleCapture { get; set; }

        /// <summary>
        /// Gets or sets maximum file size that can be uplaoded.
        /// </summary>
        /// <remarks>Use this property if you are working with VivaAction.SelectMedia action type.</remarks>
        /// <value>Max file size.</value>
        [JsonProperty(PropertyName = "maxSizePerFile")]
        public int MaxSizePerFile { get; set; }

        /// <summary>
        /// Gets file formats supported for upload.
        /// </summary>
        /// <remarks>Use this property if you are working with VivaAction.SelectMedia action type.</remarks>
        /// <value>File formats supported for upload.</value>
        [JsonProperty(PropertyName = "supportedFileFormats")]
        public IList<string> SupportedFileFormats { get; } = new List<string>(); 
    }
}
