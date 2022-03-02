// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// O365 connector card input for ActionCard action.
    /// </summary>
    public class O365ConnectorCardInputBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardInputBase"/> class.
        /// </summary>
        public O365ConnectorCardInputBase()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardInputBase"/> class.
        /// </summary>
        /// <param name="type">Input type name. Possible values include:
        /// 'textInput', 'dateInput', 'multichoiceInput'.</param>
        /// <param name="id">Input Id. It must be unique per entire O365 connector card.</param>
        /// <param name="isRequired">Define if this input is a required field. Default value is false.</param>
        /// <param name="title">Input title that will be shown as the placeholder.</param>
        /// <param name="value">Default value for this input field.</param>
        public O365ConnectorCardInputBase(string type = default, string id = default, bool? isRequired = default, string title = default, string value = default)
        {
            Type = type;
            Id = id;
            IsRequired = isRequired;
            Title = title;
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets input type name. Possible values include: 'textInput',
        /// 'dateInput', 'multichoiceInput'.
        /// </summary>
        /// <value>The input type name.</value>
        [JsonPropertyName("@type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets input ID. It must be unique per entire O365 connector
        /// card.
        /// </summary>
        /// <value>The input ID.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets define if this input is a required field. Default
        /// value is false.
        /// </summary>
        /// <value>Boolean indicating if this input is a required field.</value>
        [JsonPropertyName("isRequired")]
        public bool? IsRequired { get; set; }

        /// <summary>
        /// Gets or sets input title that will be shown as the placeholder.
        /// </summary>
        /// <value>The input title that will be shown as the placeholder.</value>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets default value for this input field.
        /// </summary>
        /// <value>The default value for this input field.</value>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
