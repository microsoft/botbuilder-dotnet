// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Skills
{
    public class SkillTurnResult
    {
        public SkillTurnResult(SkillTurnStatus status, object result = null)
        {
            Status = status;
            Result = result;
        }

        public SkillTurnStatus Status { get; set; }

        public object Result { get; set; }
    }
}
