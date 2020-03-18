// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Sets a property with the result of evaluating a value expression.
    /// </summary>
    public class SetProperties : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.SetProperties";

        [JsonConstructor]
        public SetProperties([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; } 

        /// <summary>
        /// Gets or sets additional property assignments.
        /// </summary>
        /// <value>
        /// Additional property settings as property=value pairs.
        /// </value>
        [JsonProperty("assignments")]
        public List<PropertyAssignment> Assignments { get; set; } = new List<PropertyAssignment>();

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            foreach (var propValue in this.Assignments)
            {
                var (value, valueError) = propValue.Value.TryGetValue(dc.State);
                if (valueError != null)
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {propValue.Value.ToString()}. Error: {valueError}");
                }

                dc.State.SetValue(propValue.Property.GetValue(dc.State), value);
            }

            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{string.Join(",", this.Assignments)}]";
        }
    }
}
