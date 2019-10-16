// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using Newtonsoft.Json;

    public class BotService : AzureService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotService"/> class.
        /// </summary>
        public BotService()
            : base(ServiceTypes.Bot)
        {
        }

        /// <summary>
        /// Gets or sets appId for the bot.
        /// </summary>
        /// <value>The App Id.</value>
        [JsonProperty("appId")]
        public string AppId { get; set; }
    }
}
