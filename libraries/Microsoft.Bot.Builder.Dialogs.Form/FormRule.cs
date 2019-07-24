using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class FormRule : EventRule
    {
        [JsonConstructor]
        public FormRule(List<IDialog> steps = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(events: new List<string> { AdaptiveEvents.RecognizedIntent }, constraint: constraint, steps: steps, callerPath: callerPath, callerLine: callerLine)
        {
        }

        public IList<string> Entities()
        {
            // TODO: Extract from constraints
            return null;
        }

        public IList<string> Slots()
        {
            // TODO: Extract from actions
            return null;
        }
    }
}
