// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.using System;
using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Compare two entity assignments to determine their relative priority.
    /// </summary>
    /// <remarks>
    /// Compare by event: AssignEntity, ChooseProperty, ChooseEntity
    /// Then unexpected before expected
    /// Then by oldest first
    /// Then by order of operations passed in.
    /// </remarks>
    public class EntityAssignmentComparer : Comparer<EntityAssignment>
    {
        private static string[] eventPreference = new string[] { AdaptiveEvents.AssignEntity, AdaptiveEvents.ChooseProperty, AdaptiveEvents.ChooseEntity };

        private string[] operationPreference;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAssignmentComparer"/> class.
        /// </summary>
        /// <param name="operations">Preference on operations.</param>
        public EntityAssignmentComparer(string[] operations)
            : base()
        {
            operationPreference = operations;
        }

        public override int Compare(EntityAssignment x, EntityAssignment y)
        {
            // Order by event
            int comparison = Array.IndexOf(eventPreference, x.Event).CompareTo(Array.IndexOf(eventPreference, y.Event));
            if (comparison == 0)
            {
                // Unexpected before expected
                comparison = x.IsExpected.CompareTo(y.IsExpected);
                if (comparison == 0)
                {
                    // Order by history
                    comparison = x.Entity.WhenRecognized.CompareTo(y.Entity.WhenRecognized);
                    if (comparison == 0)
                    {
                        // Order by operations
                        comparison = Array.IndexOf(operationPreference, x.Operation).CompareTo(Array.IndexOf(operationPreference, y.Operation));
                    }
                }
            }

            return comparison;
        }
    }
}
