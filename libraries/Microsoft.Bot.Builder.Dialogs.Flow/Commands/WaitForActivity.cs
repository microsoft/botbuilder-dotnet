using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Wait for input from the user
    /// </summary>
    public class WaitForActivity : IDialogCommand
    {
        public WaitForActivity() { }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("n");

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
    }
}
