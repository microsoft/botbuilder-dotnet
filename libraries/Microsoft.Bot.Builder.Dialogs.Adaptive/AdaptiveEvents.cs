// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class AdaptiveEvents : DialogEvents
    {
        public const string Error = "error";

        public const string RecognizedIntent = "recognizedIntent";
        public const string UnknownIntent = "unknownIntent";

        public const string NewConversation = "newConversation";
        public const string EndConversation = "endConversation";

        public const string NewUser = "newUser";
        public const string DeleteUser = "deleteUser";

        public const string MemberAdded = "memberAdded";
        public const string MemberRemoved = "memberRemoved";
    }
}
