// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class EmitEvent : DialogAction
    {
        private const string EventValuePropertyValue = "eventValue";

        [JsonConstructor]
        public EmitEvent(string eventName = null, object eventValue = null, bool bubble = true, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.EventName = eventName;
            this.EventValue = EventValue;
            this.BubbleEvent = bubble;
        }

        public string EventName { get; set; }

        public object EventValue { get; set; }

        public bool BubbleEvent { get; set; }

        public string EventValueProperty
        {
            get
            {
                if (InputBindings.ContainsKey(EventValuePropertyValue))
                {
                    return InputBindings[EventValuePropertyValue];
                }

                return string.Empty;
            }

            set
            {
                InputBindings[EventValuePropertyValue] = value;
            }
        }

        //public string ResultProperty
        //{
        //    get
        //    {
        //        return OutputBinding;
        //    }

        //    set
        //    {
        //        InputBindings[DialogContextState.DIALOG_VALUE] = value;
        //        OutputBinding = value;
        //    }
        //}

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            var handled = await dc.EmitEventAsync(EventName, EventValue, BubbleEvent, false, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(handled, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"EmitEvent[{EventName ?? string.Empty}]";
        }
    }
}
