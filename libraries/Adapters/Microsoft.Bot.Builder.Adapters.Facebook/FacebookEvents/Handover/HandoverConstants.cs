// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover
{
    /// <summary>
    /// Constants used as part of the Facebook handover protocol.
    /// </summary>
    public static class HandoverConstants
    {
        /// <summary>
        /// Constant for the pass_thread_control webhook event.
        /// </summary>
        public const string PassThreadControl = "pass_thread_control";

        /// <summary>
        /// Constant for the request_thread_control webhook event.
        /// </summary>
        public const string RequestThreadControl = "request_thread_control";

        /// <summary>
        /// Constant for the take_thread_control webhook event.
        /// </summary>
        public const string TakeThreadControl = "take_thread_control";

        /// <summary>
        /// Constant value for the inbox ID of any page in Facebook.
        /// </summary>
        public const string PageInboxId = "263902037430900";

        /// <summary>
        /// Constant for passing thread control to be shared between all bots using the handover protocol.
        /// </summary>
        public const string MetadataPassThreadControl = "PassThreadControl";

        /// <summary>
        /// Constant for requesting thread control to be shared between all bots using the handover protocol.
        /// </summary>
        public const string MetadataRequestThreadControl = "RequestThreadControl";

        /// <summary>
        /// Constant for taking thread control to be shared between all bots using the handover protocol.
        /// </summary>
        public const string MetadataTakeThreadControl = "TakeThreadControl";
    }
}
