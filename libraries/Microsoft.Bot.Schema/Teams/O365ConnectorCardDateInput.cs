// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card date input.
    /// </summary>
    public partial class O365ConnectorCardDateInput : O365ConnectorCardInputBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardDateInput"/> class.
        /// </summary>
        public O365ConnectorCardDateInput()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardDateInput"/> class.
        /// </summary>
        /// <param name="type">Input type name. Possible values include:
        /// 'textInput', 'dateInput', 'multichoiceInput'.</param>
        /// <param name="id">Input Id. It must be unique per entire O365
        /// connector card.</param>
        /// <param name="isRequired">Define if this input is a required field.
        /// Default value is false.</param>
        /// <param name="title">Input title that will be shown as the
        /// placeholder.</param>
        /// <param name="value">Default value for this input field.</param>
        /// <param name="includeTime">Include time input field. Default value
        /// is false (date only).</param>
        public O365ConnectorCardDateInput(string type = default, string id = default, bool? isRequired = default, string title = default, string value = default, bool? includeTime = default)
            : base(type, id, isRequired, title, value)
        {
            IncludeTime = includeTime;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets include time input field. Default value  is false
        /// (date only).
        /// </summary>
        /// <value>Boolean indicating whether to include time.</value>
        [JsonProperty(PropertyName = "includeTime")]
        public bool? IncludeTime { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
