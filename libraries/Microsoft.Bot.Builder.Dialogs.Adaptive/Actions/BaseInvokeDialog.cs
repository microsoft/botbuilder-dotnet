﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using AdaptiveExpressions.Properties;
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
        public BaseInvokeDialog(string dialogIdToCall = null, object bindingOptions = null)
            : base()
        {
            if (dialogIdToCall != null)
            {
                this.Dialog = dialogIdToCall;
            }

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
        public ObjectExpression<object> Options { get; set; } = new ObjectExpression<object>();

        /// <summary>
        /// Gets or sets the dialog to call.
        /// </summary>
        /// <value>
        /// The dialog to call.
        /// </value>
        [JsonProperty("dialog")]
        public DialogExpression Dialog { get; set; } 

        /// <summary>
        /// Gets or sets a value indicating whether to have the new dialog should process the activity.
        /// </summary>
        /// <value>The default for this will be true, which means the new dialog should not look the activity.  You can set this to false to dispatch the activity to the new dialog.</value>
        [DefaultValue(true)]
        [JsonProperty("activityProcessed")]
        public BoolExpression ActivityProcessed { get; set; } = true;

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            if (Dialog?.Value != null)
            {
                yield return Dialog.Value;
            }

            yield break;
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{Dialog?.ToString()}]";
        }

        /// <summary>
        /// Resolve Dialog Expression as either Dialog, or StringExpression to get dialogid.
        /// </summary>
        /// <param name="dc">dialogcontext.</param>
        /// <returns>dialog.</returns>
        protected virtual Dialog ResolveDialog(DialogContext dc)
        {
            if (this.Dialog?.Value != null)
            {
                return this.Dialog.Value;
            }

            // NOTE: we want the result of the expression as a string so we can look up the string using external FindDialog().
            var se = new StringExpression($"={this.Dialog.ExpressionText}");
            var dialogId = se.GetValue(dc.State);
            return dc.FindDialog(dialogId ?? throw new Exception($"{this.Dialog.ToString()} not found."));
        }

        /// <summary>
        /// BindOptions - evaluate expressions in options.
        /// </summary>
        /// <param name="dc">dialog context.</param>
        /// <param name="options">options to bind.</param>
        /// <returns>merged options with expressions bound to values.</returns>
        protected virtual object BindOptions(DialogContext dc, object options)
        {
            // binding options are static definition of options with overlay of passed in options);
            var bindingOptions = (JObject)ObjectPath.Merge(this.Options.GetValue(dc.State), options ?? new JObject());
            var boundOptions = new JObject();

            foreach (var binding in bindingOptions)
            {
                // evaluate the value
                var value = new ValueExpression(binding.Value).EvaluateExpression(dc.State);

                // and store in options as the result
                ObjectPath.SetPathValue(boundOptions, binding.Key, value);
            }

            return boundOptions;
        }
    }
}
