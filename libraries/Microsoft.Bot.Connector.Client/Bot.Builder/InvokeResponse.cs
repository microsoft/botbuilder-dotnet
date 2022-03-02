﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Client.Bot.Builder
{
    /// <summary>
    /// A tuple class containing an HTTP status code and a JSON-serializable
    /// object. The HTTP status code is, in the invoke activity scenario, what will
    /// be set in the resulting POST. The body of the resulting POST will be
    /// the JSON-serialized content from the <see cref="Body"/> property.
    /// </summary>
    public class InvokeResponse
    {
        /// <summary>Gets or sets the HTTP status code for the response.</summary>
        /// <value>The HTTP status code.</value>
        /// <remarks>
        /// The POST that is generated in response to the incoming invoke activity
        /// will have the HTTP status code specified by this field.
        /// </remarks>
        public int Status { get; set; }

        /// <summary>Gets or sets the body content for the response.</summary>
        /// <value>The body content.</value>
        /// <remarks>
        /// The POST that is generated in response to the incoming invoke activity
        /// will have a body generated by JSON serializing the object in this field.
        /// </remarks>
        public object Body { get; set; }

        /// <summary>
        /// Gets a value indicating whether the invoke response was successful.
        /// </summary>
        /// <returns>
        /// A value that indicates if the HTTP response was successful.
        /// true if <see cref="Status"/> was in the Successful range (200-299); otherwise false.
        /// </returns>
        public bool IsSuccessStatusCode() => Status >= 200 && Status <= 299;
    }
}