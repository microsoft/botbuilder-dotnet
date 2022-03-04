﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// File download info attachment.
    /// </summary>
    public class FileDownloadInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileDownloadInfo"/> class.
        /// </summary>
        /// <param name="downloadUrl">File download url.</param>
        /// <param name="uniqueId">Unique Id for the file.</param>
        /// <param name="fileType">Type of file.</param>
        /// <param name="etag">ETag for the file.</param>
        public FileDownloadInfo(string downloadUrl = default, string uniqueId = default, string fileType = default, object etag = default)
        {
            DownloadUrl = downloadUrl;
            UniqueId = uniqueId;
            FileType = fileType;
            Etag = etag;
        }

        /// <summary>
        /// Gets or sets file download URL.
        /// </summary>
        /// <value>The file download URL.</value>
        [JsonProperty(PropertyName = "downloadUrl")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string DownloadUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets unique Id for the file.
        /// </summary>
        /// <value>The unique ID for the file.</value>
        [JsonProperty(PropertyName = "uniqueId")]
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or sets type of file.
        /// </summary>
        /// <value>The type of the file.</value>
        [JsonProperty(PropertyName = "fileType")]
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets eTag for the file.
        /// </summary>
        /// <value>The eTag for the file.</value>
        [JsonProperty(PropertyName = "etag")]
        public object Etag { get; set; }
    }
}
