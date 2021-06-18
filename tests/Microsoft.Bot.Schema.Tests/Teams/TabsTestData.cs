// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    internal class TabsTestData
    {
        internal class TabRequestTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new TabEntityContext(),
                    new TabContext(),
                    "oAuthStateMagicCode",
                };

                yield return new object[]
                {
                    null,
                    null,
                    null,
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal class TabResponseTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { null };
                yield return new object[] { new TabResponsePayload() };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal class TabResponseCardsTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { null };
                yield return new object[]
                {
                    new List<TabResponseCard>()
                    {
                        new TabResponseCard(),
                        new TabResponseCard(),
                    } 
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
