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
        public void Long_Key_Should_Be_Truncated()
        {
            var tooLongKey = new string('a', CosmosDbKeyEscape.MaxKeyLength + 1);

            var sanitizedKey = CosmosDbKeyEscape.EscapeKey(tooLongKey);
            Assert.IsTrue(sanitizedKey.Length <= CosmosDbKeyEscape.MaxKeyLength, "Key too long");

            // The resulting key should be:
            var hash = tooLongKey.GetHashCode().ToString("x");
            var correctKey = sanitizedKey.Substring(0, CosmosDbKeyEscape.MaxKeyLength - hash.Length) + hash;

            Assert.AreEqual(correctKey, sanitizedKey);
        }

        [TestMethod]
        public void Long_Key_With_Illegal_Characters_Should_Be_Truncated()
        {
            var tooLongKeyWithIllegalCharacters = "?test?" + new string('A', 1000);
            var sanitizedKey = CosmosDbKeyEscape.EscapeKey(tooLongKeyWithIllegalCharacters);

            // Verify the key ws truncated
            Assert.IsTrue(sanitizedKey.Length <= CosmosDbKeyEscape.MaxKeyLength, "Key too long");

            // Make sure the escaping still happened
            Assert.IsTrue(sanitizedKey.StartsWith("*3ftest*3f"));
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

        [TestMethod]
        public void Long_Key_Should_Not_Be_Truncated_With_False_CompatibilityMode()
        {
            var tooLongKey = new string('a', CosmosDbKeyEscape.MaxKeyLength + 1);

            var sanitizedKey = CosmosDbKeyEscape.EscapeKey(tooLongKey, string.Empty, false);
            Assert.AreEqual(CosmosDbKeyEscape.MaxKeyLength + 1, sanitizedKey.Length, "Key should not have been truncated");

            // The resulting key should be identical
            Assert.AreEqual(tooLongKey, sanitizedKey);
        }

        [TestMethod]
        public void Long_Key_With_Illegal_Characters_Should_Not_Be_Truncated_With_False_CompatibilityMode()
        {
            var longKeyWithIllegalCharacters = "?test?" + new string('A', 1000);
            var sanitizedKey = CosmosDbKeyEscape.EscapeKey(longKeyWithIllegalCharacters, string.Empty, false);

            // Verify the key was NOT truncated
            Assert.AreEqual(1010, sanitizedKey.Length, "Key should not have been truncated");

            // Make sure the escaping still happened
            Assert.IsTrue(sanitizedKey.StartsWith("*3ftest*3f"));
        }

        [TestMethod]
        public void KeySuffix_Is_Added_To_End_of_Key()
        {
            var suffix = "test suffix";
            var key = "this is a test";
            var sanitizedKey = CosmosDbKeyEscape.EscapeKey(key, suffix, false);

            // Verify the suffix was added to the end of the key
            Assert.AreEqual(sanitizedKey, $"{key}{suffix}", "Suffix was not added to end of key");
        }
    }
}
