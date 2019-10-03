// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace SkillHost.Controllers
{
    public class SkillArgs
    {
        public SkillMethod Method { get; set; }

        public object[] Args { get; set; }

        public object Result { get; set; }
    }
}
