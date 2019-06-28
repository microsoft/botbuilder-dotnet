// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.BotKit.Adapters.Slack
{
    public class SlackDialog
    {
        private DialogData data;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackDialog"/> class.
        /// Create a Slack Dialog object for use with [replyWithDialog()](#replyWithDialog).
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="callbackId">Callback id of dialog.</param>
        /// <param name="submitLabel">Label for the submit button.</param>
        /// <param name="elements">An array of dialog elements.</param>
        public SlackDialog(string title, string callbackId, string submitLabel, List<DialogElement> elements)
        {
            this.data = new DialogData(title, callbackId, submitLabel, elements);
        }

        /// <summary>
        /// Set the State property of the dialog.
        /// </summary>
        /// <param name="value">Value for state.</param>
        public void SetState(string value)
        {
            this.data.State = value;
        }

        /// <summary>
        /// Set the NotifyOnCancel property of the dialog.
        /// </summary>
        /// <param name="set">Set true to have Slack notify you with a `dialog_cancellation` event if a user cancels the dialog without submitting.</param>
        public void SetNotifyOnCancel(bool set)
        {
            this.data.NotifyOnCancel = set;
        }

        /// <summary>
        /// Set the title of the dialog.
        /// </summary>
        /// <param name="value">Value for title.</param>
        public void SetTitle(string value)
        {
            this.data.Title = value;
        }

        /// <summary>
        /// Set the dialog's callback_id.
        /// </summary>
        /// <param name="value">Value for the callback_id.</param>
        public void SetCallbackId(string value)
        {
            this.data.CallbackId = value;
        }

        /// <summary>
        /// Set the button text for the submit button on the dialog.
        /// </summary>
        /// <param name="value">Value for the button label.</param>
        public void SetSubmitLabel(string value)
        {
            this.data.SubmitLabel = value;
        }

        /// <summary>
        /// Add a text element to the dialog.
        /// </summary>
        /// <param name="label">Label of the element.</param>
        /// <param name="name">Name of the element.</param>
        /// <param name="value">Value of the element.</param>
        /// <param name="options">.</param>
        /// <param name="subtype">Subtype of the element.</param>
        public void AddText(DialogElement label, string name, string value, object options, string subtype = default(string))
        {
            DialogElement element;

            element = label;

            if (options.GetType() == typeof(DialogElement))
            {
                element = (DialogElement)options;
            }

            this.data.Elements.Add(element);
        }

        /// <summary>
        /// Add a text element to the dialog.
        /// </summary>
        /// <param name="label">Label of the element.</param>
        /// <param name="name">Name of the element.</param>
        /// <param name="value">Value of the element.</param>
        /// <param name="options">.</param>
        /// <param name="subtype">Subtype of the element.</param>
        public void AddText(string label, string name, string value, object options, string subtype = default(string))
        {
            DialogElement element;

            element = new DialogElement
            {
                Label = label,
                Name = name,
                Value = value,
                Type = "text",
                Subtype = subtype,
            };

            if (options.GetType() == typeof(DialogElement))
            {
                element = (DialogElement)options;
            }

            this.data.Elements.Add(element);
        }

        /// <summary>
        /// Add an email input to the dialog.
        /// </summary>
        /// <param name="label">Label of the input.</param>
        /// <param name="name">Name of the input.</param>
        /// <param name="value">Value of the input.</param>
        /// <param name="options">.</param>
        public void AddEmail(string label, string name, string value, object options = default(object))
        {
            this.AddText(label, name, value, options, "email");
        }

        /// <summary>
        /// Add a number input to the dialog.
        /// </summary>
        /// <param name="label">Label of the input.</param>
        /// <param name="name">Name of the input.</param>
        /// <param name="value">Value of the input.</param>
        /// <param name="options">.</param>
        public void AddNumber(string label, string name, string value, object options = default(object))
        {
            this.AddText(label, name, value, options, "number");
        }

        /// <summary>
        /// Add a telephone number input to the dialog.
        /// </summary>
        /// <param name="label">Label of the input.</param>
        /// <param name="name">Name of the input.</param>
        /// <param name="value">Value of the input.</param>
        /// <param name="options">.</param>
        public void AddTel(string label, string name, string value, object options = default(object))
        {
            this.AddText(label, name, value, options, "tel");
        }

        /// <summary>
        /// Add a URL input to the dialog.
        /// </summary>
        /// <param name="label">Label of the input.</param>
        /// <param name="name">Name of the input.</param>
        /// <param name="value">Value of the input.</param>
        /// <param name="options">.</param>
        public void AddUrl(string label, string name, string value, object options = default(object))
        {
            this.AddText(label, name, value, options, "url");
        }

        /// <summary>
        /// Add a text area input to the dialog.
        /// </summary>
        /// <param name="label">Label of the input.</param>
        /// <param name="name">Name of the input.</param>
        /// <param name="value">Value of the input.</param>
        /// <param name="options">.</param>
        /// <param name="subtype">Subtype of the input.</param>
        public void AddTextArea(DialogElement label, string name, string value, object options, string subtype)
        {
            DialogElement element;

            element = label;

            if (options.GetType() == typeof(DialogElement))
            {
                element = (DialogElement)options;
            }

            this.data.Elements.Add(element);
        }

        /// <summary>
        /// Add a text area input to the dialog.
        /// </summary>
        /// <param name="label">Label of the input.</param>
        /// <param name="name">Name of the input.</param>
        /// <param name="value">Value of the input.</param>
        /// <param name="options">.</param>
        /// <param name="subtype">Subtype of the input.</param>
        public void AddTextArea(string label, string name, string value, object options, string subtype)
        {
            DialogElement element;

            element = new DialogElement
            {
                Label = label,
                Name = name,
                Value = value,
                Type = "textArea",
                Subtype = subtype,
            };

            if (options.GetType() == typeof(DialogElement))
            {
                element = (DialogElement)options;
            }

            this.data.Elements.Add(element);
        }

        /// <summary>
        /// Add a dropdown select input to the dialog.
        /// </summary>
        /// <param name="label">Label of the input.</param>
        /// <param name="name">Name of the input.</param>
        /// <param name="value">Value of the input.</param>
        /// <param name="optionList">List of options of the input.</param>
        /// <param name="options">The options.</param>
        public void AddSelect(string label, string name, string value, Dictionary<string, string> optionList, object options)
        {
            DialogElement element;

            element = new DialogElement
            {
                Label = label,
                Name = name,
                Value = value,
                Type = "select",
                OptionList = optionList,
            };

            if (options.GetType() == typeof(DialogElement))
            {
                element = (DialogElement)options;
            }

            this.data.Elements.Add(element);
        }

        /// <summary>
        /// Get the dialog object as a JSON encoded string.
        /// </summary>
        /// <returns>The JSON encoded string.</returns>
        public string AsString()
        {
            return JsonConvert.ToString(this.data.ToString());
        }

        /// <summary>
        /// Get the dialog object for use with bot.replyWithDialog().
        /// </summary>
        /// <returns>The data.</returns>
        public DialogData AsObject()
        {
            return this.data;
        }
    }
}
