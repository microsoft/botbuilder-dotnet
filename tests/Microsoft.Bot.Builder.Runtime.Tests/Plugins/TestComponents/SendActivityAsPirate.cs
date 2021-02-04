using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Runtime.Tests.Plugins.TestComponents
{
    public class SendActivityAsPirate : Dialog
    {
        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
