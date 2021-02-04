// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides information about the options desired for the payment request.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public partial class PaymentOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentOptions"/> class.
        /// </summary>
        public PaymentOptions()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentOptions"/> class.
        /// </summary>
        /// <param name="requestPayerName">Indicates whether the user agent
        /// should collect and return the payer's name as part of the payment
        /// request.</param>
        /// <param name="requestPayerEmail">Indicates whether the user agent
        /// should collect and return the payer's email address as part of the
        /// payment request.</param>
        /// <param name="requestPayerPhone">Indicates whether the user agent
        /// should collect and return the payer's phone number as part of the
        /// payment request.</param>
        /// <param name="requestShipping">Indicates whether the user agent
        /// should collect and return a shipping address as part of the payment
        /// request.</param>
        /// <param name="shippingType">If requestShipping is set to true, then
        /// the shippingType field may be used to influence the way the user
        /// agent presents the user interface for gathering the shipping
        /// address.</param>
        public PaymentOptions(bool? requestPayerName = default(bool?), bool? requestPayerEmail = default(bool?), bool? requestPayerPhone = default(bool?), bool? requestShipping = default(bool?), string shippingType = default(string))
        {
            RequestPayerName = requestPayerName;
            RequestPayerEmail = requestPayerEmail;
            RequestPayerPhone = requestPayerPhone;
            RequestShipping = requestShipping;
            ShippingType = shippingType;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets indicates whether the user agent should collect and
        /// return the payer's name as part of the payment request.
        /// </summary>
        /// <value>Boolean indicating if user agent should collect and return payer's name.</value>
        [JsonProperty(PropertyName = "requestPayerName")]
        public bool? RequestPayerName { get; set; }

        /// <summary>
        /// Gets or sets indicates whether the user agent should collect and
        /// return the payer's email address as part of the payment request.
        /// </summary>
        /// <value>Boolean indicating if user agent should collect and return payer's email address.</value>
        [JsonProperty(PropertyName = "requestPayerEmail")]
        public bool? RequestPayerEmail { get; set; }

        /// <summary>
        /// Gets or sets indicates whether the user agent should collect and
        /// return the payer's phone number as part of the payment request.
        /// </summary>
        /// <value>Boolean indicating if user agent should collect and return payer's phone number.</value>
        [JsonProperty(PropertyName = "requestPayerPhone")]
        public bool? RequestPayerPhone { get; set; }

        /// <summary>
        /// Gets or sets indicates whether the user agent should collect and
        /// return a shipping address as part of the payment request.
        /// </summary>
        /// <value>Boolean indicating if user agent should collect and return shipping address.</value>
        [JsonProperty(PropertyName = "requestShipping")]
        public bool? RequestShipping { get; set; }

        /// <summary>
        /// Gets or sets if requestShipping is set to true, then the
        /// shippingType field may be used to influence the way the user agent
        /// presents the user interface for gathering the shipping address.
        /// </summary>
        /// <value>The shipping type.</value>
        [JsonProperty(PropertyName = "shippingType")]
        public string ShippingType { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
