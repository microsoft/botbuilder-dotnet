// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
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
        /// configurable options for the dialog
        /// </summary>
        public object Options { get; set; }

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

        public BaseInvokeDialog(string dialogIdToCall = null, string property = null, object options = null) 
            : base()
        {
            this.dialogIdToCall = dialogIdToCall;

            if (options != null)
            {
                this.Options = options;
            }

            if (!string.IsNullOrEmpty(property))
            {
                Property = property;
            }
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

        public override List<IDialog> ListDependencies()
        {
            if (Dialog != null)
            {
                return new List<IDialog>() { Dialog };
            }

            return new List<IDialog>();
        }

        protected void BindOptions(DialogContext dc)
        {
            if (Options == null)
            {
                return;
            }

            if (Options is JObject jObj)
            {
                foreach (var value in jObj.Values())
                {
                    if (value.Type == JTokenType.String)
                    {
                        var (result, error) = new ExpressionEngine().Parse(value.Value<string>()).TryEvaluate(dc.State);

                        if (error != null)
                        {
                            throw new Exception(error);
                        }

                        value.Replace(JToken.FromObject(result));
                    }
                }
            }
        }
    }
}
