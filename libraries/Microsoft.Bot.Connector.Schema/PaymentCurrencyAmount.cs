// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Supplies monetary amounts.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentCurrencyAmount
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentCurrencyAmount"/> class.
        /// </summary>
        public PaymentCurrencyAmount()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentCurrencyAmount"/> class.
        /// </summary>
        /// <param name="currency">A currency identifier.</param>
        /// <param name="value">Decimal monetary value.</param>
        /// <param name="currencySystem">Currency system.</param>
        public PaymentCurrencyAmount(string currency = default, string value = default, string currencySystem = default)
        {
            Currency = currency;
            Value = value;
            CurrencySystem = currencySystem;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets a currency identifier.
        /// </summary>
        /// <value>The currency.</value>
        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets decimal monetary value.
        /// </summary>
        /// <value>The decimal monetary value.</value>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets currency system.
        /// </summary>
        /// <value>The currency system.</value>
        [JsonPropertyName("currencySystem")]
        public string CurrencySystem { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
