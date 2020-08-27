// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests
{
    [TestClass]
    public sealed class IdentifierTests
    {
        public static IEnumerable<ulong> Bytes => new[]
        {
            0ul, 0x7Eul, 0x7Ful, 0x80ul, 0x81ul, 0xFFul,
        };

        public static IEnumerable<ulong> Items =>
            Bytes
            .Concat(from one in Bytes from two in Bytes select (one << 8) | two);

        public static IEnumerable<object[]> Data =>
            from one in Items
            from two in Items
            select new object[] { one, two };

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Identifier_Encode_Decode(ulong one, ulong two)
        {
            var item = Identifier.Encode(one, two);
            Identifier.Decode(item, out var oneX, out var twoX);
            Assert.AreEqual(one, oneX);
            Assert.AreEqual(two, twoX);
        }
    }
}
