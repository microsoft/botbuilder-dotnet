using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Functions
{
    public class IsDialogActiveFunction : ExpressionEvaluator
    {
        public const string Name = "isDialogActive";

        public IsDialogActiveFunction()
            : base(ExpressionType.Lambda, Evaluate, ReturnType.Boolean)
        {
        }

        public static (object value, string error) Evaluate(Expression expression, IMemory state, Options options)
        {
            if (state.TryGetValue("dialogcontext.stack", out object stackArray))
            {
                List<string> args = new List<string>();
                foreach (var child in expression.Children)
                {
                    var (value, error) = child.TryEvaluate<string>(state, options);
                    if (error != null)
                    {
                        throw new Exception(error);
                    }

                    args.Add(value);
                }

                foreach (var dlg in ((JArray)stackArray).ToObject<List<string>>())
                {
                    if (args.Contains(dlg))
                    {
                        // one of the dialogs was found in the stack.
                        return (true, null);
                    }
                }

                // none of the dialogs was found in the stack.
                return (false, null);
            }

            return (null, "DialogContext.Stack not found");
        }
    }
}
