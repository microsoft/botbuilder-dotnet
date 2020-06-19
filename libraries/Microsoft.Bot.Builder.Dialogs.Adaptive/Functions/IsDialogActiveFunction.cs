// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Functions
{
    /// <summary>
    /// Defines isDialogActive(id) expression function.
    /// </summary>
    /// <remarks>
    /// This expression will return true if any of the dialogIds is on the dialog execution stack.
    /// You can pass 1..N dialog ids.
    /// </remarks>
    /// <example>
    /// isDialogActive('dialog1')
    /// isDialogActive('dialog1', 'dialog2', 'dialog3').
    /// </example>
    public class IsDialogActiveFunction : ExpressionEvaluator
    {
        public const string Name = "isDialogActive";
        public const string Alias = "isActionActive";

        public IsDialogActiveFunction()
            : base(Name, Function, ReturnType.Boolean)
        {
        }

        private static (object value, string error) Function(Expression expression, IMemory state, Options options)
        {
            if (state.TryGetValue("dialogcontext.stack", out object stackArray))
            {
                List<string> args = new List<string>();
                foreach (var child in expression.Children)
                {
                    var (value, error) = child.TryEvaluate<string>(state, options);
                    if (error != null)
                    {
                        return (null, error);
                    }

                    args.Add(value);
                }

                foreach (var dlg in JArray.FromObject(stackArray))                
                {
                    if (args.Contains(dlg.ToString()))
                    {
                        // one of the dialog ids was found in the stack.
                        return (true, null);
                    }
                }

                // none of the dialog ids was found in the stack.
                return (false, null);
            }

            return (null, "DialogContext.Stack not found");
        }
    }
}
