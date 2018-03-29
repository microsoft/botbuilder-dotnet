// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class LocaleConverterTests
    {
        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public void LocaleConverter_ConvertFromFrench()
        {
            LocaleConverter localeConverter = LocaleConverter.Converter;

            var convertedMessage = localeConverter.Convert("Set a meeting on 30/9/2017", "fr-fr", "en-us");
            Assert.IsNotNull(convertedMessage);
            Assert.AreEqual("Set a meeting on 09/30/2017", convertedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public  void LocaleConverter_ConvertToChinese()
        {
            LocaleConverter localeConverter = LocaleConverter.Converter;

            var convertedMessage = localeConverter.Convert("Book me a plane ticket for France on 12/25/2018", "en-us", "zh-cn");
            Assert.IsNotNull(convertedMessage);
            Assert.AreEqual("Book me a plane ticket for France on 2018-12-25", convertedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public void LocaleConverter_InvalidFromLocale()
        {
            LocaleConverter localeConverter = LocaleConverter.Converter;

             Assert.ThrowsException<InvalidOperationException>(() =>
                 localeConverter.Convert("Book me a plane ticket for France on 12/25/2018", "na-na", "en-us")); 
        }   

    [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Locale Converter")]
        public void LocaleConverter_InvalidToLocale()
        {
            LocaleConverter localeConverter = LocaleConverter.Converter;

             Assert.ThrowsException<InvalidOperationException>( ()=>
                 localeConverter.Convert("Book me a plane ticket for France on 12/25/2018", "en-us", "na-na")); 
        }
    }
}
