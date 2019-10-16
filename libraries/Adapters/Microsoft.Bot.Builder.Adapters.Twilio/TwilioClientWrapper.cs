// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
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
        /// Initializes a new instance of the <see cref="TwilioClientWrapper"/> class.
        /// </summary>
        /// <param name="options">An object containing API credentials, a webhook verification token and other options.</param>
        public TwilioClientWrapper(TwilioAdapterOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.TwilioNumber))
            {
                throw new ArgumentException("TwilioNumber is a required part of the configuration.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.AccountSid))
            {
                throw new ArgumentException("AccountSid is a required part of the configuration.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.AuthToken))
            {
                throw new ArgumentException("AuthToken is a required part of the configuration.", nameof(options));
            }

            TwilioClient.Init(Options.AccountSid, Options.AuthToken);
        }

        public TwilioAdapterOptions Options { get; private set; }

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
