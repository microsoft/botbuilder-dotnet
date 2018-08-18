using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using LanguageGeneration.V2;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class RequestBuilder : IRequestBuilder
    {
        public async Task<ICompositeRequest> BuildRequestAsync(IList<Slot> slots)
        {
            ICompositeRequest compositeRequest = new CompositeRequest();
            IList<Slot> perRequestSlots = new List<Slot>();
            var commonSlotsDictionary = new LGSlotDictionary();

            foreach (var slot in slots)
            {
                if (slot.KeyValue.Key == "GetStateName")
                {
                    perRequestSlots.Add(slot);
                }
                else
                {
                    if (slot.Type == SlotTypeEnum.BooleanType)
                    {
                        var lgValue = new LGValue(LgValueType.BooleanType);
                        lgValue.BooleanValues = (List<bool>)slot.KeyValue.Value;
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }

                    else if (slot.Type == SlotTypeEnum.DateTimeType)
                    {
                        var lgValue = new LGValue(LgValueType.DateTimeType);
                        lgValue.DateTimeValues = (List<DateTime>)slot.KeyValue.Value;
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }

                    else if (slot.Type == SlotTypeEnum.FloatType)
                    {
                        var lgValue = new LGValue(LgValueType.FloatType);
                        lgValue.FloatValues = (List<float>)slot.KeyValue.Value;
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }

                    else if (slot.Type == SlotTypeEnum.IntType)
                    {
                        var lgValue = new LGValue(LgValueType.IntType);
                        lgValue.IntValues = (List<int>)slot.KeyValue.Value;
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }

                    else if (slot.Type == SlotTypeEnum.StringType)
                    {
                        var lgValue = new LGValue(LgValueType.StringType);
                        lgValue.StringValues = (List<string>)slot.KeyValue.Value;
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }
                }
            }

            foreach (var slot in perRequestSlots)
            {
                var lgRequest = new LGRequest();

                var lgValue = new LGValue(LgValueType.StringType);
                lgValue.StringValues = (List<string>)slot.KeyValue.Value;
                lgRequest.Slots = commonSlotsDictionary;
                lgRequest.Slots.Add(slot.KeyValue.Key, lgValue);

                compositeRequest.Requests.Add(slot.KeyValue.Key, lgRequest);
            }

            return compositeRequest;
        }
    }
}
