// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public class TwilioApi : ITwilioClient
    {
        /// <summary>
        /// Initialize the Twilio client by supplying the username and password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public void LogIn(string username, string password)
        {
            TwilioClient.Init(username, password);
        }

        /// <summary>
        /// Sends a message and returns its resource ID.
        /// </summary>
        /// <param name="messageOptions">Object that represents the Twilio message options.</param>
        /// <returns>ID from the created Twilio message.</returns>
        public async Task<string> SendMessage(CreateMessageOptions messageOptions)
        {
            var messageResource = await MessageResource.CreateAsync((CreateMessageOptions)messageOptions).ConfigureAwait(false);
            return messageResource.Sid;
        }
    }
}
