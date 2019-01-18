// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders
{
    public class SetVariableCommandLoader : ILoader
    {
        public object Load(JObject obj, JsonSerializer serializer, Type type)
        {
            // Allow expressions for the Value property in string format directly
            // Example:
            // "Name": "Age",
            // "Value": "DialogTurnResult.Result"
            if (obj["Value"].Type == JTokenType.String)
            {
                return new SetVarStep()
                {
                    Name = obj.Value<string>("Name"),
                    Value = new CommonExpression(obj.Value<string>("Value"))
                };
            }
            return obj.ToObject(type, serializer);
        }
    }
}
