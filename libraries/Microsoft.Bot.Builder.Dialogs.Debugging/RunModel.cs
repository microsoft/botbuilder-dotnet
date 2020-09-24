// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    internal sealed class RunModel
    {
        public DialogDebugAdapter.Phase? PhaseSent { get; set; }

        public DialogDebugAdapter.Phase Phase { get; set; } = DialogDebugAdapter.Phase.Started;

        public object Gate { get; } = new object();

        public void Post(DialogDebugAdapter.Phase what)
        {
            Monitor.Enter(Gate);
            try
            {
                Phase = what;
                Monitor.Pulse(Gate);
            }
            finally
            {
                Monitor.Exit(Gate);
            }
        }
    }
}
