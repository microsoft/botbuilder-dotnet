// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// The endorsements property within each key contains one or more endorsement 
    /// strings which you can use to verify that the channel ID specified in the channelId 
    /// property within the Activity object of the incoming request is authentic.
    /// More details at:
    ///     https://docs.microsoft.com/en-us/bot-framework/rest-api/bot-framework-rest-connector-authentication
    /// </summary>
    public sealed class EndorsementsRetriever : IDocumentRetriever, IConfigurationRetriever<IDictionary<string, string[]>>
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates an instance of the Endorsements Retriever class. 
        /// </summary>
        /// <param name="httpClient">Allow the calling layer to manage the lifetime of the HttpClient, complete with
        /// timeouts, pooling, instancing and so on. This is to avoid having to Use/Dispose a new instance
        /// of the client on each call, which may be very expensive in terms of latency, TLS connections
        /// and related issues.</param>
        public EndorsementsRetriever(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

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

        public async Task<IDictionary<string, string[]>> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            var res = await retriever.GetDocumentAsync(address, cancel);
            var obj = JsonConvert.DeserializeObject<JObject>(res);
            if (obj != null && obj.HasValues && obj["keys"] != null)
            {
                var keys = obj.SelectToken("keys").Value<JArray>();
                var endorsements = keys.Where(key => key["endorsements"] != null).Select(
                    key => Tuple.Create(
                        key.SelectToken(AuthenticationConstants.KeyIdHeader).Value<string>(),
                        key.SelectToken("endorsements").Values<string>()));

                return endorsements.Distinct(new EndorsementsComparer())
                    .ToDictionary(item => item.Item1, item => item.Item2.ToArray());
            }
            else
            {
                return new Dictionary<string, string[]>();
            }
        }

        public async Task<string> GetDocumentAsync(string address, CancellationToken cancel)
        {
            using (var response = await _httpClient.GetAsync(address, cancel))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                JObject obj = JsonConvert.DeserializeObject<JObject>(json);
                if (obj != null && obj.HasValues && obj[JsonWebKeySetUri] != null)
                {
                    var keysUrl = obj.SelectToken(JsonWebKeySetUri).Value<string>();
                    using (var keysResponse = await _httpClient.GetAsync(keysUrl, cancel))
                    {
                        keysResponse.EnsureSuccessStatusCode();
                        return await keysResponse.Content.ReadAsStringAsync();
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private class EndorsementsComparer : IEqualityComparer<Tuple<string, IEnumerable<string>>>
        {
            public bool Equals(Tuple<string, IEnumerable<string>> x, Tuple<string, IEnumerable<string>> y)
            {
                return x.Item1 == y.Item1;
            }
            public int GetHashCode(Tuple<string, IEnumerable<string>> obj)
            {
                return obj.Item1.GetHashCode();
            }
        }
    }
}
