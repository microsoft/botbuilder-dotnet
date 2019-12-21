// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Tracks entity related events to surface.
    /// </summary>
    /// <remarks>
    /// When processing entities possible ambiguity are identified and when resolved they turn into assign events.
    /// This tracking persists across multiple input utterances.
    /// </remarks>
    public class EntityEvents
    {
        private const string Events = "this.events";

        /// <summary>
        /// Gets mappings where a property is ready to be set to a specific entity.
        /// </summary>
        /// <value>List of entities to use when changeing properties.</value>
        public List<EntityAssignment> AssignEntities { get; } = new List<EntityAssignment>();

        /// <summary>
        /// Gets mappings where the entity is ambiguous.
        /// </summary>
        /// <value>List of ambiguous entities and the property they should be assigned to.</value>
        public List<EntityAssignment> ChooseEntities { get; } = new List<EntityAssignment>();

        /// <summary>
        /// Gets entity that can be consumed by more than one property.
        /// </summary>
        /// <value>List of choices between entity and property assignments.</value>
        public List<List<EntityAssignment>> ChooseProperties { get; } = new List<List<EntityAssignment>>();

        /// <summary>
        /// Gets list of properties to clear.
        /// </summary>
        /// <value>List of properties to clear.</value>
        public List<string> ClearProperties { get; } = new List<string>();

        /// <summary>
        /// Read event queues from memory.
        /// </summary>
        /// <param name="context">Context for memory.</param>
        /// <returns>Event queues.</returns>
        public static EntityEvents Read(SequenceContext context)
        {
            if (!context.GetState().TryGetValue<EntityEvents>(Events, out var queues))
            {
                queues = new EntityEvents();
            }

            return queues;
        }

        /// <summary>
        /// Write state into memory.
        /// </summary>
        /// <param name="context">Memory context.</param>
        public void Write(SequenceContext context)
            => context.GetState().SetValue(Events, this);

        /// <summary>
        /// Merge another event queue.
        /// </summary>
        /// <param name="queues">Queues to merge.</param>
        public void Merge(EntityEvents queues)
        {
            AssignEntities.AddRange(queues.AssignEntities);
            ChooseEntities.AddRange(queues.ChooseEntities);
            ChooseProperties.AddRange(queues.ChooseProperties);
            ClearProperties.AddRange(queues.ClearProperties);
        }

        /// <summary>
        /// Remove an event result from queues.
        /// </summary>
        /// <param name="eventName">Event to remove.</param>
        /// <returns>True if event was found.</returns>
        public bool DequeueEvent(string eventName)
        {
            var changed = true;
            switch (eventName)
            {
                case AdaptiveEvents.ChooseProperty: ChooseProperties.Dequeue(); break;
                case AdaptiveEvents.ChooseEntity: ChooseEntities.Dequeue(); break;
                case AdaptiveEvents.ClearProperty: ClearProperties.Dequeue(); break;
                case AdaptiveEvents.AssignEntity: AssignEntities.Dequeue(); break;
                case AdaptiveEvents.Ask:
                default:
                    changed = false;
                    break;
            }

            return changed;
        }
    }
}
