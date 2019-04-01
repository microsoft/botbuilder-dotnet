// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Rules
{
    /// <summary>
    /// Rule triggered when a dialog event matching a list of event names is emitted.
    /// </summary>
    public class EventRule : Rule
    {
        /// <summary>
        /// List of events to filter
        /// </summary>
        public List<string> Events { get; set; }

        public EventRule(List<string> events = null, List<IDialog> steps = null, string constraint = null)
            : base(constraint: constraint, steps: steps)
        {
            this.Events = events ?? new List<string>();
            this.Steps = steps ?? new List<IDialog>();
        }

        protected override void GatherConstraints(List<string> constraints)
        {
            base.GatherConstraints(constraints);

            // add in the constraints for Events property
            StringBuilder sb = new StringBuilder();
            string append = string.Empty;
            foreach (var evt in Events)
            {
                sb.Append($"{append} (turn.DialogEvent.Name == '{evt}') ");
                append = "||";
            }
            constraints.Add(sb.ToString());
        }

    }
}
