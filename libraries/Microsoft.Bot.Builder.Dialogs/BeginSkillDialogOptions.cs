// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A class with dialog arguments for a <see cref="SkillDialog"/>.
    /// </summary>
    public class BeginSkillDialogOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="Activity"/> to send to the skill.
        /// </summary>
        /// <value>
        /// The <see cref="Activity"/> to send to the skill.
        /// </value>
        public Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DeliveryModes"/> to use for sending
        /// the activity to the skill. If Activity.Type='invoke', this value
        /// will be overridden and default to <see cref="DeliveryModes.ExpectReplies"/>.
        /// If Activity.DeliveryMode is present, it will also take precedence over this.  
        /// </summary>
        /// <value>
        /// The <see cref="DeliveryModes"/> to use for sending the activity
        /// to the skill. This value will be ignored if Activity.DeliveryMode
        /// has a value or Activity.Type='invoke'.
        /// </value>
        public string DeliveryMode { get; set; }
    }
}
