// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Security;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Wrapper class for the Twilio API.
    /// </summary>
    public class TwilioClientWrapper
    {
        private const string TwilioSignature = "x-twilio-signature";
        private const string TwilioHeader = "x-forwarded-proto";

        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioClientWrapper"/> class.
        /// </summary>
        /// <param name="options">An object containing API credentials, a webhook verification token and other options.</param>
        public TwilioClientWrapper(TwilioClientWrapperOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            
            TwilioClient.Init(Options.TwilioAccountSid, Options.TwilioAuthToken);
        }

        /// <summary>
        /// Gets the <see cref="TwilioClientWrapperOptions"/> for the wrapper. 
        /// </summary>
        /// <value>
        /// The <see cref="TwilioClientWrapperOptions"/> for the wrapper.
        /// </value>
        public TwilioClientWrapperOptions Options { get; }

        /// <summary>
        /// Sends a Twilio SMS message.
        /// </summary>
        /// <param name="messageOptions">An object containing the parameters for the message to send.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The SID of the Twilio message sent.</returns>
        public virtual async Task<string> SendMessageAsync(TwilioMessageOptions messageOptions, CancellationToken cancellationToken)
        {
            var createMessageOptions = new CreateMessageOptions(messageOptions.To)
            {
                ApplicationSid = messageOptions.ApplicationSid,
                MediaUrl = messageOptions.MediaUrl,
                Body = messageOptions.Body,
                From = messageOptions.From
            };

            var messageResource = await MessageResource.CreateAsync(createMessageOptions).ConfigureAwait(false);
            return messageResource.Sid;
        }

        /// <summary>
        /// Validates an HTTP request as coming from Twilio.
        /// </summary>
        /// <param name="httpRequest">The request to validate.</param>
        /// <param name="body">The request payload, as key-value pairs.</param>
        /// <returns>The result of the comparison between the signature in the request and the hashed body.</returns>
        public virtual bool ValidateSignature(HttpRequest httpRequest, Dictionary<string, string> body)
        {
            var urlString = Options.TwilioValidationUrl?.ToString();
            
            var twilioSignature = httpRequest.Headers.ContainsKey(TwilioSignature)
                ? httpRequest.Headers[TwilioSignature].ToString()
                : throw new ArgumentNullException($"HttpRequest is missing \"{TwilioSignature}\"");

            if (string.IsNullOrWhiteSpace(urlString))
            {
                urlString = httpRequest.Headers[TwilioHeader][0];
                if (string.IsNullOrWhiteSpace(urlString))
                {
                    urlString = $"{httpRequest.Protocol}://{httpRequest.Host + httpRequest.Path}";
                }
            }

            var requestValidator = new RequestValidator(Options.TwilioAuthToken);

            return requestValidator.Validate(urlString, body, twilioSignature);
        }
    }
}
