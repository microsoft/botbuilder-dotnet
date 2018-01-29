// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Mention extension 
    /// </summary>
    public partial class Mention : Entity
    {
        partial void CustomInit()
        {
            this.Type = "mention";
        }
    }
}
