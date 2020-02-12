// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills
{
    public class SkillDialogComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public IEnumerable<DeclarativeType> GetDeclarativeTypes()
        {
            yield return new DeclarativeType<BeginSkillDialog>(BeginSkillDialog.DeclarativeType);
        }

        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, Stack<string> paths)
        {
            yield break;
        }
    }
}
