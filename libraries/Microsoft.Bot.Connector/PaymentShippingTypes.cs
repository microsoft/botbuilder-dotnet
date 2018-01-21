namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Different shipping types. See http://www.w3.org/TR/payment-request/#dom-paymentoptions-shippingtype for more information.
    /// </summary>
    public static class PaymentShippingTypes
    {
        /// <summary>
        /// This is the default and refers to the address being collected as the destination for shipping.
        /// </summary>
        public const string Shipping = "shipping";

        /// <summary>
        /// This refers to the address being collected as being used for delivery. This is commonly faster than shipping. For example, it might be used for food delivery.
        /// </summary>
        public const string Delivery = "delivery";

        /// <summary>
        /// This refers to the address being collected as part of a service pickup. For example, this could be the address for laundry pickup.
        /// </summary>
        public const string PickUp = "pickup";
    }
}
