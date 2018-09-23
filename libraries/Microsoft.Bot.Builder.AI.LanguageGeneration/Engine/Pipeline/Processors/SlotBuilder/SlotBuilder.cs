using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// Builder responsible for building <see cref="Slot"/> objects which are the main data container carrying user referenced templates that need to be resolved.
    /// </summary>
    internal class SlotBuilder : ISlotBuilder
    {
        private readonly IActivityInspector _activityInspector;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SlotBuilder()
        {
            _activityInspector = new ActivityInspector();
        }

        /// <summary>
        /// The builder executor function, takes a <see cref="Activity"/> and a <see cref="IDictionary{string, object}"/> and builds a list of <see cref="Slot"/> objects that will be used to carry data to the service.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> containing template references that need to be resolved.</param>
        /// <param name="entities">The list containing entity values that will be used to substitute entity references in template resolution values.</param>
        /// <returns>a <see cref="IList{Slot}"/></returns>
        public IList<Slot> BuildSlots(Activity activity, IDictionary<string, object> entities)
        {
            var activitySlots = _activityInspector.Inspect(activity);
            //var entitySlots = await _entityInspector.InspectAsync(entities).ConfigureAwait(false);
            var entitySlots = entities;

            var slots = new List<Slot>();


            foreach (var activitySlot in activitySlots)
            {
                var slot = new Slot
                {
                    KeyValue = new KeyValuePair<string, object>("GetStateName", activitySlot.Replace("[", "").Replace("]", ""))
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
