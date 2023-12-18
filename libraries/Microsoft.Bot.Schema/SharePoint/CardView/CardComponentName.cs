// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Names of the supported Adaptive Card Extension Card View Components.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter), /*camelCase*/ true)]
    public enum CardComponentName
    {
        /// <summary>
        /// Text component.
        /// </summary>
        Text,

        /// <summary>
        /// Card button component.
        /// </summary>
        CardButton,

        /// <summary>
        /// Card bar component.
        /// </summary>
        CardBar,

        /// <summary>
        /// Text input component.
        /// </summary>
        TextInput,

        /// <summary>
        /// Search box component.
        /// </summary>
        SearchBox,

        /// <summary>
        /// Search footer component.
        /// </summary>
        SearchFooter
    }
}
