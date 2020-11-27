// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Enum representing Messaging Extension Attachment Layout types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter), /*camelCase*/ true)]
    public enum MessagingExtensionAttachmentLayoutResponseType
    {
        /// <summary>
        /// List layout type.
        /// </summary>
        List,

        /// <summary>
        /// Grid layout type.
        /// </summary>
        Grid
    }
}
