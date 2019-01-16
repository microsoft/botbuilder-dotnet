using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Set State variable as an action
    /// </summary>
    public class GotoCommand : IDialogCommand
    {
        public GotoCommand() { }

        /// <summary>
        /// Id of the command
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("n");

        /// <summary>
        /// Command to go to 
        /// </summary>
        public string CommandId { get; set; }

        public Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(CommandId);
        }
    }
}
