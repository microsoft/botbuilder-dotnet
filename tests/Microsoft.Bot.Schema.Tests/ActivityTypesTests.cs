// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Schema.Tests
{
    public class ActivityTypesTests
    {
        [TestClass]
        public class GetRuntimeTypeTests : ActivityTypesTests
        {
            [TestMethod]
            public void UnknownTypeReturnsBaseActivityType()
            {
                var activityType = ActivityTypes.GetRuntimeType("!!NOT-A-REAL-TYPE!!");

                Assert.AreEqual(typeof(Activity), activityType);
            }

            [TestMethod]
            public void KnownTypeReturnsExpectedType()
            {
                var activityType = ActivityTypes.GetRuntimeType(ActivityTypes.Event);

                Assert.AreEqual(typeof(EventActivity), activityType);
            }
        }
    }
}
