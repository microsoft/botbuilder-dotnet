// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// A PaymentResponse is returned when a user has selected a payment method
    /// and approved a payment request.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentResponse"/> class.
        /// </summary>
        public PaymentResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentResponse"/> class.
        /// </summary>
        /// <param name="methodName">The payment method identifier for the
        /// payment method that the user selected to fulfil the
        /// transaction.</param>
        /// <param name="details">A JSON-serializable object that provides a
        /// payment method specific message used by the merchant to process the
        /// transaction and determine successful fund transfer.</param>
        /// <param name="shippingAddress">If the requestShipping flag was set
        /// to true in the PaymentOptions passed to the PaymentRequest
        /// constructor, then shippingAddress will be the full and final
        /// shipping address chosen by the user.</param>
        /// <param name="shippingOption">If the requestShipping flag was set to
        /// true in the PaymentOptions passed to the PaymentRequest
        /// constructor, then shippingOption will be the id attribute of the
        /// selected shipping option.</param>
        /// <param name="payerEmail">If the requestPayerEmail flag was set to
        /// true in the PaymentOptions passed to the PaymentRequest
        /// constructor, then payerEmail will be the email address chosen by
        /// the user.</param>
        /// <param name="payerPhone">If the requestPayerPhone flag was set to
        /// true in the PaymentOptions passed to the PaymentRequest
        /// constructor, then payerPhone will be the phone number chosen by the
        /// user.</param>
        public PaymentResponse(string methodName = default, object details = default, PaymentAddress shippingAddress = default, string shippingOption = default, string payerEmail = default, string payerPhone = default)
        {
            MethodName = methodName;
            Details = details;
            ShippingAddress = shippingAddress;
            ShippingOption = shippingOption;
            PayerEmail = payerEmail;
            PayerPhone = payerPhone;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the payment method identifier for the payment method
        /// that the user selected to fulfill the transaction.
        /// </summary>
        /// <value>The payment method identifier.</value>
        [JsonPropertyName("methodName")]
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets a JSON-serializable object that provides a payment
        /// method specific message used by the merchant to process the
        /// transaction and determine successful fund transfer.
        /// </summary>
        /// <value>The JSON-serializable data object that provides additional information to process the transaction.</value>
        [JsonPropertyName("details")]
        public object Details { get; set; }

        /// <summary>
        /// Gets or sets if the requestShipping flag was set to true in the
        /// PaymentOptions passed to the PaymentRequest constructor, then
        /// shippingAddress will be the full and final shipping address chosen
        /// by the user.
        /// </summary>
        /// <value>The final shipping address chosen by the user.</value>
        [JsonPropertyName("shippingAddress")]
        public PaymentAddress ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets if the requestShipping flag was set to true in the
        /// PaymentOptions passed to the PaymentRequest constructor, then
        /// shippingOption will be the id attribute of the selected shipping
        /// option.
        /// </summary>
        /// <value>The shipping option ID.</value>
        [JsonPropertyName("shippingOption")]
        public string ShippingOption { get; set; }

        /// <summary>
        /// Gets or sets if the requestPayerEmail flag was set to true in the
        /// PaymentOptions passed to the PaymentRequest constructor, then
        /// payerEmail will be the email address chosen by the user.
        /// </summary>
        /// <value>The payer email chosen by the user..</value>
        [JsonPropertyName("payerEmail")]
        public string PayerEmail { get; set; }

        /// <summary>
        /// Gets or sets if the requestPayerPhone flag was set to true in the
        /// PaymentOptions passed to the PaymentRequest constructor, then
        /// payerPhone will be the phone number chosen by the user.
        /// </summary>
        /// <value>The payer phone number chosen by the user.</value>
        [JsonPropertyName("payerPhone")]
        public string PayerPhone { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
