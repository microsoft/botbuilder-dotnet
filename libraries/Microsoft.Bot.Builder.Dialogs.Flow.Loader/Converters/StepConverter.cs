// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Resolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Converters
{
    public class StepConverter : InterfaceConverter<IStep>
    {
        public StepConverter(IRefResolver resolver) : base(resolver) { }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Sequence);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);

            if (jToken is JArray)
            {
                // If we have an array of IStep, create a sequence
                // and add all the array elements to make for a more compact format.
                // Users can still specify a Sequence explicitly if they prefer so.
                var sequence = new Sequence();

                foreach (var stepToken in jToken as JArray)
                {
                    var stepObj = stepToken.ToObject<IStep>(serializer);
                    sequence.Add(stepObj);
                }

                return sequence;
            }

            // If we have a single step, call the generic InterfaceConverter<IStep>.
            // Note that since we did JToken.Load(reader) above, we already 'consumed' the json
            // in the deserialization process, so we want to create a new reader or otherwise
            // the base class will try to read from the JsonReader and it will be empty.
            return base.ReadJson(jToken.CreateReader(), objectType, existingValue, serializer);
        }

    }
}
