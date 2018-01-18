using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Payments
{
    /// <summary>
    /// Pay method data for Microsoft Payment.
    /// </summary>
    public partial class MicrosoftPayMethodData
    {
        /// <summary>
        /// The pay method name.
        /// </summary>
        public const string MethodName = "https://pay.microsoft.com/microsoftpay";

        private const string TestModeValue = "TEST";

        /// <summary>
        /// Initializes a new instance of the MicrosoftPayMethodData class.
        /// </summary>
        public MicrosoftPayMethodData(string merchantId = default(string), IList<string> supportedNetworks = default(IList<string>), IList<string> supportedTypes = default(IList<string>), bool testMode = false) : this(merchantId, supportedNetworks, supportedTypes)
        {
            Mode = testMode ? TestModeValue : null;
        }

        /// <summary>
        /// Payment method mode
        /// </summary>
        [JsonProperty(PropertyName = "mode", NullValueHandling = NullValueHandling.Ignore)]
        public string Mode { get; set; }

        /// <summary>
        /// Get Microsoft Pay method data
        /// </summary>
        /// <returns>Payment method data</returns>
        public PaymentMethodData ToPaymentMethodData()
        {
            return new PaymentMethodData
            {
                SupportedMethods = new List<string> { MethodName },
                Data = this
            };
        }
    }
}
