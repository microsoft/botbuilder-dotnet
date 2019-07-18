// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using Newtonsoft.Json;

    public class FileService : ConnectedService
    {
        public FileService()
            : base(ServiceTypes.File)
        {
        }

        /// <summary>
        /// Gets or sets file path.
        /// </summary>
        /// <value>The Path for the file.</value>
        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
