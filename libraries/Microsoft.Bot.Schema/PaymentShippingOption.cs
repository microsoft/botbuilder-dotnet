// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a shipping option.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentShippingOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentShippingOption"/> class.
        /// </summary>
        public PaymentShippingOption()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentShippingOption"/> class.
        /// </summary>
        /// <param name="id">String identifier used to reference this
        /// PaymentShippingOption.</param>
        /// <param name="label">Human-readable description of the item.</param>
        /// <param name="amount">Contains the monetary amount for the
        /// item.</param>
        /// <param name="selected">Indicates whether this is the default
        /// selected PaymentShippingOption.</param>
        public PaymentShippingOption(string id = default(string), string label = default(string), PaymentCurrencyAmount amount = default(PaymentCurrencyAmount), bool? selected = default(bool?))
        {
            Id = id;
            Label = label;
            Amount = amount;
            Selected = selected;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets string identifier used to reference this
        /// PaymentShippingOption.
        /// </summary>
        /// <value>The <see cref="PaymentShippingOption"/> ID.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets human-readable description of the item.
        /// </summary>
        /// <value>The human-readable description of the item.</value>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets contains the monetary amount for the item.
        /// </summary>
        /// <value>The monetary amount for the item.</value>
        [JsonProperty(PropertyName = "amount")]
        public PaymentCurrencyAmount Amount { get; set; }

        /// <summary>
        /// Gets or sets indicates whether this is the default selected <see cref="PaymentShippingOption"/>.
        /// </summary>
        /// <value>Boolean indivating if this is the default selected <see cref="PaymentShippingOption"/>.</value>
        [JsonProperty(PropertyName = "selected")]
        public bool? Selected { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
