// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class Ask : SendActivity
    {
        [JsonConstructor]
        public Ask(
            string text = null,
            List<string> expectedProperties = null,
            [CallerFilePath] string callerPath = "",
            [CallerLineNumber] int callerLine = 0)
        : base(text, callerPath, callerLine)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new ActivityTemplate(text ?? string.Empty);
            this.ExpectedProperties = expectedProperties;
        }

        /// <summary>
        /// Gets or sets slots expected to be filled by response.
        /// </summary>
        /// <value>
        /// Slots expected to be filled by response.
        /// </value>
        public List<string> ExpectedProperties { get; set; } = new List<string>();

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            //get number of retries from memory
            if (!dc.GetState().TryGetValue(DialogPath.Retries, out int retries))
            {
                retries = 0;
            }

            dc.GetState().TryGetValue(TurnPath.DIALOGEVENT, out DialogEvent trigger);
            
            if (dc.GetState().TryGetValue(DialogPath.ExpectedProperties, out List<string> lastExpectedProperties)
                && ExpectedProperties.Where(prop => !lastExpectedProperties.Contains(prop)).ToList().Count == 0
                && lastExpectedProperties.Where(prop => !ExpectedProperties.Contains(prop)).ToList().Count == 0)
            {
                if (trigger.Name.Equals("ask"))
                {
                    //repeat Ask: expected properties unchanged && event is triggered by Ask
                    retries++;
                }                    
            }
            else
            {
                retries = 0;
            }

            dc.GetState().SetValue(DialogPath.ExpectedProperties, ExpectedProperties);
            dc.GetState().SetValue(DialogPath.Retries, retries);
            var result = await base.BeginDialogAsync(dc, options, cancellationToken).ConfigureAwait(false);
            result.Status = DialogTurnStatus.CompleteAndWait;
            return result;
        }
    }
}
