using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Class which allows you to edit the current actions. 
    /// </summary>
    public class EditActions : Dialog, IDialogDependencies
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditActions"/> class.
        /// </summary>
        /// <param name="sourceFilePath">Source file.</param>
        /// <param name="sourceLineNumber">Line number.</param>
        [JsonConstructor]
        public EditActions([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets the actions to be applied to the active action.
        /// </summary>
        /// <value>
        /// The actions to be applied to the active action.
        /// </value>
        [JsonProperty("actions")]
        public List<Dialog> Actions { get; set; } = new List<Dialog>();

        /// <summary>
        /// Gets or sets the type of change to appy to the active actions.
        /// </summary>
        /// <value>
        /// The type of change to appy to the active actions.
        /// </value>
        [JsonProperty("changeType")]
        public ActionChangeType ChangeType { get; set; }

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            return this.Actions;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc is SequenceContext sc)
            {
                var planActions = Actions.Select(s => new ActionState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = options
                });

                var changes = new ActionChangeList()
                {
                    ChangeType = ChangeType,
                    Actions = planActions.ToList()
                };

                sc.QueueChanges(changes);

                return await sc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("`EditAction` should only be used in the context of an adaptive dialog.");
            }
        }

        protected override string OnComputeId()
        {
            var idList = Actions.Select(s => s.Id);
            return $"{this.GetType().Name}[{this.ChangeType}|{string.Join(",", idList)}]";
        }
    }
}
