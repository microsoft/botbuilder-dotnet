// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Wrapper class for the Twilio API.
    /// </summary>
    public class TwilioClientWrapper
    {
        /// <summary>
        /// Initializes the Twilio client with a user name and password.
        /// </summary>
        /// <param name="username">The user name for the Twilio API.</param>
        /// <param name="password">The password for the Twilio API.</param>
        public virtual void LogIn(string username, string password)
        {
            TwilioClient.Init(username, password);
        }

        /// <summary>
        /// Sends a Twilio SMS message.
        /// </summary>
        /// <param name="messageOptions">An object containing the parameters for the message to send.</param>
        /// <returns>The SID of the Twilio message sent.</returns>
        public virtual async Task<string> SendMessage(CreateMessageOptions messageOptions)
        {
            var messageResource = await MessageResource.CreateAsync((CreateMessageOptions)messageOptions).ConfigureAwait(false);
            return messageResource.Sid;
        }
    }
}
