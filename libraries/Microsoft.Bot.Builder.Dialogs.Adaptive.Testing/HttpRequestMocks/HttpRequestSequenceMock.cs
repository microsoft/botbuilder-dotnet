// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    /// <summary>
    /// Mock http request in sequence order. The last response will be repeated.
    /// </summary>
    public class HttpRequestSequenceMock : HttpRequestMock
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.HttpRequestSequenceMock";

        [JsonConstructor]
        public HttpRequestSequenceMock([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the HttpMethod to match. If null, match to any method.
        /// </summary>
        /// <value>
        /// One of GET, POST, PATCH, PUT, DELETE, null.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("method")]
        public HttpRequest.HttpMethod? Method { get; set; }

        /// <summary>
        /// Gets or sets the Url to match. Absolute or relative, may contain * wildcards.
        /// </summary>
        /// <value>
        /// Url.
        /// </value>
        [JsonProperty("url")]
#pragma warning disable CA1056 // Uri properties should not be strings (by design, excluding)
        public string Url { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets the sequence of responses to reply. The last one will be repeated.
        /// </summary>
        /// <value>
        /// The sequence of responses to reply.
        /// </value>
        [JsonProperty("responses")]
        public List<HttpResponseMock> Responses { get;  } = new List<HttpResponseMock>();

        public override void Setup(MockHttpMessageHandler handler)
        {
            var response = new SequenceResponseManager(Responses);

            MockedRequest mocked;
            if (!Method.HasValue)
            {
                mocked = handler.When(Url);
            }
            else
            {
                mocked = handler.When(new HttpMethod(Method.Value.ToString()), Url);
            }

            mocked.Respond(re => response.GetContent());
        }
    }
}
