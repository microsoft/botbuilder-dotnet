// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint Confirmation Dialog object.
    /// </summary>
    public class ConfirmationDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationDialog"/> class.
        /// </summary>
        public ConfirmationDialog()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the title of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the title to display.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets the message of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the message to display.</value>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
