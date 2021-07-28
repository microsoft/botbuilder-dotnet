// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardDateInputTests
    {
        [Fact]
        public void O365ConnectorCardDateInputInits()
        {
            var type = "dateInput";
            var id = "dateInput123";
            var isRequired = true;
            var title = "Enter Date";
            var value = "2009-06-15T13:45:30";
            var includeTime = false;

            var dateInput = new O365ConnectorCardDateInput(type, id, isRequired, title, value, includeTime);

            Assert.NotNull(dateInput);
            Assert.IsType<O365ConnectorCardDateInput>(dateInput);
            Assert.Equal(id, dateInput.Id);
            Assert.Equal(isRequired, dateInput.IsRequired);
            Assert.Equal(title, dateInput.Title);
            Assert.Equal(value, dateInput.Value);
            Assert.Equal(includeTime, dateInput.IncludeTime);
        }
        
        [Fact]
        public void O365ConnectorCardDateInputInitsWithNoArgs()
        {
            var dateInput = new O365ConnectorCardDateInput();

            Assert.NotNull(dateInput);
            Assert.IsType<O365ConnectorCardDateInput>(dateInput);
        }
    }
}
