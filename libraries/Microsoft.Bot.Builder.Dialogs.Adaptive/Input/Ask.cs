// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Ask for an open-ended response.
    /// </summary>
    /// <remarks>
    /// This sends an activity and then terminates the turn with <see cref="DialogTurnStatus.CompleteAndWait"/>.
    /// The next activity from the user will then be handled by the parent adaptive dialog.
    /// 
    /// It also builds in a model of the properties that are expected in response through <see cref="DialogPath.ExpectedProperties"/>.
    /// <see cref="DialogPath.Retries"/> is updated as the same question is asked multiple times.
    /// </remarks>
    public class Ask : SendActivity
    {
        [JsonProperty("$kind")]
        public new const string DeclarativeType = "Microsoft.Ask";

        [JsonConstructor]
        public Ask(
            string text = null,
            ArrayExpression<string> expectedProperties = null,
            [CallerFilePath] string callerPath = "",
            [CallerLineNumber] int callerLine = 0)
        : base(text, callerPath, callerLine)
        {
            this.Activity = new ActivityTemplate(text ?? string.Empty);
            this.ExpectedProperties = expectedProperties;
        }

        /// <summary>
        /// Gets or sets properties expected to be filled by response.
        /// </summary>
        /// <value>
        /// Properties expected to be filled by response.
        /// </value>
        [JsonProperty("expectedProperties")]
        public ArrayExpression<string> ExpectedProperties { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            //get number of retries from memory
            if (!dc.State.TryGetValue(DialogPath.Retries, out int retries))
            {
                retries = 0;
            }

            dc.State.TryGetValue(TurnPath.DialogEvent, out DialogEvent trigger);

            var expected = this.ExpectedProperties?.GetValue(dc.State);
            if (expected != null
                && dc.State.TryGetValue(DialogPath.ExpectedProperties, out List<string> lastExpectedProperties)
                && !expected.Any(prop => !lastExpectedProperties.Contains(prop))
                && !lastExpectedProperties.Any(prop => !expected.Contains(prop))
                && dc.State.TryGetValue(DialogPath.LastTriggerEvent, out DialogEvent lastTrigger)
                && lastTrigger.Name.Equals(trigger.Name))
            {
                retries++;
            }
            else
            {
                retries = 0;
            }

            dc.State.SetValue(DialogPath.Retries, retries);
            dc.State.SetValue(DialogPath.LastTriggerEvent, trigger);
            dc.State.SetValue(DialogPath.ExpectedProperties, expected);
            var result = await base.BeginDialogAsync(dc, options, cancellationToken).ConfigureAwait(false);
            result.Status = DialogTurnStatus.CompleteAndWait;
            return result;
        }
    }
}
