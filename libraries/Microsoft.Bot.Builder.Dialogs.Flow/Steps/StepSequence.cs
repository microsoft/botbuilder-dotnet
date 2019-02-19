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
    public class Sequence : List<IStep>, IStep
    {
        public Sequence(string id = null)
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

            var state = dialogContext.DialogState;
            state.TryGetValue(this.currentIdLabel, out object obj);
            string currentId = (string)obj;

            if (currentId == "END_SEQUENCE")
            {
                // sequence is done
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
                        
                        // remember next step to process
                        if (i < this.Count)
                        {
                            currentId = this[i].Id;
                            state[this.currentIdLabel] = currentId;
                        }
                        else
                        {
                            state[this.currentIdLabel] = "END_SEQUENCE";
                        }

                        // execute dialog command
                        var commandResult = await command.Execute(dialogContext, cancellationToken).ConfigureAwait(false);
                        if (commandResult is DialogTurnResult)
                        {
                            return commandResult as DialogTurnResult;
                        }

                        if (commandResult is Task<object>)
                        {
                            commandResult = ((Task<object>)commandResult).Result;
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
