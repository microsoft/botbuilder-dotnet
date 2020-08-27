// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    /// <summary>
    /// Manage Sequence Response for HttpRequestSequenceMock.
    /// </summary>
    public class SequenceResponseManager
    {
        private int _id;
        private List<HttpResponseMockMessage> _messages = new List<HttpResponseMockMessage>();

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
                // Create a default message for response.
                _messages.Add(new HttpResponseMockMessage());
            }
            else
            {
                foreach (var response in responses)
                {
                    _messages.Add(new HttpResponseMockMessage(response));
                }
            }
        }

        /// <summary>
        /// Return the message in sequence order. The last one will be repeated.
        /// </summary>
        /// <returns>
        /// The HttpResponseMessage.
        /// </returns>
        public HttpResponseMessage GetMessage()
        {
            var result = _messages[_id];
            if (_id < _messages.Count - 1)
            {
                _id++;
            }

            // We create a new one here in case the consumer will dispose the object.
            return result.GetMessage();
        }
    }
}
