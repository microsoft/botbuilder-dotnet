using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class SlotBuilder : ISlotBuilder
    {
        private readonly IActivityInspector _activityInspector;
        private readonly IEntityInspector _entityInspector;

        public SlotBuilder()
        {
            _activityInspector = new ActivityInspector();
            _entityInspector = new EntityInspector();
        }

        public async Task<IList<Slot>> BuildSlotsAsync(Activity activity, IDictionary<string, object> entities)
        {
            var activitySlots = await _activityInspector.InspectAsync(activity).ConfigureAwait(false);
            var entitySlots = await _entityInspector.InspectAsync(entities).ConfigureAwait(false);

            var slots = new List<Slot>();


            foreach (var activitySlot in activitySlots)
            {
                var slot = new Slot
                {
                    KeyValue = new KeyValuePair<string, object>("GetStateName", new List<string> { activitySlot })
                };
                slots.Add(slot);
            }

            foreach (var entitySlot in entitySlots)
            {
                var slot = new Slot
                {
                    KeyValue = new KeyValuePair<string, object>(entitySlot.Key, entitySlot.Value)
                };
                slots.Add(slot);
            }

            return slots;
        }
    }
}
