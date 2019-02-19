using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Builder.Dialogs.Flow;

namespace Microsoft.Bot.Builder.TestBot.Json.CCI
{
    public class SwitchStep2 : IStep
    {
        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Cases to compare against result of condition expression
        /// </summary>
        public IList<Tuple<IExpressionEval, IStep>> Routes { get; set; } = new List<Tuple<IExpressionEval, IStep>>();

        /// <summary>
        /// Default action to take if no match
        /// </summary>
        public IStep DefaultAction { get; set; }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            foreach (var route in Routes)
            {
                var result = await route.Item1.Evaluate(dialogContext.UserState);
                bool resultAsBool = (bool)Convert.ChangeType(result, typeof(bool));
                if (resultAsBool)
                {
                    return route.Item2.Execute(dialogContext, cancellationToken);
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
