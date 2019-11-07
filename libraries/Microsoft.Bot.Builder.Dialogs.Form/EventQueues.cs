// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive;

namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    public class EventQueues
    {
        private const string Events = "this.events";

        /// <summary>
        /// Gets mappings where a property is ready to be set to a specific entity.
        /// </summary>
        /// <value>List of entities to use when changeing properties.</value>
        public List<EntityToProperty> SetProperty { get; } = new List<EntityToProperty>();

        /// <summary>
        /// Gets mappings where the entity is ambiguous.
        /// </summary>
        /// <value>List of ambiguous entities and the property they should be assigned to.</value>
        public List<EntityToProperty> ClarifyEntity { get; } = new List<EntityToProperty>();

        /// <summary>
        /// Gets entity that can be consumed by more than one slot.
        /// </summary>
        /// <value>List of choices between entity and property assignments.</value>
        public List<List<EntityToProperty>> ChooseProperty { get; } = new List<List<EntityToProperty>>();

        /// <summary>
        /// Gets slist of properties to clear.
        /// </summary>
        /// <value>List of peropties to clear.</value>
        public List<string> ClearProperty { get; } = new List<string>();

        /// <summary>
        /// Read event queues from memory.
        /// </summary>
        /// <param name="context">Context for memory.</param>
        /// <returns>Event queues.</returns>
        public static EventQueues Read(SequenceContext context)
        {
            if (!context.GetState().TryGetValue<EventQueues>(Events, out var queues))
            {
                queues = new EventQueues();
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
        public void Merge(EventQueues queues)
        {
            SetProperty.AddRange(queues.SetProperty);
            ClarifyEntity.AddRange(queues.ClarifyEntity);
            ChooseProperty.AddRange(queues.ChooseProperty);
            ClearProperty.AddRange(queues.ClearProperty);
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
                case FormEvents.ChooseProperty: ChooseProperty.Dequeue(); break;
                case FormEvents.ClarifyEntity: ClarifyEntity.Dequeue(); break;
                case FormEvents.ClearProperty: ClearProperty.Dequeue(); break;
                case FormEvents.SetProperty: SetProperty.Dequeue(); break;
                case FormEvents.Ask:
                default:
                    changed = false;
                    break;
            }

            return changed;
        }
    }
}
