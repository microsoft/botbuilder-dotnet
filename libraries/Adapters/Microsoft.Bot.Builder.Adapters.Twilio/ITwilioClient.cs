// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public interface ITwilioClient
    {
        /// <summary>
        /// Logs into the Twilio API service.
        /// </summary>
        /// <param name="username">The username for the Twilio API.</param>
        /// <param name="password">The password for the Twilio API.</param>
        void LogIn(string username, string password);

        /// <summary>
        /// Returns the resource ID from a message.
        /// </summary>
        /// <param name="messageOptions">Object that represents the Twilio message options.</param>
        /// <returns>ID from Twilio message.</returns>
        Task<string> SendMessage(CreateMessageOptions messageOptions);
    }
}
