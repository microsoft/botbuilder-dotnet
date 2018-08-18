// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using Newtonsoft.Json;

    public class ConnectedService
    {
        public ConnectedService(string type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets or sets type of the service.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets user friendly name of the service.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets unique id for the service.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Encrypt properties on this service.
        /// </summary>
        /// <param name="secret"> secret to use to decrypt the keys in this service.</param>
        public virtual void Decrypt(string secret)
        {
        }

        /// <summary>
        /// Decrypt properties on this service
        /// </summary>
        /// <param name="secret">secret to use to encrypt the keys in this service.</param>
        public virtual void Encrypt(string secret)
        {
        }
    }
}
