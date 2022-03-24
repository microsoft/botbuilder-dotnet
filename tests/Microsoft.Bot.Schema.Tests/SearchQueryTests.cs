// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class SearchQueryTests
    {
        [Fact]
        public void SearchInvokeValue()
        {
            var kind = SearchInvokeTypes.Typeahead;
            var context = "Microsoft.Graph";
            var options = new SearchInvokeOptions { Skip = 10, Top = 5 };
            var text = "the query text";

            var value = new SearchInvokeValue()
            {
                Kind = kind,
                Context = context,
                QueryOptions = options, 
                QueryText = text
            };

            Assert.Equal(kind, value.Kind);
            Assert.Equal(context, value.Context);
            Assert.Equal(options, value.QueryOptions);
            Assert.Equal(text, value.QueryText);
        }

        [Fact]
        public void SearchInvokeResponse()
        {
            var statusCode = 200;
            var type = "myType";
            var value = new { };

            var res = new SearchInvokeResponse()
            {
                StatusCode = statusCode,
                Type = type,
                Value = value,
            };

            Assert.Equal(statusCode, res.StatusCode);
            Assert.Equal(type, res.Type);
            Assert.Equal(value, res.Value);
        }

        [Fact]
        public void SearchQueryOptions()
        {
            int skip = 10;
            int top = 5;

            var options = new SearchInvokeOptions
            {
                Skip = skip,
                Top = top
            };
            
            Assert.Equal(skip, options.Skip);
            Assert.Equal(top, options.Top);
        }
    }
}
