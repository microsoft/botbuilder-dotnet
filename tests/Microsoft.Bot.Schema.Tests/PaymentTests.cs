// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class PaymentTests
    {
        [Fact]
        public void PaymentAddressInits()
        {
            var country = "USA";
            var addressLine = new List<string>() { "555 110th Ave NE" };
            var region = "WA";
            var city = "Bellevue";
            var dependentLocality = "CCP";
            var postalCode = "98004";
            var sortingCode = "mySortingCode";
            var languageCode = "en-US";
            var organization = "Microsoft";
            var recipient = "Mr. Bot Botly";
            var phone = "555-555-5555";

            var paymentAddress = new PaymentAddress(
                country,
                addressLine,
                region,
                city,
                dependentLocality,
                postalCode,
                sortingCode,
                languageCode,
                organization,
                recipient,
                phone);

            Assert.NotNull(paymentAddress);
            Assert.IsType<PaymentAddress>(paymentAddress);
            Assert.Equal(country, paymentAddress.Country);
            Assert.Equal(addressLine, paymentAddress.AddressLine);
            Assert.Equal(region, paymentAddress.Region);
            Assert.Equal(city, paymentAddress.City);
            Assert.Equal(dependentLocality, paymentAddress.DependentLocality);
            Assert.Equal(postalCode, paymentAddress.PostalCode);
            Assert.Equal(sortingCode, paymentAddress.SortingCode);
            Assert.Equal(languageCode, paymentAddress.LanguageCode);
            Assert.Equal(organization, paymentAddress.Organization);
            Assert.Equal(phone, paymentAddress.Phone);
            Assert.Equal(recipient, paymentAddress.Recipient);
        }
        
        [Fact]
        public void PaymentAddressInitsWithNoArgs()
        {
            var paymentAddress = new PaymentAddress();

            Assert.NotNull(paymentAddress);
            Assert.IsType<PaymentAddress>(paymentAddress);
        }

        [Fact]
        public void PaymentCurrencyAmountInits()
        {
            var currency = "$";
            var value = "100.00";
            var currencySystem = "USD";

            var paymentCurrencyAmount = new PaymentCurrencyAmount(currency, value, currencySystem);

            Assert.NotNull(paymentCurrencyAmount);
            Assert.IsType<PaymentCurrencyAmount>(paymentCurrencyAmount);
            Assert.Equal(currency, paymentCurrencyAmount.Currency);
            Assert.Equal(value, paymentCurrencyAmount.Value);
            Assert.Equal(currencySystem, paymentCurrencyAmount.CurrencySystem);
        }
        
        [Fact]
        public void PaymentCurrencyAmountInitsWithNoArgs()
        {
            var paymentCurrencyAmount = new PaymentCurrencyAmount();

            Assert.NotNull(paymentCurrencyAmount);
            Assert.IsType<PaymentCurrencyAmount>(paymentCurrencyAmount);
        }

        [Fact]
        public void PaymentDetailsInits()
        {
            var total = new PaymentItem("ball", new PaymentCurrencyAmount("$", "15.00", "USD"), false);
            var displayItems = new List<PaymentItem>() { total, new PaymentItem(), new PaymentItem() };
            var shippingOptions = new List<PaymentShippingOption>() { new PaymentShippingOption("123", "ballShipping", new PaymentCurrencyAmount(), false) };
            var modifiers = new List<PaymentDetailsModifier> { new PaymentDetailsModifier() };
            var error = "uh-oh";

            var paymentDetails = new PaymentDetails(total, displayItems, shippingOptions, modifiers, error);

            Assert.NotNull(paymentDetails);
            Assert.IsType<PaymentDetails>(paymentDetails);
            Assert.Equal(total, paymentDetails.Total);
            Assert.Equal(displayItems, paymentDetails.DisplayItems);
            Assert.Equal(shippingOptions, paymentDetails.ShippingOptions);
            Assert.Equal(modifiers, paymentDetails.Modifiers);
            Assert.Equal(error, paymentDetails.Error);
        }
        
        [Fact]
        public void PaymentDetailsInitsWithNoArgs()
        {
            var paymentDetails = new PaymentDetails();

            Assert.NotNull(paymentDetails);
            Assert.IsType<PaymentDetails>(paymentDetails);
        }

        [Fact]
        public void PaymentDetailsModifierInits()
        {
            var supportedMethods = new List<string>() { "credit", "debit" };
            var total = new PaymentItem("Awesome");
            var additionalDisplayItems = new List<PaymentItem> { new PaymentItem("item1"), new PaymentItem("item2") };
            var data = new { };

            var paymentDetailsModifier = new PaymentDetailsModifier(supportedMethods, total, additionalDisplayItems, data);

            Assert.NotNull(paymentDetailsModifier);
            Assert.IsType<PaymentDetailsModifier>(paymentDetailsModifier);
            Assert.Equal(supportedMethods, paymentDetailsModifier.SupportedMethods);
            Assert.Equal(total, paymentDetailsModifier.Total);
            Assert.Equal(additionalDisplayItems, paymentDetailsModifier.AdditionalDisplayItems);
            Assert.Equal(data, paymentDetailsModifier.Data);
        }
        
        [Fact]
        public void PaymentDetailsModifierInitsWithNoArgs()
        {
            var paymentDetailsModifier = new PaymentDetailsModifier();

            Assert.NotNull(paymentDetailsModifier);
            Assert.IsType<PaymentDetailsModifier>(paymentDetailsModifier);
        }

        [Fact]
        public void PaymentItemInits()
        {
            var label = "yo-yo";
            var amount = new PaymentCurrencyAmount("$", "5.00", "USD");
            var pending = false;

            var paymentItem = new PaymentItem(label, amount, pending);

            Assert.NotNull(paymentItem);
            Assert.IsType<PaymentItem>(paymentItem);
            Assert.Equal(label, paymentItem.Label);
            Assert.Equal(amount, paymentItem.Amount);
            Assert.Equal(pending, paymentItem.Pending);
        }
        
        [Fact]
        public void PaymentItemInitsWithNoArgs()
        {
            var paymentItem = new PaymentItem();

            Assert.NotNull(paymentItem);
            Assert.IsType<PaymentItem>(paymentItem);
        }

        [Fact]
        public void PaymentMethodDataInits()
        {
            var supportedMethods = new List<string> { "debit", "credit" };
            var data = new { };

            var paymentMethodData = new PaymentMethodData(supportedMethods, data);

            Assert.NotNull(paymentMethodData);
            Assert.IsType<PaymentMethodData>(paymentMethodData);
            Assert.Equal(supportedMethods, paymentMethodData.SupportedMethods);
            Assert.Equal(data, paymentMethodData.Data);
        }
        
        [Fact]
        public void PaymentMethodDataInitsWithNoArgs()
        {
            var paymentMethodData = new PaymentMethodData();

            Assert.NotNull(paymentMethodData);
            Assert.IsType<PaymentMethodData>(paymentMethodData);
        }

        [Fact]
        public void PaymentOptionsInits()
        {
            var requestPayerName = true;
            var requestPayerEmail = true;
            var requestPayerPhone = true;
            var requestShipping = true;
            var shippingType = "ground";

            var paymentOptions = new PaymentOptions(requestPayerName, requestPayerEmail, requestPayerPhone, requestShipping, shippingType);

            Assert.NotNull(paymentOptions);
            Assert.IsType<PaymentOptions>(paymentOptions);
            Assert.Equal(requestPayerName, paymentOptions.RequestPayerName);
            Assert.Equal(requestPayerEmail, paymentOptions.RequestPayerEmail);
            Assert.Equal(requestPayerPhone, paymentOptions.RequestPayerPhone);
            Assert.Equal(requestShipping, paymentOptions.RequestShipping);
            Assert.Equal(shippingType, paymentOptions.ShippingType);
        }
        
        [Fact]
        public void PaymentOptionsInitsWithNoArgs()
        {
            var paymentOptions = new PaymentOptions();

            Assert.NotNull(paymentOptions);
            Assert.IsType<PaymentOptions>(paymentOptions);
        }

        [Fact]
        public void PaymentRequestInits()
        {
            var id = "id";
            var methodData = new List<PaymentMethodData> { new PaymentMethodData(new List<string>() { "credit", "debit" }, new { }) };
            var details = new PaymentDetails(new PaymentItem("card", new PaymentCurrencyAmount("$", "5.00", "USD"), false));
            var options = new PaymentOptions(true, true, true);
            var expires = "P1D";

            var paymentRequest = new PaymentRequest(id, methodData, details, options, expires);

            Assert.NotNull(paymentRequest);
            Assert.IsType<PaymentRequest>(paymentRequest);
            Assert.Equal(id, paymentRequest.Id);
            Assert.Equal(methodData, paymentRequest.MethodData);
            Assert.Equal(details, paymentRequest.Details);
            Assert.Equal(options, paymentRequest.Options);
            Assert.Equal(expires, paymentRequest.Expires);
        }
        
        [Fact]
        public void PaymentRequestInitsWithNoArgs()
        {
            var paymentRequest = new PaymentRequest();

            Assert.NotNull(paymentRequest);
            Assert.IsType<PaymentRequest>(paymentRequest);
        }
    }
}
