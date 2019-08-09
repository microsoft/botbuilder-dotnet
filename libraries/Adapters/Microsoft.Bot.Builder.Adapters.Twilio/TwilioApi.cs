// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public class TwilioApi : ITwilioClient
    {
        public void LogIn(string username, string password)
        {
            TwilioClient.Init(username, password);
        }

        public async Task<string> GetResourceIdentifier(object messageOptions)
        {
            var messageResource = await MessageResource.CreateAsync((CreateMessageOptions)messageOptions).ConfigureAwait(false);
            return messageResource.Sid;
        }
    }
}
