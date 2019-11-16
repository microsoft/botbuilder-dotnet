// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which calls another dialog.
    /// </summary>
    public abstract class BaseInvokeDialog : Dialog, IDialogDependencies
    {
        // Expression for dialogId to call (allowing dynamic expression)
        private string dialogIdToCall;

        public BaseInvokeDialog(string dialogIdToCall = null, IDictionary<string, string> bindingOptions = null)
            : base()
        {
            this.dialogIdToCall = dialogIdToCall;

            if (bindingOptions != null)
            {
                this.Options = bindingOptions;
            }
        }

        /// <summary>
        /// Gets or sets configurable options for the dialog. 
        /// </summary>
        /// <value>
        /// Configurable options for the dialog. 
        /// </value>
        [JsonProperty("options")]
        public object Options { get; set; } = new JObject();

        /// <summary>
        /// Gets or sets the dialog to call.
        /// </summary>
        /// <value>
        /// The dialog to call.
        /// </value>
        [JsonProperty("dialog")]
        public Dialog Dialog { get; set; }

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            if (Dialog != null)
            {
                yield return Dialog;
            }

            yield break;
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{Dialog?.Id ?? this.dialogIdToCall}]";
        }

        protected Dialog ResolveDialog(DialogContext dc)
        {
            if (this.Dialog != null)
            {
                return this.Dialog;
            }

            var dialogId = this.dialogIdToCall ?? throw new Exception($"{this.GetType().Name} requires a dialog to be called.");
            return dc.FindDialog(dialogId) ?? throw new Exception($"{dialogId} not found.");
        }

        protected object BindOptions(DialogContext dc, object options)
        {
            // binding options are static definition of options with overlay of passed in options);
            var bindingOptions = (JObject)ObjectPath.Merge(Options, options ?? new JObject());
            var boundOptions = new JObject();

            foreach (var binding in bindingOptions)
            {
                // evalute the value
                var (result, error) = new ExpressionEngine().Parse(binding.Value.ToString()).TryEvaluate(dc.GetState());

                if (error != null)
                {
                    throw new Exception(error);
                }

                // and store in options as the result
                boundOptions[binding.Key] = JToken.FromObject(result);
            }

            return boundOptions;
        }
    }
}
