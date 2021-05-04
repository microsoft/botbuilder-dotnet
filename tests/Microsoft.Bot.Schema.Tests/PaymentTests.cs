﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
    }
}
