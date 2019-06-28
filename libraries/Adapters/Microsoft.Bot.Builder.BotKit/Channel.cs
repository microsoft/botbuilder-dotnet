// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.BotKit
{
    /// <summary>
    /// Data related to the Channel.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="id">Id of the channel.</param>
        /// <param name="name">Name of the channel.</param>
        public Channel(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        /// <summary>
        /// Gets or Sets the Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets the Name.
        /// </summary>
        public string Name { get; set; }
    }
}
