// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Rules
{
    /// <summary>
    /// This rule fires when the utterance is not recognized and the fallback consultation is happening 
    /// It will only trigger if and when 
    /// * it is the leaf dialog AND 
    /// * none of the parent dialogs handle the event 
    /// This provides the parent dialogs the opportunity to handle global commands as fallback interruption
    /// </summary>
    public class UnknownIntentRule : EventRule
    {
        [JsonConstructor]
        public UnknownIntentRule(List<IDialog> steps = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(events: new List<string>()
            {
                AdaptiveEvents.UnknownIntent
            },
            steps: steps,
            constraint: constraint,
            callerPath: callerPath, callerLine: callerLine)
        {
        }

        protected override PlanChangeList OnCreateChangeList(PlanningContext planning, object dialogOptions = null)
        {
            var recognizerResult = planning.State.GetValue<RecognizerResult>("turn.dialogEvent.value");
            if (recognizerResult != null)
            {
                return new PlanChangeList()
                {
                    //ChangeType = this.ChangeType,
                    Desire = DialogConsultationDesire.CanProcess,
                    Steps = Steps.Select(s => new PlanStepState()
                    {
                        DialogStack = new List<DialogInstance>(),
                        DialogId = s.Id,
                        Options = dialogOptions
                    }).ToList()
                };
            }

            return new PlanChangeList()
            { 
                Steps = Steps.Select(s => new PlanStepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = dialogOptions
                }).ToList()
            };
        }
    }
}
