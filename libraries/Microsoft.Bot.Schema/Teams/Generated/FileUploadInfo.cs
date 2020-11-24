// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Information about the file to be uploaded.
    /// </summary>
    public partial class FileUploadInfo
    {
        /// <summary>
        /// Initializes a new instance of the FileUploadInfo class.
        /// </summary>
        public FileUploadInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the FileUploadInfo class.
        /// </summary>
        /// <param name="name">Name of the file.</param>
        /// <param name="uploadUrl">URL to an upload session that the bot can
        /// use to set the file contents.</param>
        /// <param name="contentUrl">URL to file.</param>
        /// <param name="uniqueId">ID that uniquely identifies the
        /// file.</param>
        /// <param name="fileType">Type of the file.</param>
        public FileUploadInfo(string name = default(string), string uploadUrl = default(string), string contentUrl = default(string), string uniqueId = default(string), string fileType = default(string))
        {
            Name = name;
            UploadUrl = uploadUrl;
            ContentUrl = contentUrl;
            UniqueId = uniqueId;
            FileType = fileType;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets name of the file.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets URL to an upload session that the bot can use to set
        /// the file contents.
        /// </summary>
        [JsonProperty(PropertyName = "uploadUrl")]
        public string UploadUrl { get; set; }

        /// <summary>
        /// Gets or sets URL to file.
        /// </summary>
        [JsonProperty(PropertyName = "contentUrl")]
        public string ContentUrl { get; set; }

        /// <summary>
        /// Gets or sets ID that uniquely identifies the file.
        /// </summary>
        [JsonProperty(PropertyName = "uniqueId")]
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or sets type of the file.
        /// </summary>
        [JsonProperty(PropertyName = "fileType")]
        public string FileType { get; set; }

    }
}
