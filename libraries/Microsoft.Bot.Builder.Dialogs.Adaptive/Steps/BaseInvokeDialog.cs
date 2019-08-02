// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Step which calls another dialog
    /// </summary>
    public abstract class BaseInvokeDialog : DialogCommand
    {
        protected string dialogIdToCall;

        /// <summary>
        /// gets or sets configurable options for the dialog. Key=>Expression pairs.
        /// </summary>
        public object Options { get; set; } = new JObject();

        /// <summary>
        /// The dialog to call.
        /// </summary>
        public IDialog Dialog { get; set; }

        /// <summary>
        /// The property from memory to pass to the calling dialog and to set the return value to.
        /// </summary>
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

        public BaseInvokeDialog(string dialogIdToCall = null, string property = null, IDictionary<string,string> bindingOptions = null)
            : base()
        {
            this.dialogIdToCall = dialogIdToCall;

            if (bindingOptions != null)
            {
                this.Options = bindingOptions;
            }

            if (!string.IsNullOrEmpty(property))
            {
                Property = property;
            }
        }

        public override List<IDialog> ListDependencies()
        {
            if (Dialog != null)
            {
                return new List<IDialog>() { Dialog };
            }

            return new List<IDialog>();
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{Dialog?.Id ?? this.dialogIdToCall}:{this.BindingPath()}]";
        }

        protected IDialog ResolveDialog(DialogContext dc)
        {
            var dialog = this.Dialog;
            if (dialog == null && !string.IsNullOrEmpty(this.dialogIdToCall))
            {
                dialog = dc.FindDialog(this.dialogIdToCall);
            }

            var dialogId = dialog?.Id ?? throw new Exception($"{this.GetType().Name} requires a dialog to be called.");
            return dialog;
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
