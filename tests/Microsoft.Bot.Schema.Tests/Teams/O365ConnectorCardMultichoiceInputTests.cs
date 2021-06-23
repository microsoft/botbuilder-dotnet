// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardMultichoiceInputTests
    {
        [Fact]
        public void O365ConnectorCardMultichoiceInputInits()
        {
            var type = "multichoiceInput";
            var id = "bookChoice";
            var isRequired = true;
            var title = "Books";
            var value = "No Book Selected";
            var choices = new List<O365ConnectorCardMultichoiceInputChoice>()
            { 
                new O365ConnectorCardMultichoiceInputChoice("C# In Depth"),
                new O365ConnectorCardMultichoiceInputChoice("C# in a Nutshell"), 
            };
            var style = "expanded";
            var isMultiSelect = true;

            var multichoiceInput = new O365ConnectorCardMultichoiceInput(type, id, isRequired, title, value, choices, style, isMultiSelect);

            Assert.NotNull(multichoiceInput);
            Assert.IsType<O365ConnectorCardMultichoiceInput>(multichoiceInput);
            Assert.Equal(id, multichoiceInput.Id);
            Assert.Equal(isRequired, multichoiceInput.IsRequired);
            Assert.Equal(title, multichoiceInput.Title);
            Assert.Equal(value, multichoiceInput.Value);
            Assert.Equal(choices, multichoiceInput.Choices);
            Assert.Equal(2, multichoiceInput.Choices.Count);
            Assert.Equal(style, multichoiceInput.Style);
            Assert.Equal(isMultiSelect, multichoiceInput.IsMultiSelect);
        }
        
        [Fact]
        public void O365ConnectorCardMultichoiceInputInitsWithNoArgs()
        {
            var multichoiceInput = new O365ConnectorCardMultichoiceInput();

            Assert.NotNull(multichoiceInput);
            Assert.IsType<O365ConnectorCardMultichoiceInput>(multichoiceInput);
        }
    }
}
