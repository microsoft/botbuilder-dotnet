using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Evaluate expression and execute actions based on the result
    /// </summary>
    /// <typeparam name="ValueT"></typeparam>
    public class SwitchStep : IStep
    {
        public SwitchStep() { }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; }

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
        public Dictionary<string, IStep> Cases { get; set; } = new Dictionary<string, IStep>();

        /// <summary>
        /// Default action to take if no match
        /// </summary>
        public IStep DefaultAction { get; set; }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            var state = dialogContext.DialogState;
            if (Condition == null)
            {
                throw new ArgumentNullException(nameof(Condition));
            }

            var conditionResult = await Condition.Evaluate(state);

            foreach (var key in this.Cases.Keys)
            {
                if (IgnoreCase && conditionResult is string)
                {
                    if (String.Equals((string)conditionResult, key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return await this.Cases[key].Execute(dialogContext, cancellationToken);
                    }
                }
                else
                {
                    var typeCode = Type.GetTypeCode(conditionResult.GetType());
                    if (conditionResult.Equals(Convert.ChangeType(key, typeCode)))
                    {
                        return await this.Cases[key].Execute(dialogContext, cancellationToken);
                    }
                }
            }

            if (DefaultAction != null)
            {
                return await this.DefaultAction.Execute(dialogContext, cancellationToken);
            }

            // fall through
            return null;
        }
    }
}
