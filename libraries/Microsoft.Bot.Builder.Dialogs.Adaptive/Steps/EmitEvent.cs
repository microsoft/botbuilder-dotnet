// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    public class EmitEvent : DialogCommand
    {
        private const string eventValueProperty = "eventValue";

        public string EventName { get; set; }
        public object EventValue { get; set; }
        public bool BubbleEvent { get; set; }

        public string EventValueProperty
        {
            get
            {
                if (InputProperties.ContainsKey(eventValueProperty))
                {
                    return InputProperties[eventValueProperty];
                }
                return string.Empty;
            }

            set
            {
                InputProperties[eventValueProperty] = value;
            }
        }

        public string ResultProperty
        {
            get
            {
                return OutputProperty;
            }

            set
            {
                OutputProperty = value;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var handled = await dc.EmitEventAsync(EventName, EventValue, BubbleEvent, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(handled, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"EmitEvent[{EventName ?? string.Empty}]";
        }
    }
}
