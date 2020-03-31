// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Mention extensions.
    /// </summary>
    public partial class Mention : Entity
    {
        partial void CustomInit()
        {
            Type = EntityTypes.Mention;
        }
    }
}
