// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Dialog optimized for prompting a user with a series of questions. Waterfalls accept a stack of
    /// functions which will be executed in sequence.Each waterfall step can ask a question of the user
    /// and the users response will be passed as an argument to the next waterfall step.
    /// </summary>
    public class Waterfall : IDialogContinue, IDialogResume
    {
        private WaterfallStep[] _steps;

        public Waterfall(WaterfallStep[] steps)
        {
            _steps = steps;
        }

        public Task DialogBegin(DialogContext dc, object dialogArgs = null)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));

            var instance = dc.Instance;
            instance.State = new WaterfallInstance { Step = 0 };
            return RunStep(dc, dialogArgs);
        }

        public async Task DialogContinue(DialogContext dc)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));

            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                var instance = (WaterfallInstance)dc.Instance.State;
                instance.Step++;
                await RunStep(dc, dc.Context.Activity.Text);
            }
        }

        public Task DialogResume(DialogContext dc, object result)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));

            var instance = (WaterfallInstance)dc.Instance.State;
            instance.Step++;
            return RunStep(dc, result);
        }

        private async Task RunStep(DialogContext dc, object result = null)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));

            var instance = (WaterfallInstance)dc.Instance.State;
            var step = instance.Step;
            if (step >= 0 && step < _steps.Length)
            {
                SkipStepFunction next = (r) => {
                    // Skip to next step
                    instance.Step++;
                    return RunStep(dc, r);
                };

                // Execute step
                await _steps[step](dc, result, next);
            }
            else
            {
                // End of waterfall so just return to parent
                await dc.End(result);
            }
        }
    }
}
