// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Envelope for Config Task Response.
    /// </summary>
    public partial class ConfigTaskResponse : ConfigResponse<TaskModuleResponseBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigTaskResponse"/> class.
        /// </summary>
        public ConfigTaskResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
