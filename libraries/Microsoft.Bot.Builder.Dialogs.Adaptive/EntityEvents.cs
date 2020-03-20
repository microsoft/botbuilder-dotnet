// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EntityEvents
    {
        private const string Events = "this.events";

        /// <summary>
        /// Gets mappings where a property is ready to be set to a specific entity.
        /// </summary>
        /// <value>List of entities to use when changeing properties.</value>
        [JsonProperty("assignEntities")]
        public List<EntityAssignment> AssignEntities { get; } = new List<EntityAssignment>();

        /// <summary>
        /// Gets mappings where the entity is ambiguous.
        /// </summary>
        /// <value>List of ambiguous entities and the property they should be assigned to.</value>
        [JsonProperty("chooseEntities")]
        public List<EntityAssignment> ChooseEntities { get; } = new List<EntityAssignment>();

        /// <summary>
        /// Gets entity that can be consumed by more than one property.
        /// </summary>
        /// <value>List of choices between entity and property assignments.</value>
        [JsonProperty("chooseProperties")]
        public List<List<EntityAssignment>> ChooseProperties { get; } = new List<List<EntityAssignment>>();

        /// <summary>
        /// Gets list of properties to clear.
        /// </summary>
        /// <value>List of properties to clear.</value>
        [JsonProperty("clearProperties")]
        public List<string> ClearProperties { get; } = new List<string>();

        [JsonProperty("hasEvents")]
        public bool HasEvents => AssignEntities.Any() || ChooseEntities.Any() || ChooseProperties.Any() || ClearProperties.Any();

        /// <summary>
        /// Read event queues from memory.
        /// </summary>
        /// <param name="actionContext">Context for memory.</param>
        /// <returns>Event queues.</returns>
        public static EntityEvents Read(ActionContext actionContext)
        {
            if (!actionContext.State.TryGetValue<EntityEvents>(Events, out var queues))
            {
                queues = new EntityEvents();
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
                case AdaptiveEvents.EndOfActions: changed = false; break;
                default:
                    changed = false;
                    break;
            }

            return changed;
        }
    }
}
