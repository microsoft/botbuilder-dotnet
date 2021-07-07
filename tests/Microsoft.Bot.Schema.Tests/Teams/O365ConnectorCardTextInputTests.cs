// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardTextInputTests
    {
        [Fact]
        public void O365ConnectorCardTextInputInits()
        {
            var type = "textInput";
            var id = "firstName";
            var isRequired = true;
            var title = "Profile";
            var value = "First Name";
            var isMultiline = false;
            var maxLength = 250;

            var textInput = new O365ConnectorCardTextInput(type, id, isRequired, title, value, isMultiline, maxLength);

            Assert.NotNull(textInput);
            Assert.IsType<O365ConnectorCardTextInput>(textInput);
            Assert.Equal(id, textInput.Id);
            Assert.Equal(isRequired, textInput.IsRequired);
            Assert.Equal(title, textInput.Title);
            Assert.Equal(value, textInput.Value);
            Assert.Equal(isMultiline, textInput.IsMultiline);
            Assert.Equal(maxLength, textInput.MaxLength);
        }
        
        [Fact]
        public void O365ConnectorCardTextInputInitsWithNoArgs()
        {
            var textInput = new O365ConnectorCardTextInput();

            Assert.NotNull(textInput);
            Assert.IsType<O365ConnectorCardTextInput>(textInput);
        }
    }
}
