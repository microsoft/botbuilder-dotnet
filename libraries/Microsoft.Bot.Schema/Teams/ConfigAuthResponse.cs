// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Envelope for Config Auth Response.
    /// </summary>
    public partial class ConfigAuthResponse : ConfigResponse<BotConfigAuth>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigAuthResponse"/> class.
        /// </summary>
        public ConfigAuthResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
