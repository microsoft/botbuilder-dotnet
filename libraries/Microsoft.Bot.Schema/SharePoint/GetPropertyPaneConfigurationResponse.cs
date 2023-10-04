// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint GetQuickView response object.
    /// </summary>
    /// <typeparam name="T">Type for data field.</typeparam>
    public class GetPropertyPaneConfigurationResponse<T> : BaseResponse<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetPropertyPaneConfigurationResponse{T}"/> class.
        /// </summary>
        /// <param name="schemaVersion">Schema version to be used.</param>
        public GetPropertyPaneConfigurationResponse(string schemaVersion)
            : base(schemaVersion)
        {
        }
    }
}
