// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public static class ResourceExtensions
    {
        /// <summary>
        /// Register ResourceExplorer and optionally register more types.
        /// </summary>
        /// <param name="dialogManager">BotAdapter to add middleware to.</param>
        /// <param name="resourceExplorer">resourceExplorer to use.</param>
        /// <returns>The bot adapter.</returns>
        public static DialogManager UseResourceExplorer(this DialogManager dialogManager, ResourceExplorer resourceExplorer)
        {
            if (resourceExplorer == null)
            {
                throw new ArgumentNullException(nameof(resourceExplorer));
            }

            dialogManager.TurnState.Add(resourceExplorer);

            return dialogManager;
        }
    }
}
