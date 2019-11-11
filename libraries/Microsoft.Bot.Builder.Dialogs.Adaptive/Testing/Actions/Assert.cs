using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions
{
    public class Assert : Dialog
    {
        /// <summary>
        /// Gets or sets condition which must be true.
        /// </summary>
        /// <value>
        /// Condition which must be true.
        /// </value>
        public string Condition { get; set; }

        /// <summary>
        /// Gets or sets description of assertion.
        /// </summary>
        /// <value>
        /// Description of assertion.
        /// </value>
        public string Description { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var (result, error) = new ExpressionEngine().Parse(Condition).TryEvaluate(dc.GetState());
            if ((bool)result == false)
            {
                var desc = await new TemplateEngineLanguageGenerator(this.Description)
                    .Generate(dc.Context, this.Description, dc.GetState()).ConfigureAwait(false);
                throw new Exception(desc);
            }

            return await dc.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
