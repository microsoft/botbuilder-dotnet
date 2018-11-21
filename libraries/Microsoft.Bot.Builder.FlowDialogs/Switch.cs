using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.ComposableDialogs;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.FlowDialogs
{
    /// <summary>
    /// Evaluate expression and execute actions based on the result
    /// </summary>
    /// <typeparam name="ValueT"></typeparam>
    public class Switch : IFlowAction
    {
        public Switch() { }

        /// <summary>
        /// Control whether case sensitive or not
        /// </summary>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Expression to evalute
        /// </summary>
        public IExpressionEval Condition { get; set; }

        /// <summary>
        /// Cases to compare against result of condition expression
        /// </summary>
        public Dictionary<string, IFlowAction> Cases { get; set; } = new Dictionary<string, IFlowAction>();

        /// <summary>
        /// Default action to take if no match
        /// </summary>
        public IFlowAction DefaultAction { get; set; }

        public async Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            var state = dialogContext.ActiveDialog.State;
            state["DialogTurnResult"] = result;

            var conditionResult = await Condition.Evaluate(state);

            foreach (var key in this.Cases.Keys)
            {
                if (IgnoreCase && conditionResult is string)
                {
                    if (String.Equals((string)conditionResult, key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return await this.Cases[key].Execute(dialogContext, options, result, cancellationToken);
                    }
                }
                else
                {
                    var typeCode = Type.GetTypeCode(conditionResult.GetType());
                    if (conditionResult.Equals(Convert.ChangeType(key, typeCode)))
                    {
                        return await this.Cases[key].Execute(dialogContext, options, result, cancellationToken);
                    }
                }
            }

            if (DefaultAction != null)
            {
                return await this.DefaultAction.Execute(dialogContext, options, result, cancellationToken);
            }

            // just return original result
            return result;
        }
    }
}
