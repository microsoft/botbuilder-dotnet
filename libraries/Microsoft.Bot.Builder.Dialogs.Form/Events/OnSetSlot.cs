using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Adaptive.TriggerHandlers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    /// <summary>
    /// Triggered when a form needs to set a slot to an entity.
    /// </summary>
    public class OnSetSlot : OnDialogEvent
    {
        [JsonConstructor]
        public OnSetSlot(List<Dialog> actions = null, string constraint = null, int priority = 0, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                events: new List<string>() { FormEvents.Ask },
                actions: actions,
                constraint: constraint,
                priority: priority,
                callerPath: callerPath,
                callerLine: callerLine)
        {
        }
    }
}
