using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Execute set of commands in sequence
    /// </summary>
    /// <remarks>
    /// Commands will be processed as long as there DialogTurnResult.Status == Complete
    /// </remarks>
    public class CommandSet : List<IDialogAction>, IDialogAction
    {
        public CommandSet(string id = null)
        {
            this.Id = id;
        }

        /// <summary>
        /// Id of the command
        /// </summary>
        public string Id { get; set; }

        private string currentIdLabel {  get { return $"{this.Id}.CurrentCommandId"; } }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(this.Id))
            {
                throw new ArgumentNullException(nameof(this.Id));
            }

            var state = dialogContext.ActiveDialog.State;
            state.TryGetValue(this.currentIdLabel, out object obj);
            string currentId = (string)obj;

            if (currentId == "END_DIALOG")
            {
                // done
                return null; 
            }

            // While we are in completed state process the commandSet.  
            bool done = true;
            do
            {
                done = true;
                for (int i = 0; i < this.Count; )
                {
                    var command = this[i++];
                    if (currentId == null || command.Id == currentId)
                    {
                        System.Diagnostics.Trace.WriteLine($"{command.GetType().Name}.{currentId}");
                        
                        // save next command
                        if (i < this.Count)
                        {
                            currentId = this[i].Id;
                            state[this.currentIdLabel] = currentId;
                        }
                        else
                        {
                            state[this.currentIdLabel] = "END_DIALOG";
                        }

                        // execute dialog command
                        var commandResult = await command.Execute(dialogContext, cancellationToken).ConfigureAwait(false);
                        if (commandResult is DialogTurnResult)
                        {
                            return commandResult as DialogTurnResult;
                        }

                        if (commandResult is string && !String.IsNullOrEmpty((string)commandResult))
                        {
                            currentId = (string)commandResult;
                            done = false;
                            break; // go up and restart the command loop with new starting point
                        }
                    }
                }
            } while (!done);
            // hit end of command, remove state 
            state.Remove(this.currentIdLabel);
            return null;
        }
    }
}
