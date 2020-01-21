// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Bot.Configuration.Encryption;
    using Newtonsoft.Json;

    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class GenericService : ConnectedService
    {
        public GenericService()
            : base(ServiceTypes.Generic)
        {
        }

        /// <summary>
        /// Gets or sets url for deep link to service.
        /// </summary>
        /// <value>The Url to Service.</value>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets configuration.
        /// </summary>
        /// <value>The service configuration.</value>
        [JsonProperty("configuration")]
        public Dictionary<string, string> Configuration { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc/>
        public override void Encrypt(string secret)
        {
            base.Encrypt(secret);

            if (this.Configuration != null)
            {
                foreach (var key in this.Configuration.Keys.ToArray())
                {
                    var value = this.Configuration[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        this.Configuration[key] = value.Encrypt(secret);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Decrypt(string secret)
        {
            base.Decrypt(secret);

            if (this.Configuration != null)
            {
                foreach (var key in this.Configuration.Keys.ToArray())
                {
                    var value = this.Configuration[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        this.Configuration[key] = value.Decrypt(secret);
                    }
                }
            }
        }
    }
}
