// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptOptions : Dictionary<string, object>
    {
        /// <summary>
        /// A helper factory method to create a typed PromptOptions type from a dictionary
        /// </summary>
        public static PromptOptions Create(IDictionary<string, object> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var promptOptions = source as PromptOptions;
            if (promptOptions != null)
            {
                return promptOptions;
            }
            promptOptions = new PromptOptions();
            Assign(promptOptions, source, nameof(PromptString), typeof(string));
            Assign(promptOptions, source, nameof(PromptActivity), typeof(MessageActivity));
            Assign(promptOptions, source, nameof(Speak), typeof(string));
            Assign(promptOptions, source, nameof(RetryPromptString), typeof(string));
            Assign(promptOptions, source, nameof(RetryPromptActivity), typeof(Activity));
            Assign(promptOptions, source, nameof(RetrySpeak), typeof(string));
            return promptOptions;
        }

        private static void Assign(PromptOptions promptOptions, IDictionary<string, object> source, string name, Type type)
        {
            if (source.ContainsKey(name))
            {
                var property = source[name];
                if (property.GetType() != type)
                {
                    throw new ArgumentException($"{name} must be type {type}");
                }
                promptOptions[name] = property;
            }
        }

        /// <summary>
        /// (Optional) Initial prompt to send the user. As string.
        /// </summary>
        public string PromptString
        {
            get { return GetProperty<string>(nameof(PromptString)); }
            set { this[nameof(PromptString)] = value; }
        }

        /// <summary>
        /// (Optional) Initial prompt to send the user. As MessageActivity.
        /// </summary>
        public MessageActivity PromptActivity
        {
            get { return GetProperty<MessageActivity>(nameof(PromptActivity)); }
            set { this[nameof(PromptActivity)] = value; }
        }

        /// <summary>
        /// (Optional) Initial SSML to send the user.
        /// </summary>
        public string Speak
        {
            get { return GetProperty<string>(nameof(Speak)); }
            set { this[nameof(Speak)] = value; }
        }

        /// <summary>
        /// (Optional) Retry prompt to send the user. As String.
        /// </summary>
        public string RetryPromptString
        {
            get { return GetProperty<string>(nameof(RetryPromptString)); }
            set { this[nameof(RetryPromptString)] = value; }
        }

        /// <summary>
        /// (Optional) Retry prompt to send the user. As MessageActivity.
        /// </summary>
        public MessageActivity RetryPromptActivity
        {
            get { return GetProperty<MessageActivity>(nameof(RetryPromptActivity)); }
            set { this[nameof(RetryPromptActivity)] = value; }
        }

        /// <summary>
        /// (Optional) Retry SSML to send the user.
        /// </summary>
        public string RetrySpeak
        {
            get { return GetProperty<string>(nameof(RetrySpeak)); }
            set { this[nameof(RetrySpeak)] = value; }
        }

        protected T GetProperty<T>(string propertyName)
        {
            if (ContainsKey(propertyName))
            {
                return (T)this[propertyName];
            }
            return default(T);
        }
    }
}
