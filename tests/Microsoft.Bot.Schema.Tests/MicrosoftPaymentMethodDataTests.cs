// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class MicrosoftPaymentMethodDataTests
    {
        [Fact]
        public void MicrosoftPaymentMethodDataInits()
        {
            var merchantId = "bestMerchant123";
            var supportedNetworks = new List<string>() { "networkA", "networkB" };
            var supportedTypes = new List<string>() { "credit", "debit" };

            var msPaymentMethodData = new MicrosoftPayMethodData(merchantId, supportedNetworks, supportedTypes);

            Assert.NotNull(msPaymentMethodData);
            Assert.IsType<MicrosoftPayMethodData>(msPaymentMethodData);
            Assert.Equal(merchantId, msPaymentMethodData.MerchantId);
            Assert.Equal(supportedNetworks, msPaymentMethodData.SupportedNetworks);
            Assert.Equal(supportedTypes, msPaymentMethodData.SupportedTypes);
            Assert.Null(msPaymentMethodData.Mode);
        }

        [Fact]
        public void MicrosoftPaymentMethodDataInitsWithNoArgs()
        {
            var msPaymentMethodData = new MicrosoftPayMethodData();

            Assert.NotNull(msPaymentMethodData);
            Assert.IsType<MicrosoftPayMethodData>(msPaymentMethodData);
        }

        [Theory]
        [InlineData(true, "TEST")]
        [InlineData(false, null)]
        public void MicrosoftPaymentMethodDataInitsWithTestMode(bool testMode, string expectedModeValue)
        {
            var merchantId = "bestMerchant123";
            var supportedNetworks = new List<string>() { "networkA", "networkB" };
            var supportedTypes = new List<string>() { "credit", "debit" };

            var msPaymentMethodData = new MicrosoftPayMethodData(merchantId, supportedNetworks, supportedTypes, testMode);

            Assert.NotNull(msPaymentMethodData);
            Assert.IsType<MicrosoftPayMethodData>(msPaymentMethodData);
            Assert.Equal(merchantId, msPaymentMethodData.MerchantId);
            Assert.Equal(supportedNetworks, msPaymentMethodData.SupportedNetworks);
            Assert.Equal(supportedTypes, msPaymentMethodData.SupportedTypes);
            Assert.Equal(expectedModeValue, msPaymentMethodData.Mode);
        }

        [Fact]
        public void ConvertsToPaymentMethodData()
        {
            var msPaymentMethodData = new MicrosoftPayMethodData();

            var convertedPaymentMethodData = msPaymentMethodData.ToPaymentMethodData();

            Assert.NotNull(convertedPaymentMethodData);
            Assert.IsType<PaymentMethodData>(convertedPaymentMethodData);
            var supportMethods = convertedPaymentMethodData.SupportedMethods;
            Assert.IsType<List<string>>(supportMethods);
            Assert.True(supportMethods.Contains("https://pay.microsoft.com/microsoftpay"));
            Assert.Equal(msPaymentMethodData, convertedPaymentMethodData.Data);
        }
    }
}
