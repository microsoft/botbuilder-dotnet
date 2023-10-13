// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint GetCardView response object.
    /// </summary>
    public class GetCardViewResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetCardViewResponse"/> class.
        /// </summary>
        /// <param name="templateType">Template type of the card view.</param>
        public GetCardViewResponse(CardViewTemplateType templateType)
        {
            this.TemplateType = templateType;
        }

        /// <summary>
        /// This enum contains the different types of card templates available in the SPFx framework.
        /// </summary>
        public enum CardViewTemplateType
        {
            /// <summary>
            /// Primary text card view
            /// </summary>
            PrimaryTextCardView,

            /// <summary>
            /// Image card view
            /// </summary>
            ImageCardView
        }

        /// <summary>
        /// Gets or Sets the template type of the card view of type <see cref="CardViewTemplateType"/> enum.
        /// </summary>
        /// <value>This value is the template type of the card view response.</value>
        [JsonProperty(PropertyName = "templateType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardViewTemplateType TemplateType { get; set; }

        /// <summary>
        /// Gets or Sets AceData for the card view of type <see cref="AceData"/>.
        /// </summary>
        /// <value>This value is the ace data of the card view response.</value>
        [JsonProperty(PropertyName = "aceData")]
        public AceData AceData { get; set; }

        /// <summary>
        /// Gets or Sets CardViewData of type <see cref="CardViewData"/>.
        /// </summary>
        /// <value>This value is the data of the card view response.</value>
        [JsonProperty(PropertyName = "data")]
        public CardViewData Data { get; set; }
    }
}
