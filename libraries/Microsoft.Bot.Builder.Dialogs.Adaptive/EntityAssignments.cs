// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Tracks entity related events to surface.
    /// </summary>
    /// <remarks>
    /// When processing entities possible ambiguity are identified and when resolved they turn into assign events.
    /// This tracking persists across multiple input utterances.
    /// </remarks>
    public class EntityAssignments
    {
        private const string Events = "this.events";

        /// <summary>
        /// Gets or sets the queue of pending entity assignments.
        /// </summary>
        /// <value>Queue of entity assignments.</value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<EntityAssignment> Assignments { get; set; } = new List<EntityAssignment>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Read entity event queue from memory.
        /// </summary>
        /// <param name="actionContext">Context for memory.</param>
        /// <returns>Entity event queue.</returns>
        public static EntityAssignments Read(ActionContext actionContext)
        {
            if (!actionContext.State.TryGetValue<EntityAssignments>(Events, out var queues))
            {
                queues = new EntityAssignments();
            }

            return queues;
        }

        /// <summary>
        /// Write state into memory.
        /// </summary>
        /// <param name="actionContext">Memory context.</param>
        public void Write(ActionContext actionContext)
            => actionContext.State.SetValue(Events, this);

        /// <summary>
        /// Return the next enetity event to surface.
        /// </summary>
        /// <returns>Next event to surface.</returns>
        public EntityAssignment NextAssignment() => Assignments.Any() ? Assignments[0] : null;

        /// <summary>
        /// Remove the current event and update the memory.
        /// </summary>
        /// <param name="actionContext">Context to use.</param>
        /// <returns>Removed event.</returns>
        public EntityAssignment Dequeue(ActionContext actionContext)
        {
            var assignment = Assignments.Dequeue();
            Write(actionContext);
            return assignment;
        }
    }
}
