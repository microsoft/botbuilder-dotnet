// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which calls another dialog.
    /// </summary>
    public abstract class BaseInvokeDialog : DialogAction
    {
        public BaseInvokeDialog(string dialogIdToCall = null, string property = null, IDictionary<string, string> bindingOptions = null)
            : base()
        {
            this.DialogId = dialogIdToCall;

            if (bindingOptions != null)
            {
                this.Options = bindingOptions;
            }

            if (!string.IsNullOrEmpty(property))
            {
                Property = property;
            }
        }

        /// <summary>
        /// Gets or sets configurable options for the dialog. 
        /// </summary>
        /// <value>
        /// Configurable options for the dialog. 
        /// </value>
        public object Options { get; set; } = new JObject();

        /// <summary>
        /// Gets or sets the dialog ID to call.
        /// </summary>
        /// <value>
        /// The dialog ID to call.
        /// </value>
        public string DialogId { get; set; }

        /// <summary>
        /// Gets or sets the property from memory to pass to the calling dialog and to set the return value to.
        /// </summary>
        /// <value>
        /// The property from memory to pass to the calling dialog and to set the return value to.
        /// </value>
        public string Property
        {
            get
            {
                return InputBindings.TryGetValue(DialogContextState.DIALOG_VALUE, out string value) ? value : null;
            }

            set
            {
                InputBindings[DialogContextState.DIALOG_VALUE] = value;
                OutputBinding = value;
            }
        }

        public override List<IDialog> ListDependencies()
        {
            return new List<IDialog>();
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{DialogId}:{this.BindingPath()}]";
        }

        protected IDialog ResolveDialog(DialogContext dc)
        {
            var (result, error) = new ExpressionEngine().Parse(this.DialogId).TryEvaluate(dc.State);
            if (error != null)
            {
                throw new Exception(error);
            }

            var dialogId = (string)result ?? this.DialogId ?? throw new Exception($"{this.GetType().Name} requires a dialog to be called.");
            return dc.FindDialog(dialogId);
        }

        protected object BindOptions(DialogContext dc, object options)
        {
            // binding options are static definition of options with overlay of passed in options);
            var bindingOptions = (JObject)ObjectPath.Merge(Options, options ?? new JObject());
            var boundOptions = new JObject();

            foreach (var binding in bindingOptions)
            {
                // evalute the value
                var (result, error) = new ExpressionEngine().Parse(binding.Value.ToString()).TryEvaluate(dc.State);

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
