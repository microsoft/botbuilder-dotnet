// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class DispatchService : LuisService
    {
        public DispatchService()
        {
            this.Type = ServiceTypes.Dispatch;
        }

        /// <summary>
        /// Gets or sets the service IDs to include in the dispatch model.
        /// </summary>
        /// <value>The list of service Ids.</value>
        [JsonProperty("serviceIds")]
        public List<string> ServiceIds { get; set; } = new List<string>();
    }
}
