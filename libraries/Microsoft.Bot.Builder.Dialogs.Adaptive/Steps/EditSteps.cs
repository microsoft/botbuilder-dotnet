using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Class which allows you to edit the current steps 
    /// </summary>
    public class EditSteps : DialogCommand, IDialogDependencies
    {
        /// <summary>
        /// Gets or sets the steps to be applied to the active steps.
        /// </summary>
        [JsonProperty("steps")]
        public List<IDialog> Steps { get; set; } = new List<IDialog>();

        /// <summary>
        /// Gets or sets the type of change to appy to the active steps.
        /// </summary>
        [JsonProperty("changeType")]
        public StepChangeTypes ChangeType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditSteps"/> class.
        /// </summary>
        [JsonConstructor]
        public EditSteps([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {

            if (dc is SequenceContext sc)
            {
                var planSteps = Steps.Select(s => new StepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = options
                });

                var changes = new StepChangeList()
                {
                    ChangeType = ChangeType,
                    Steps = planSteps.ToList()
                };

                if (this.ChangeType == StepChangeTypes.InsertStepsBeforeTags)
                {
                    changes.Tags = this.Tags;
                }

                sc.QueueChanges(changes);

                return await sc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("`EditStep` should only be used in the context of an adaptive dialog.");
            }
        }

        protected override string OnComputeId()
        {
            var idList = Steps.Select(s => s.Id);
            return $"{nameof(EditSteps)}({this.ChangeType}|{string.Join(",", idList)})";
        }

        public override List<IDialog> ListDependencies()
        {
            return this.Steps;
        }

    }
}
