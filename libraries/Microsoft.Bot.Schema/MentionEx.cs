// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Mention information (entity type: "mention")
    /// </summary>
    public partial class Mention : Entity
    {

        partial void CustomInit()
        {
            this.Type = "mention";
        }
    }
}
