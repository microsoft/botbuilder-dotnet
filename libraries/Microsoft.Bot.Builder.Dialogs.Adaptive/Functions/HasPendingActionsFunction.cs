// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Functions
{
    /// <summary>
    /// Defines hasPendingActions() expression function.
    /// </summary>
    /// <remarks>
    /// This expression will return true if the current adaptive dialog has any pending actions.
    /// </remarks>
    public class HasPendingActionsFunction : ExpressionEvaluator
    {
        public const string Name = "hasPendingActions";

        public HasPendingActionsFunction()
            : base(Name, Function, ReturnType.Boolean)
        {
        }

        private static (object value, string error) Function(Expression expression, IMemory state, Options options)
        {
            if (state.TryGetValue("dialog._adaptive.actions", out object val))
            {
                if (val != null)
                {
                    JArray actions = JArray.FromObject(val);
                    if (actions != null)
                    {
                        return (actions.Count > 0, null);
                    }
                }
            }

            return (false, null);
        }
    }
}
