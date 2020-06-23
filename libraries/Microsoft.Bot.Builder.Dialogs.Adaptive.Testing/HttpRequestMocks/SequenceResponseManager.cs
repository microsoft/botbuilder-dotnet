// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    public class SequenceResponseManager
    {
        private int _id;
        private List<HttpResponseMockContent> _contents;

        public SequenceResponseManager(List<HttpResponseMock> responses)
        {
            _id = 0;
            if (responses == null || responses.Count == 0)
            {
                _contents = new List<HttpResponseMockContent>()
                {
                    new HttpResponseMockContent()
                };
            }
            else
            {
                _contents = responses.Select(r =>
                {
                    return new HttpResponseMockContent(r);
                }).ToList();
            }
        }

        public HttpContent GetContent()
        {
            var result = _contents[_id];
            if (_id < _contents.Count - 1)
            {
                _id++;
            }

            // We create a new one here in case the consumer will dispose the content object.
            return result.GetHttpContent();
        }
    }
}
