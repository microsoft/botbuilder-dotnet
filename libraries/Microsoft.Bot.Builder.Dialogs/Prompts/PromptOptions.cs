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
        /// Gets or sets the initial prompt to send the user as <seealso cref="string"/>Activity.
        /// </summary>
        /// <value>
        /// The initial prompt to send the user as <seealso cref="string"/>Activity.
        /// </value>
        public string PromptString
        {
            get { return GetProperty<string>(nameof(PromptString)); }
            set { this[nameof(PromptString)] = value; }
        }

        /// <summary>
        /// Gets or sets the initial prompt to send the user as <seealso cref="Activity"/>Activity.
        /// </summary>
        /// <value>
        /// The initial prompt to send the user as <seealso cref="Activity"/>Activity.
        /// </value>
        public Activity PromptActivity
        {
            get { return GetProperty<Activity>(nameof(PromptActivity)); }
            set { this[nameof(PromptActivity)] = value; }
        }

        /// <summary>
        /// Gets or sets the initial SSML to send the user. This is optional.
        /// </summary>
        /// <value>
        /// The initial SSML to send the user.
        /// </value>
        public string Speak
        {
            get { return GetProperty<string>(nameof(Speak)); }
            set { this[nameof(Speak)] = value; }
        }

        /// <summary>
        /// Gets or sets the retry prompt to send the user as <seealso cref="string"/>Activity.
        /// </summary>
        /// <value>
        /// The retry prompt to send the user as <seealso cref="string"/>Activity.
        /// </value>
        public string RetryPromptString
        {
            get { return GetProperty<string>(nameof(RetryPromptString)); }
            set { this[nameof(RetryPromptString)] = value; }
        }

        /// <summary>
        /// Gets or sets the retry prompt to send the user as <seealso cref="Activity"/>Activity.
        /// </summary>
        /// <value>
        /// The retry prompt to send the user as <seealso cref="Activity"/>Activity.
        /// </value>
        public Activity RetryPromptActivity
        {
            get { return GetProperty<Activity>(nameof(RetryPromptActivity)); }
            set { this[nameof(RetryPromptActivity)] = value; }
        }

        /// <summary>
        /// Gets or sets the retry SSML to send the user. This is optional.
        /// </summary>
        /// <value>
        /// The retry SSML to send the user.
        /// </value>
        public string RetrySpeak
        {
            get { return GetProperty<string>(nameof(RetrySpeak)); }
            set { this[nameof(RetrySpeak)] = value; }
        }

        /// <summary>
        /// A helper factory method to create a typed PromptOptions type from a dictionary.
        /// </summary>
        /// <param name="source">The dictionary containing the prompt options.</param>
        /// <returns>The options for the prompt.</returns>
        public static PromptOptions Create(IDictionary<string, object> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source is PromptOptions promptOptions)
            {
                return promptOptions;
            }

            promptOptions = new PromptOptions();
            Assign(promptOptions, source, nameof(PromptString), typeof(string));
            Assign(promptOptions, source, nameof(PromptActivity), typeof(Activity));
            Assign(promptOptions, source, nameof(Speak), typeof(string));
            Assign(promptOptions, source, nameof(RetryPromptString), typeof(string));
            Assign(promptOptions, source, nameof(RetryPromptActivity), typeof(Activity));
            Assign(promptOptions, source, nameof(RetrySpeak), typeof(string));
            return promptOptions;
        }

        protected T GetProperty<T>(string propertyName)
        {
            if (ContainsKey(propertyName))
            {
                return (T)this[propertyName];
            }

            return default(T);
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
    }
}
