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
        private List<HttpContent> _contents;

        public SequenceResponseManager(List<HttpResponseMock> responses)
        {
            _id = 0;
            if (responses == null || responses.Count == 0)
            {
                _contents = new List<HttpContent>()
                {
                    new StringContent(string.Empty)
                };
            }
            else
            {
                _contents = responses.Select(r =>
                {
                    switch (r.ContentType)
                    {
                        case HttpResponseMock.ContentTypes.String:
                            return (HttpContent)new StringContent(r.Content == null ? string.Empty : r.Content.ToString());
                        case HttpResponseMock.ContentTypes.ByteArray:
                            var bytes = Convert.FromBase64String(r.Content == null ? string.Empty : r.Content.ToString());
                            return (HttpContent)new ByteArrayContent(bytes);
                        default:
                            throw new NotSupportedException($"{r.ContentType} is not supported yet!");
                    }
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

            return result;
        }
    }
}
