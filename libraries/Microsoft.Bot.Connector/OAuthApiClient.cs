using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector
{
    public class OAuthClient : ServiceClient<OAuthClient>
    {
        private ConnectorClient client;
        private string uri;


        public OAuthClient(ConnectorClient client, string uri)
        {
            this.client = client;
            this.uri = uri;
        }

        public async Task<TokenResponse> GetUserTokenAsync(string userId, string connectionName, string magicCode, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connectionName == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "connectionName");
            }
            if (userId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "userId");
            }

            // Tracing
            bool _shouldTrace = ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("userId", userId);
                tracingParameters.Add("connectionName", connectionName);
                tracingParameters.Add("magicCode", magicCode);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(_invocationId, this, "GetUserTokenAsync", tracingParameters);
            }
            // Construct URL
            var _url = new Uri(new System.Uri(uri + (uri.EndsWith("/") ? "" : "/")), "api/usertoken/GetToken?userId={userId}&connectionName={connectionName}{magicCodeParam}").ToString();
            _url = _url.Replace("{connectionName}", System.Uri.EscapeDataString(connectionName));
            _url = _url.Replace("{userId}", System.Uri.EscapeDataString(userId));
            if (!string.IsNullOrEmpty(magicCode))
            {
                _url = _url.Replace("{magicCodeParam}", $"&code={System.Uri.EscapeDataString(magicCode)}");
            }
            else
            {
                _url = _url.Replace("{magicCodeParam}", String.Empty);
            }

            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("GET");
            _httpRequest.RequestUri = new System.Uri(_url);

            MicrosoftAppCredentials.TrustServiceUrl(_url);

            // Set Credentials
            if (client.Credentials != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await client.Credentials.ProcessHttpRequestAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();

            if (_shouldTrace)
            {
                ServiceClientTracing.SendRequest(_invocationId, _httpRequest);
            }
            _httpResponse = await client.HttpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            if (_shouldTrace)
            {
                ServiceClientTracing.ReceiveResponse(_invocationId, _httpResponse);
            }
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if (_statusCode == HttpStatusCode.OK)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var tokenResponse = Rest.Serialization.SafeJsonConvert.DeserializeObject<TokenResponse>(_responseContent);
                    return tokenResponse;
                }
                catch (JsonException)
                {
                    _httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    return null;
                }
            }
            else if (_statusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> SignOutUserAsync(string userId, string connectionName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connectionName == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "connectionName");
            }
            if (userId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "userId");
            }

            bool _shouldTrace = ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("userId", userId);
                tracingParameters.Add("connectionName", connectionName);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(_invocationId, this, "SignOutUserAsync", tracingParameters);
            }

            // Construct URL
            var _url = new System.Uri(new System.Uri(uri + (uri.EndsWith("/") ? "" : "/")), "api/usertoken/SignOut?&userId={userId}&connectionName={connectionName}").ToString();
            _url = _url.Replace("{connectionName}", System.Uri.EscapeDataString(connectionName));
            _url = _url.Replace("{userId}", System.Uri.EscapeDataString(userId));

            MicrosoftAppCredentials.TrustServiceUrl(_url);

            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("DELETE");
            _httpRequest.RequestUri = new System.Uri(_url);

            // Set Credentials
            if (client.Credentials != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await client.Credentials.ProcessHttpRequestAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();

            if (_shouldTrace)
            {
                ServiceClientTracing.SendRequest(_invocationId, _httpRequest);
            }
            _httpResponse = await client.HttpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            if (_shouldTrace)
            {
                ServiceClientTracing.ReceiveResponse(_invocationId, _httpResponse);
            }

            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            if (_statusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> GetSignInLinkAsync(IActivity activity, string connectionName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connectionName == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "connectionName");
            }
            if (activity == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "activity");
            }

            bool _shouldTrace = ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("activity", activity);
                tracingParameters.Add("connectionName", connectionName);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(_invocationId, this, "GetSignInLinkAsync", tracingParameters);
            }

            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = new ConversationReference()
                {
                    ActivityId = activity.Id,
                    Bot = activity.Recipient,       // Activity is from the user to the bot
                    ChannelId = activity.ChannelId,
                    Conversation = activity.Conversation,
                    ServiceUrl = activity.ServiceUrl,
                    User = activity.From
                },
                MsAppId = (client.Credentials as MicrosoftAppCredentials)?.MicrosoftAppId
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var finalState = Convert.ToBase64String(encodedState);

            // Construct URL
            var _url = new System.Uri(new System.Uri(uri + (uri.EndsWith("/") ? "" : "/")), "api/botsignin/getsigninurl?&state={state}").ToString();
            _url = _url.Replace("{state}", finalState);

            MicrosoftAppCredentials.TrustServiceUrl(_url);

            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("GET");
            _httpRequest.RequestUri = new System.Uri(_url);

            // Set Credentials
            if (client.Credentials != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await client.Credentials.ProcessHttpRequestAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();

            if (_shouldTrace)
            {
                ServiceClientTracing.SendRequest(_invocationId, _httpRequest);
            }
            _httpResponse = await client.HttpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            if (_shouldTrace)
            {
                ServiceClientTracing.ReceiveResponse(_invocationId, _httpResponse);
            }

            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            if (_statusCode == HttpStatusCode.OK)
            {
                var link = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                return link;
            }
            return String.Empty;
        }

        public async Task SendEmulateOAuthCardsAsync(bool emulateOAuthCards)
        {
            bool _shouldTrace = ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("emulateOAuthCards", emulateOAuthCards);
                ServiceClientTracing.Enter(_invocationId, this, "SendEmulateOAuthCards", tracingParameters);
            }

            var cancellationToken = default(CancellationToken);
            // Construct URL
            var _url = new System.Uri(new System.Uri(uri + (uri.EndsWith("/") ? "" : "/")), "api/usertoken/emulateOAuthCards?emulate={emulate}").ToString();
            _url = _url.Replace("{emulate}", emulateOAuthCards.ToString());

            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("POST");
            _httpRequest.RequestUri = new System.Uri(_url);

            MicrosoftAppCredentials.TrustServiceUrl(_url);

            // Set Credentials
            if (client.Credentials != null)
            {
                await client.Credentials.ProcessHttpRequestAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            }

            if (_shouldTrace)
            {
                ServiceClientTracing.SendRequest(_invocationId, _httpRequest);
            }
            _httpResponse = await client.HttpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            if (_shouldTrace)
            {
                ServiceClientTracing.ReceiveResponse(_invocationId, _httpResponse);
            }
        }
    }
}
