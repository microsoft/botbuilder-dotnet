// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    internal class ActionPolicyException : Exception
    {
        public ActionPolicyException(ActionPolicy actionPolicy, Dialog dialog = null)
        {
            this.ActionPolicy = actionPolicy;
        }

        public ActionPolicy ActionPolicy { get; set; }
        
        public Dialog Dialog { get; set; }
    }
}
