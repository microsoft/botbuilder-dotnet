// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    /// <summary>
    /// Manage Sequence Response for HttpRequestSequenceMock.
    /// </summary>
    public class SequenceResponseManager
    {
        private int _id;
        private List<HttpResponseMockContent> _contents = new List<HttpResponseMockContent>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceResponseManager"/> class.
        /// Return the list of mocks in sequence order. The last one will be repeated.
        /// </summary>
        /// <param name="responses">The list of HttpResponseMock.</param>
        public SequenceResponseManager(List<HttpResponseMock> responses)
        {
            _id = 0;
            if (responses == null || responses.Count == 0)
            {
                // Create an empty content for response.
                _contents.Add(new HttpResponseMockContent());
            }
            else
            {
                foreach (var response in responses)
                {
                    _contents.Add(new HttpResponseMockContent(response));
                }
            }
        }

        /// <summary>
        /// Return the content in sequence order. The last one will be repeated.
        /// </summary>
        /// <returns>
        /// The HttpContent.
        /// </returns>
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
