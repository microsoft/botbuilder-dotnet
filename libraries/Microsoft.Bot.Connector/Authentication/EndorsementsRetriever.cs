// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// The endorsements property within each key contains one or more endorsement
    /// strings which you can use to verify that the channel ID specified in the channelId
    /// property within the Activity object of the incoming request is authentic.
    /// More details at:
    ///     https://docs.microsoft.com/bot-framework/rest-api/bot-framework-rest-connector-authentication.
    /// </summary>
    public sealed class EndorsementsRetriever : IDocumentRetriever, IConfigurationRetriever<IDictionary<string, HashSet<string>>>
    {
        /// <summary>
        /// JSON Web Key Set Metadata value
        /// From the OpenID Spec at
        ///     https://openid.net/specs/openid-connect-discovery-1_0.html
        ///     URL of the OP's JSON Web Key Set [JWK] document. This contains the signing key(s)
        ///     the RP uses to validate signatures from the OP. The JWK Set MAY also contain the
        ///     Server's encryption key(s), which are used by RPs to encrypt requests to the
        ///     Server. When both signing and encryption keys are made available, a use (Key Use)
        ///     parameter value is REQUIRED for all keys in the referenced JWK Set to indicate
        ///     each key's intended usage. Although some algorithms allow the same key to be
        ///     used for both signatures and encryption, doing so is NOT RECOMMENDED, as it
        ///     is less secure. The JWK x5c parameter MAY be used to provide X.509 representations
        ///     of keys provided. When used, the bare key values MUST still be present and MUST
        ///     match those in the certificate.
        /// </summary>
        public const string JsonWebKeySetUri = "jwks_uri";

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndorsementsRetriever"/> class.
        /// </summary>
        /// <param name="httpClient">Allow the calling layer to manage the lifetime of the HttpClient, complete with
        /// timeouts, pooling, instancing and so on. This is to avoid having to Use/Dispose a new instance
        /// of the client on each call, which may be very expensive in terms of latency, TLS connections
        /// and related issues.</param>
        public EndorsementsRetriever(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Retrieves a populated configuration given an address and a document retriever.
        /// </summary>
        /// <param name="address">Address of the discovery document.</param>
        /// <param name="retriever">The document retriever to use to read the discovery document.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// a populated configuration.</remarks>
        public async Task<IDictionary<string, HashSet<string>>> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancellationToken)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (retriever == null)
            {
                throw new ArgumentNullException(nameof(retriever));
            }

            var jsonDocument = await retriever.GetDocumentAsync(address, cancellationToken).ConfigureAwait(false);
            var configurationRoot = JObject.Parse(jsonDocument);

            var keys = configurationRoot["keys"]?.Value<JArray>();

            if (keys == null)
            {
                return new Dictionary<string, HashSet<string>>(0);
            }

            var results = new Dictionary<string, HashSet<string>>(keys.Count);

            foreach (var key in keys)
            {
                var keyId = key[AuthenticationConstants.KeyIdHeader]?.Value<string>();

                if (keyId != null
                        &&
                   !results.ContainsKey(keyId))
                {
                    var endorsementsToken = key["endorsements"];

                    if (endorsementsToken != null)
                    {
                        results.Add(keyId, new HashSet<string>(endorsementsToken.Values<string>()));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Obtains a document from an address.
        /// </summary>
        /// <param name="address">location of document.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// the document as a string.</remarks>
        public async Task<string> GetDocumentAsync(string address, CancellationToken cancellationToken)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            using (var documentResponse = await _httpClient.GetAsync(address, cancellationToken).ConfigureAwait(false))
            {
                if (!documentResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"An non-success status code of {documentResponse.StatusCode} was received while fetching the endorsements document.");
                }

                var json = await documentResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return string.Empty;
                }

                var obj = JObject.Parse(json);
                var keysUrl = obj[JsonWebKeySetUri]?.Value<string>();

                if (keysUrl == null)
                {
                    return string.Empty;
                }

                using (var keysResponse = await _httpClient.GetAsync(keysUrl, cancellationToken).ConfigureAwait(false))
                {
                    if (!keysResponse.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"An non-success status code of {keysResponse.StatusCode} was received while fetching the web key set document.");
                    }

                    return await keysResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
