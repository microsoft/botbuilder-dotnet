// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive;

namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    public class EventQueues
    {
        /// <summary>
        /// Gets unknown entities.
        /// </summary>
        public List<EntityInfo> UnknownEntity { get; } = new List<EntityInfo>();

        /// <summary>
        /// Gets mappings where a property is ready to be set to a specific entity.
        /// </summary>
        public List<EntityToProperty> SetProperty { get; } = new List<EntityToProperty>();

        /// <summary>
        /// Gets mappings where the entity is ambiguous.
        /// </summary>
        public List<EntityToProperty> ClarifyEntity { get; } = new List<EntityToProperty>();

        /// <summary>
        /// Gets entity that can be consumed by more than one slot.
        /// </summary>
        public List<List<EntityToProperty>> ChooseProperty { get; } = new List<List<EntityToProperty>>();

        // Slots to clear
        public List<string> ClearProperty { get; } = new List<string>();

        public static EventQueues Read(SequenceContext context)
        {
            if (!context.State.TryGetValue<EventQueues>("this.events", out var queues))
            {
                queues = new EventQueues();
            }

            return queues;
        }

        public void Write(SequenceContext context)
            => context.State.Add("this.events", this);

        public void Merge(EventQueues queues)
        {
            UnknownEntity.AddRange(queues.UnknownEntity);
            SetProperty.AddRange(queues.SetProperty);
            ClarifyEntity.AddRange(queues.ClarifyEntity);
            ChooseProperty.AddRange(queues.ChooseProperty);
            ClearProperty.AddRange(queues.ClearProperty);
        }

        public bool DequeueEvent(string eventName)
        {
            var changed = true;
            switch (eventName)
            {
                case FormEvents.ChooseProperty: ChooseProperty.Dequeue(); break;
                case FormEvents.ClarifyEntity: ClarifyEntity.Dequeue(); break;
                case FormEvents.ClearProperty: ClearProperty.Dequeue(); break;
                case FormEvents.SetProperty: SetProperty.Dequeue(); break;
                case FormEvents.UnknownEntity: UnknownEntity.Dequeue(); break;
                case FormEvents.Ask:
                default:
                    changed = false;
                    break;
            }

            return changed;
        }
    }
}
