// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Represents a Twilio client.
    /// </summary>
    public interface ITwilioClient
    {
        /// <summary>
        /// Initializes the Twilio client with a user name and password.
        /// </summary>
        /// <param name="username">The user name for the Twilio API.</param>
        /// <param name="password">The password for the Twilio API.</param>
        void LogIn(string username, string password);

        /// <summary>
        /// Sends a Twilio SMS message.
        /// </summary>
        /// <param name="messageOptions">An object containing the parameters for the message to send.</param>
        /// <returns>The SID of the Twilio message sent.</returns>
        /// <remarks><paramref name="messageOptions"/> should be a <see cref="CreateMessageOptions"/> object.</remarks>
        Task<string> GetResourceIdentifier(object messageOptions);
    }
}
