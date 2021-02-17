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
    /// Then by operations in order from schema (usually within AssignEntity).
    /// Then by unexpected before expected.
    /// Then by oldest turn first.
    /// Then by minimum position in utterance.
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

        /// <summary>
        /// Compares <see cref="EntityAssignment"/> x against y to determine its relative priority.
        /// </summary>
        /// <param name="x">Reference Entity.</param>
        /// <param name="y">Comparisson Entity.</param>
        /// <returns>Numerical value representing x's relative priority.</returns>
        public override int Compare(EntityAssignment x, EntityAssignment y)
        {
            // Order by event
            int comparison = Array.IndexOf(eventPreference, x.Event).CompareTo(Array.IndexOf(eventPreference, y.Event));
            if (comparison == 0)
            {
                // Order by operations
                comparison = Array.IndexOf(operationPreference, x.Operation).CompareTo(Array.IndexOf(operationPreference, y.Operation));
                if (comparison == 0)
                {
                    // Unexpected before expected
                    comparison = x.IsExpected.CompareTo(y.IsExpected);
                    if (comparison == 0)
                    {
                        // Order by history
                        comparison = x.Value.WhenRecognized.CompareTo(y.Value.WhenRecognized);
                        if (comparison == 0)
                        {
                            // Order by position in utterance
                            comparison = x.Value.Start.CompareTo(y.Value.Start);
                        }
                    }
                }
            }

            return comparison;
        }
    }
}
