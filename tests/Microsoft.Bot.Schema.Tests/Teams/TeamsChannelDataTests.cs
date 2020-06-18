// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    [TestClass]
    public class TeamsChannelDataTests
    {
        [TestMethod]
        public void GetAadGroupId()
        {
            // Arrange
            const string AadGroupId = "teamGroup123";
            var activity = new Activity { ChannelData = new TeamsChannelData { Team = new TeamInfo { AadGroupId = AadGroupId } } };

            // Act
            var channelData = activity.GetChannelData<TeamsChannelData>();

            // Assert
            Assert.AreEqual(AadGroupId, channelData.Team.AadGroupId);
        }
    }
}
