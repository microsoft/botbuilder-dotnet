// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Bot.Builder.Dialogs.Declarative;
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
        public BaseInvokeDialog(string dialogIdToCall = null, object bindingOptions = null)
            : base()
        {
            if (dialogIdToCall != null)
            {
                this.DialogId = dialogIdToCall;
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
        public object Options { get; set; }

        /// <summary>
        /// Gets or sets the dialog to call.
        /// </summary>
        /// <value>
        /// The dialog to call.
        /// </value>
        [JsonProperty("dialog")]
        public Dialog Dialog { get; set; }

        /// <summary>
        /// Gets or sets the expression whih resolves to the dialog Id to call.
        /// </summary>
        /// <remarks>In the case of calling dialogs which are not declarative you can invoke them by id using dialogId property.  </remarks>
        /// <value>
        /// The dialog.id of a dialog which is in a DialogSet in the parent call chain.  If Dialog is defined this property is ignored.
        /// </value>
        [JsonProperty("dialogId")]
        public StringExpression DialogId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to have the new dialog should process the activity.
        /// </summary>
        /// <value>The default for this will be true, which means the new dialog should not look the activity.  You can set this to false to dispatch the activity to the new dialog.</value>
        [DefaultValue(true)]
        [JsonProperty("activityProcessed")]
        public bool ActivityProcessed { get; set; } = true;

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
            return $"{this.GetType().Name}[{Dialog?.Id ?? DialogId?.ToString()}]";
        }

        protected Dialog ResolveDialog(DialogContext dc)
        {
            if (this.Dialog != null)
            {
                return this.Dialog;
            }

            var dialogId = this.DialogId?.TryGetValue(dc.GetState()).Value;
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
                var (value, error) = new ValueExpression(binding.Value).TryGetValue(dc.GetState());

                if (error != null)
                {
                    throw new Exception(error);
                }

                // and store in options as the result
                ObjectPath.SetPathValue(boundOptions, binding.Key, value);
            }

            return boundOptions;
        }
    }
}
