// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptOptions : Dictionary<string, object>
    {
        /// <summary>
        /// (Optional) Initial prompt to send the user. As string.
        /// </summary>
        public string PromptString
        {
            get { return GetProperty<string>(nameof(PromptString)); }
            set { this[nameof(PromptString)] = value; }
        }

        /// <summary>
        /// (Optional) Initial prompt to send the user. As Activity.
        /// </summary>
        public Activity PromptActivity
        {
            get { return GetProperty<Activity>(nameof(PromptActivity)); }
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
        /// (Optional) Retry prompt to send the user. As Activity.
        /// </summary>
        public Activity RetryPromptActivity
        {
            get { return GetProperty<Activity>(nameof(RetryPromptActivity)); }
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
