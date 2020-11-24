// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// File info card.
    /// </summary>
    public partial class FileInfoCard
    {
        /// <summary>
        /// Initializes a new instance of the FileInfoCard class.
        /// </summary>
        public FileInfoCard()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the FileInfoCard class.
        /// </summary>
        /// <param name="uniqueId">Unique Id for the file.</param>
        /// <param name="fileType">Type of file.</param>
        /// <param name="etag">ETag for the file.</param>
        public FileInfoCard(string uniqueId = default(string), string fileType = default(string), object etag = default(object))
        {
            UniqueId = uniqueId;
            FileType = fileType;
            Etag = etag;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets unique Id for the file.
        /// </summary>
        [JsonProperty(PropertyName = "uniqueId")]
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or sets type of file.
        /// </summary>
        [JsonProperty(PropertyName = "fileType")]
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets eTag for the file.
        /// </summary>
        [JsonProperty(PropertyName = "etag")]
        public object Etag { get; set; }

    }
}
