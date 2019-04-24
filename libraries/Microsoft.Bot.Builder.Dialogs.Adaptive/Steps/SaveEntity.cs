// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    public class SaveEntity : DialogCommand
    {
        [JsonConstructor]
        public SaveEntity([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0) : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public SaveEntity(string entity, string property, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            if (!string.IsNullOrEmpty(entity))
            {
                this.Entity = entity;
            }

            if (!string.IsNullOrEmpty(property))
            {
                this.Property = property;
            }
        }

        public string Entity { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (dc.State.TryGetValue<object>($"turn.entities.{Entity}", out var values))
            {
                if (values.GetType() == typeof(JArray))
                {
                    dc.State.SetValue(Property, ((JArray)values)[0]);
                }
                else
                {
                    dc.State.SetValue(Property, values);
                }
            }
            return await dc.EndDialogAsync();
        }
    }
}
