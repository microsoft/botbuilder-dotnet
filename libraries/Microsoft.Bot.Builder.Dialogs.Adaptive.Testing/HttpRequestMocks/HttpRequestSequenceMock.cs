// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// <summary>
        /// The type of request.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.HttpRequestSequenceMock";

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestSequenceMock"/> class.
        /// </summary>
        /// <param name="path">optional path.</param>
        /// <param name="line">optional line.</param>
        [JsonConstructor]
        public HttpRequestSequenceMock([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Type for how <see cref="Body"/> matches against request's body.
        /// </summary>
        public enum BodyMatchType
        {
            /// <summary>
            /// Exact match.
            /// </summary>
            Exact,

            /// <summary>
            /// Match as a part.
            /// </summary>
            Partial
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
        /// Gets or sets the match type for body.
        /// </summary>
        /// <value>
        /// One of Exact, Partial.
        /// </value>
        [DefaultValue(BodyMatchType.Partial)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("matchType")]
        public BodyMatchType MatchType { get; set; } = BodyMatchType.Partial;

        /// <summary>
        /// Gets or sets the body to match against request's body.
        /// </summary>
        /// <value>
        /// Content.
        /// </value>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// Gets the sequence of responses to reply. The last one will be repeated.
        /// </summary>
        /// <value>
        /// The sequence of responses to reply.
        /// </value>
        [JsonProperty("responses")]
        public List<HttpResponseMock> Responses { get; } = new List<HttpResponseMock>();

        /// <summary>
        /// Configures the initial conditions.
        /// </summary>
        /// <param name="handler">The HttpMessageHandler.</param>
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

            if (!string.IsNullOrEmpty(Body))
            {
                if (Method == HttpRequest.HttpMethod.DELETE || Method == HttpRequest.HttpMethod.GET)
                {
                    throw new ArgumentException("GET and DELETE don't support matching body!");
                }

                if (MatchType == BodyMatchType.Exact)
                {
                    mocked = mocked.WithContent(Body);
                }
                else if (MatchType == BodyMatchType.Partial)
                {
                    mocked = mocked.WithPartialContent(Body);
                }
                else
                {
                    throw new InvalidEnumArgumentException($"{nameof(MatchType)} does not support {MatchType} yet!");
                }
            }

            mocked.Respond(re => response.GetMessage());
        }
    }
}
