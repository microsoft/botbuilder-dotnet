// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

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

        internal class TabResponsePayloadTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    null,
                    null,
                    null
                };

                yield return new object[]
                {
                    "docx",
                    new TabResponseCards(),
                    new TabSuggestedActions(),
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal class TabSubmitTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                // TabEntityContext tabEntityContext, TabContext tabContext, TabSubmitData tabSubmitData
                yield return new object[]
                {
                    null,
                    null,
                    null,
                };

                yield return new object[]
                {
                    new TabEntityContext(),
                    new TabContext(),
                    new TabSubmitData(),
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal class TabSubmitDataTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                // string tabType, JObject properties
                yield return new object[] { null, null };
                yield return new object[]
                {
                    "pdf",
                    new JObject() { { "key", "value" } },
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal class TabSuggestedActionsTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { null };
                yield return new object[]
                { 
                    new List<CardAction>()
                    { 
                        new CardAction(),
                        new CardAction(),
                    } 
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
