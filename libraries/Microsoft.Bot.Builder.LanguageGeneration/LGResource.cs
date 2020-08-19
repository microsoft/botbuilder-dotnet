// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG resource entity, contains some core data structure.
    /// </summary>
    public class LGResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LGResource"/> class.
        /// </summary>
        /// <param name="id">Resource id.</param>
        /// <param name="fullName">The full path to the resource on disk.</param>
        /// <param name="content">Resource content.</param>
        public LGResource(string id, string fullName, string content)
        {
            Id = id ?? string.Empty;
            FullName = fullName ?? id ?? string.Empty;
            Content = content;
        }

        /// <summary>
        /// Gets or sets resource id.
        /// </summary>
        /// <value>
        /// Resource id.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the full path to the resource on disk.
        /// </summary>
        /// <value>
        /// The full path to the resource on disk.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets resource content.
        /// </summary>
        /// <value>
        /// Resource content.
        /// </value>
        public string Content { get; set; }
    }
}
