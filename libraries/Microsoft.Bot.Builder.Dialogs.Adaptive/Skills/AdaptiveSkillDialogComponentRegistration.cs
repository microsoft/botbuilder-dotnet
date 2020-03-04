// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Skills
{
    public class AdaptiveSkillDialogComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public IEnumerable<DeclarativeType> GetDeclarativeTypes()
        {
            yield return new DeclarativeType<AdaptiveSkillDialog>(AdaptiveSkillDialog.DeclarativeType);
        }

        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, Stack<string> paths)
        {
            yield break;
        }
    }
}
