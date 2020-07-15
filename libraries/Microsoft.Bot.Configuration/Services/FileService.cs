// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Configuration properties for a connected File service.
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class FileService : ConnectedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
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
