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
    public sealed class EndorsementsRetriever : IDocumentRetriever, IConfigurationRetriever<IDictionary<string, string[]>>
    {
        public async Task<IDictionary<string, string[]>> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            var res = await retriever.GetDocumentAsync(address, cancel);
            var obj = JsonConvert.DeserializeObject<JObject>(res);
            if (obj != null && obj.HasValues && obj["keys"] != null)
            {
                var keys = obj.SelectToken("keys").Value<JArray>();
                var endorsements = keys.Where(key => key["endorsements"] != null).Select(key => Tuple.Create(key.SelectToken("kid").Value<string>(), key.SelectToken("endorsements").Values<string>()));
                return endorsements.Distinct(new EndorsementsComparer()).ToDictionary(item => item.Item1, item => item.Item2.ToArray());
            }
            else
            {
                return new Dictionary<string, string[]>();
            }
        }

        public async Task<string> GetDocumentAsync(string address, CancellationToken cancel)
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(address, cancel))
                {
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    JObject obj = JsonConvert.DeserializeObject<JObject>(json);
                    if (obj != null && obj.HasValues && obj["jwks_uri"] != null)
                    {
                        var keysUrl = obj.SelectToken("jwks_uri").Value<string>();
                        using (var keysResponse = await client.GetAsync(keysUrl, cancel))
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
