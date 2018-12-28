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
            Assert.ThrowsException<ArgumentNullException>(() => CosmosDbKeyEscape.EscapeKey(null));

            // Empty string should throw
            Assert.ThrowsException<ArgumentNullException>(() => CosmosDbKeyEscape.EscapeKey(string.Empty));

            // Whitespace key should throw
            Assert.ThrowsException<ArgumentNullException>(() => CosmosDbKeyEscape.EscapeKey("     "));
        }

        [TestMethod]
        public void Sanitize_Key_Should_Not_Change_A_Valid_Key()
        {
            var validKey = "Abc12345";
            var sanitizedKey = CosmosDbKeyEscape.EscapeKey(validKey);
            Assert.AreEqual(validKey, sanitizedKey);
        }

        [TestMethod]
        public void Sanitize_Key_Should_Escape_Illegal_Character()
        {
            // Ascii code of "?" is "3f".
            var sanitizedKey = CosmosDbKeyEscape.EscapeKey("?test?");
            Assert.AreEqual(sanitizedKey, "*3ftest*3f");

            // Ascii code of "/" is "2f".
            var sanitizedKey2 = CosmosDbKeyEscape.EscapeKey("/test/");
            Assert.AreEqual(sanitizedKey2, "*2ftest*2f");

            // Ascii code of "\" is "5c".
            var sanitizedKey3 = CosmosDbKeyEscape.EscapeKey("\\test\\");
            Assert.AreEqual(sanitizedKey3, "*5ctest*5c");

            // Ascii code of "#" is "23".
            var sanitizedKey4 = CosmosDbKeyEscape.EscapeKey("#test#");
            Assert.AreEqual(sanitizedKey4, "*23test*23");

            // Ascii code of "*" is "2a".
            var sanitizedKey5 = CosmosDbKeyEscape.EscapeKey("*test*");
            Assert.AreEqual(sanitizedKey5, "*2atest*2a");

            // Check a compound key
            var compoundSanitizedKey = CosmosDbKeyEscape.EscapeKey("?#/");
            Assert.AreEqual(compoundSanitizedKey, "*3f*23*2f");
        }

        [TestMethod]
        public void Collisions_Should_Not_Happen()
        {
            var validKey = "*2atest*2a";
            var validKey2 = "*test*";

            // If we failed to esacpe the "*", then validKey2 would
            // escape to the same value as validKey. To prevent this
            // we makes sure to escape the *. 

            // Ascii code of "*" is "2a".
            var escaped1 = CosmosDbKeyEscape.EscapeKey(validKey);
            var escaped2 = CosmosDbKeyEscape.EscapeKey(validKey2);

            Assert.AreNotEqual(escaped1, escaped2); 
        }
    }
}
