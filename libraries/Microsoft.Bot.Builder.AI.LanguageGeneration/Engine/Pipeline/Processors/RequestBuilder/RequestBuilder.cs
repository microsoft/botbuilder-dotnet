using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using LanguageGeneration.V2;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The concrete class for building the <see cref="ICompositeRequest"/> object, which in turn contains all the unique template refereces in user <see cref="Activity"/> and the <see cref="Slot"/> objets that contains user entity values.
    /// </summary>
    internal class RequestBuilder : IRequestBuilder
    {
        private readonly string _applicationId;
        public RequestBuilder(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid Language generation application id.");
            }
            _applicationId = applicationId;
        }

        /// <summary>
        /// The main method to build the <see cref="ICompositeRequest"/> object.
        /// </summary>
        /// <param name="slots">The <see cref="IList{Slot}"/>.</param>
        /// <param name="locale">Locale.</param>
        /// <returns>A <see cref="ICompositeRequest"/>.</returns>
        public ICompositeRequest BuildRequest(IList<Slot> slots, string locale)
        {
            if (slots == null)
            {
                throw new ArgumentNullException(nameof(slots));
            }

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
                        var lgValue = new LGValue(LgValueType.BooleanType)
                        {
                            BooleanValues = new List<bool> { (bool)slot.KeyValue.Value }
                        };
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }

                    else if (slot.Type == SlotTypeEnum.DateTimeType)
                    {
                        var lgValue = new LGValue(LgValueType.DateTimeType)
                        {
                            DateTimeValues = new List<DateTime> { (DateTime)slot.KeyValue.Value }
                        };
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }

                    else if (slot.Type == SlotTypeEnum.FloatType)
                    {
                        var lgValue = new LGValue(LgValueType.FloatType)
                        {
                            FloatValues = new List<float> { (float)slot.KeyValue.Value }
                        };
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }

                    else if (slot.Type == SlotTypeEnum.IntType)
                    {
                        var lgValue = new LGValue(LgValueType.IntType)
                        {
                            IntValues = new List<int> { (int)slot.KeyValue.Value }
                        };
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }

                    else if (slot.Type == SlotTypeEnum.StringType)
                    {
                        var lgValue = new LGValue(LgValueType.StringType)
                        {
                            StringValues = new List<string> { (string)slot.KeyValue.Value }
                        };
                        commonSlotsDictionary.Add(slot.KeyValue.Key, lgValue);
                    }

                    else
                    {
                        throw new ArgumentException("Unknown slot type");
                    }
                }
            }

            foreach (var slot in perRequestSlots)
            {
                var lgRequest = new LGRequest()
                {
                    Scenario = _applicationId,
                    Locale = locale,
                    TemplateId = (string)slot.KeyValue.Value
                };

                lgRequest.Slots = commonSlotsDictionary;
                compositeRequest.Requests.Add(lgRequest.TemplateId, lgRequest);
            }

            return compositeRequest;
        }
    }
}
