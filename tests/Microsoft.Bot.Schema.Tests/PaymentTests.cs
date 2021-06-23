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

        [Fact]
        public void PaymentRequestCompleteInits()
        {
            var id = "id";
            var paymentRequest = new PaymentRequest(
                "paymentId",
                new List<PaymentMethodData>() { new PaymentMethodData(new List<string> { "credit", "debit" }) },
                new PaymentDetails(),
                new PaymentOptions(true, true, true));
            var paymentResponse = new PaymentResponse("credit", new { }, new PaymentAddress(), "ground", "example@somedomain.com", "555-555-5555");

            var paymentRequestComplete = new PaymentRequestComplete(id, paymentRequest, paymentResponse);

            Assert.NotNull(paymentRequestComplete);
            Assert.IsType<PaymentRequestComplete>(paymentRequestComplete);
            Assert.Equal(id, paymentRequestComplete.Id);
            Assert.Equal(paymentRequest, paymentRequestComplete.PaymentRequest);
            Assert.Equal(paymentResponse, paymentRequestComplete.PaymentResponse);
        }
        
        [Fact]
        public void PaymentRequestCompleteInitsWithNoArgs()
        {
            var paymentRequestComplete = new PaymentRequestComplete();

            Assert.NotNull(paymentRequestComplete);
            Assert.IsType<PaymentRequestComplete>(paymentRequestComplete);
        }

        [Fact]
        public void PaymentRequestCompleteResultInits()
        {
            var result = "success";
            var paymentRequestCompleteResult = new PaymentRequestCompleteResult(result);

            Assert.NotNull(paymentRequestCompleteResult);
            Assert.IsType<PaymentRequestCompleteResult>(paymentRequestCompleteResult);
            Assert.Equal(result, paymentRequestCompleteResult.Result);
        }
        
        [Fact]
        public void PaymentRequestCompleteResultInitsWithNoArgs()
        {
            var paymentRequestCompleteResult = new PaymentRequestCompleteResult();

            Assert.NotNull(paymentRequestCompleteResult);
            Assert.IsType<PaymentRequestCompleteResult>(paymentRequestCompleteResult);
        }

        [Fact]
        public void PaymentRequestUpdateInits()
        {
            var id = "id";
            var details = new PaymentDetails();
            var shippingAddress = GetShippingAddress();
            var shippingOption = "ground";

            var paymentRequestUpdate = new PaymentRequestUpdate(id, details, shippingAddress, shippingOption);

            Assert.NotNull(paymentRequestUpdate);
            Assert.IsType<PaymentRequestUpdate>(paymentRequestUpdate);
            Assert.Equal(id, paymentRequestUpdate.Id);
            Assert.Equal(details, paymentRequestUpdate.Details);
            Assert.Equal(shippingAddress, paymentRequestUpdate.ShippingAddress);
            Assert.Equal(shippingOption, paymentRequestUpdate.ShippingOption);
        }
        
        [Fact]
        public void PaymentRequestUpdateInitsWithNoArgs()
        {
            var paymentRequestUpdate = new PaymentRequestUpdate();

            Assert.NotNull(paymentRequestUpdate);
            Assert.IsType<PaymentRequestUpdate>(paymentRequestUpdate);
        }

        [Fact]
        public void PaymentRequestUpdateResultInits()
        {
            var details = new PaymentDetails(new PaymentItem(), new List<PaymentItem>(), new List<PaymentShippingOption>(), new List<PaymentDetailsModifier>(), "uh-oh");

            var paymentRequestUpdateResult = new PaymentRequestUpdateResult(details);

            Assert.NotNull(paymentRequestUpdateResult);
            Assert.IsType<PaymentRequestUpdateResult>(paymentRequestUpdateResult);
            Assert.Equal(details, paymentRequestUpdateResult.Details);
        }
        
        [Fact]
        public void PaymentRequestUpdateResultInitsWithNoArgs()
        {
            var paymentRequestUpdateResult = new PaymentRequestUpdateResult();

            Assert.NotNull(paymentRequestUpdateResult);
            Assert.IsType<PaymentRequestUpdateResult>(paymentRequestUpdateResult);
        }

        [Fact]
        public void PaymentResponseInits()
        {
            var methodName = "credit";
            var details = new { };
            var shippingAddress = GetShippingAddress();
            var shippingOption = "ground";
            var payerEmail = "example@somedomain.com";
            var payerPhone = "555-555-5555";

            var paymentResponse = new PaymentResponse(methodName, details, shippingAddress, shippingOption, payerEmail, payerPhone);

            Assert.NotNull(paymentResponse);
            Assert.IsType<PaymentResponse>(paymentResponse);
            Assert.Equal(methodName, paymentResponse.MethodName);
            Assert.Equal(details, paymentResponse.Details);
            Assert.Equal(shippingAddress, paymentResponse.ShippingAddress);
            Assert.Equal(shippingOption, paymentResponse.ShippingOption);
            Assert.Equal(payerEmail, paymentResponse.PayerEmail);
            Assert.Equal(payerPhone, paymentResponse.PayerPhone);
        }
        
        [Fact]
        public void PaymentResponseInitsWithNoArgs()
        {
            var paymentResponse = new PaymentResponse();

            Assert.NotNull(paymentResponse);
            Assert.IsType<PaymentResponse>(paymentResponse);
        }

        [Fact]
        public void PaymentShippingOptionInits()
        {
            var id = "shippingOptionId";
            var label = "label";
            var amount = new PaymentCurrencyAmount();
            var selected = true;

            var paymentShippingOption = new PaymentShippingOption(id, label, amount, selected);

            Assert.NotNull(paymentShippingOption);
            Assert.IsType<PaymentShippingOption>(paymentShippingOption);
            Assert.Equal(id, paymentShippingOption.Id);
            Assert.Equal(label, paymentShippingOption.Label);
            Assert.Equal(amount, paymentShippingOption.Amount);
            Assert.Equal(selected, paymentShippingOption.Selected);
        }
        
        [Fact]
        public void PaymentShippingOptionInitsWithNoArgs()
        {
            var paymentShippingOption = new PaymentShippingOption();

            Assert.NotNull(paymentShippingOption);
            Assert.IsType<PaymentShippingOption>(paymentShippingOption);
        }

        private PaymentAddress GetShippingAddress()
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

            return new PaymentAddress(
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
        }
    }
}
