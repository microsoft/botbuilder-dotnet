// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// GeoCoordinates extensions
    /// </summary>
    public partial class Place : Entity
    {
        partial void CustomInit()
        {
            Type = "Place";
        }
    }
}
