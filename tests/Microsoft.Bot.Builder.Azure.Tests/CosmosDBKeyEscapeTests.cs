// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - CosmosDB")]
    public class CosmosDBKeyEscapeTests
    {
        [TestMethod]
        public void Sanitize_Key_Should_Fail_With_Null_Key()
        {
            // Null key should throw
            Assert.ThrowsException<ArgumentNullException>(() => CosmosDBKeyEscape.EscapeKey(null));

            // Empty string should throw
            Assert.ThrowsException<ArgumentNullException>(() => CosmosDBKeyEscape.EscapeKey(string.Empty));

            // Whitespace key should throw
            Assert.ThrowsException<ArgumentNullException>(() => CosmosDBKeyEscape.EscapeKey("     "));
        }

        [TestMethod]
        public void Sanitize_Key_Should_Not_Change_A_Valid_Key()
        {
            var validKey = "Abc12345";
            var sanitizedKey = CosmosDBKeyEscape.EscapeKey(validKey);
            Assert.AreEqual(validKey, sanitizedKey);
        }

        [TestMethod]
        public void Sanitize_Key_Should_Escape_Illegal_Character()
        {
            // Ascii code of "?" is "3f".
            var sanitizedKey = CosmosDBKeyEscape.EscapeKey("?test?");
            Assert.AreEqual(sanitizedKey, "*3ftest*3f");

            // Ascii code of "/" is "2f".
            var sanitizedKey2 = CosmosDBKeyEscape.EscapeKey("/test/");
            Assert.AreEqual(sanitizedKey2, "*2ftest*2f");

            // Ascii code of "\" is "5c".
            var sanitizedKey3 = CosmosDBKeyEscape.EscapeKey("\\test\\");
            Assert.AreEqual(sanitizedKey3, "*5ctest*5c");

            // Ascii code of "#" is "23".
            var sanitizedKey4 = CosmosDBKeyEscape.EscapeKey("#test#");
            Assert.AreEqual(sanitizedKey4, "*23test*23");

            // Check a compound key
            var compoundSanitizedKey = CosmosDBKeyEscape.EscapeKey("?#/");
            Assert.AreEqual(compoundSanitizedKey, "*3f*23*2f");
        }
    }
}
