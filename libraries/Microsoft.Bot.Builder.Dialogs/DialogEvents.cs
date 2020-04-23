// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogEvents
    {
        public const string BeginDialog = "beginDialog";
        public const string RepromptDialog = "repromptDialog";
        public const string CancelDialog = "cancelDialog";
        public const string ActivityReceived = "activityReceived";
        public const string Error = "error";
        public const string BeginTemplateEvaluation = "beginTemplateEvaluation";
        public const string BeginExpressionEvaluation = "beginExpressionEvaluation";
        public const string Message = "message";
    }
}
