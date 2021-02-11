// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    [TestClass]
    [TestCategory("FunctionalTests")]
    [TestCategory("Adapters")]
#if !AUTOMATEDBUILD
    [Ignore]
#endif
    public class TwilioNumberTests
    {
        private string _botEndpoint;
        private string _senderNumber;
        private string _twilioNumber;
        private string _twilioAuthToken;
        private string _twilioAccountSid;

        [TestMethod]
        public async Task SendAndReceiveSmsShouldSucceed()
        {
            GetEnvironmentVars();
            TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
            var echoGuid = Guid.NewGuid().ToString();
            await SendMessageAsync(echoGuid);
            var response = RetrieveMessage();

            Assert.IsTrue(response.Contains($"Echo: {echoGuid}"));
        }

        private async Task SendMessageAsync(string message)
        {
            var parameters = GetParameters(message);
            var signature = ComputeSignature(parameters);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Twilio-Signature", signature);

                await client.PostAsync(_botEndpoint, new FormUrlEncodedContent(parameters));
            }
        }

        private string ComputeSignature(IDictionary<string, string> parameters)
        {
            var toEncode = new StringBuilder(_botEndpoint);
            
            if (parameters != null)
            {
                var sortedKeys = new List<string>(parameters.Keys);
                sortedKeys.Sort(StringComparer.Ordinal);

                foreach (var key in sortedKeys)
                {
                    toEncode.Append(key).Append(parameters[key] ?? string.Empty);
                }
            }

            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_twilioAuthToken));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(toEncode.ToString()));
            
            return Convert.ToBase64String(hash);
        }

        private string RetrieveMessage()
        {
            System.Threading.Thread.Sleep(60000);
            var lastMessage = MessageResource.Read(limit: 10).First().Body;
                
            if (lastMessage.Contains("Echo: "))
            {
                return lastMessage;
            }

            throw new InvalidOperationException("Echo message not found");
        }

        private Dictionary<string, string> GetParameters(string message)
        {
            var parameters = new Dictionary<string, string>
            {
                { "Body", message },
                { "From", _senderNumber }
            };

            return parameters;
        }

        private void GetEnvironmentVars()
        {
            if (string.IsNullOrWhiteSpace(_twilioNumber) || string.IsNullOrWhiteSpace(_twilioAuthToken) || string.IsNullOrWhiteSpace(_twilioAccountSid) || string.IsNullOrWhiteSpace(_botEndpoint) || string.IsNullOrWhiteSpace(_senderNumber))
            {
                _twilioNumber = Environment.GetEnvironmentVariable("TWILIO_NUMBER");
                if (string.IsNullOrWhiteSpace(_twilioNumber))
                {
                    Assert.Fail("Environment variable 'TwilioNumber' not found.");
                }

                _twilioAuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
                if (string.IsNullOrWhiteSpace(_twilioAuthToken))
                {
                    Assert.Fail("Environment variable 'TWILIO_AUTH_TOKEN' not found.");
                }

                _twilioAccountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
                if (string.IsNullOrWhiteSpace(_twilioAccountSid))
                {
                    Assert.Fail("Environment variable 'TWILIO_ACCOUNT_SID' not found.");
                }

                _senderNumber = Environment.GetEnvironmentVariable("SENDER_NUMBER");
                if (string.IsNullOrWhiteSpace(_senderNumber))
                {
                    Assert.Fail("Environment variable 'SENDER_NUMBER' not found.");
                }

                _botEndpoint = Environment.GetEnvironmentVariable("TwilioValidationUrl");
                if (string.IsNullOrWhiteSpace(_botEndpoint))
                {
                    Assert.Fail("Environment variable 'TwilioValidationUrl' not found.");
                }
            }
        }
    }
}
