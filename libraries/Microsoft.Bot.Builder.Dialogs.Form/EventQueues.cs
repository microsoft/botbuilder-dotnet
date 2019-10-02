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
        public List<EntityInfo> Unknown { get; } = new List<EntityInfo>();

        /// <summary>
        /// Gets mappings where a property is ready to be set to a specific entity.
        /// </summary>
        public List<EntityToProperty> SetProperty { get; } = new List<EntityToProperty>();

        /// <summary>
        /// Gets mappings where the entity is ambiguous.
        /// </summary>
        public List<EntityToProperty> ClarifyEntity { get; } = new List<EntityToProperty>();

        /// <summary>
        /// Gets singleton property that has multiple possible entities to bind to.
        /// </summary>
        public List<EntitiesToProperty> ChooseEntity { get; } = new List<EntitiesToProperty>();

        /// <summary>
        /// Gets entity that can be consumed by more than one slot.
        /// </summary>
        public List<EntityToProperties> ChooseProperty { get; } = new List<EntityToProperties>();

        /// <summary>
        /// Gets alternative entity to property mappings.
        /// </summary>
        public List<List<EntityToProperty>> ChooseMapping { get; } = new List<List<EntityToProperty>>();

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
            Unknown.AddRange(queues.Unknown);
            SetProperty.AddRange(queues.SetProperty);
            ClarifyEntity.AddRange(queues.ClarifyEntity);
            ChooseEntity.AddRange(queues.ChooseEntity);
            ChooseProperty.AddRange(queues.ChooseProperty);
            ClearProperty.AddRange(queues.ClearProperty);
        }
    }
}
