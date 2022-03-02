// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Adapter class to represent BotBuilder card action as adaptive card action (in type of Action.Submit).
    /// </summary>
    public class TaskModuleAction : CardAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleAction"/> class.
        /// </summary>
        /// <param name="title">Button title.</param>
        /// <param name="value">Free hidden value binding with button. The value will be sent out with "task/fetch" invoke event.</param>
        public TaskModuleAction(string title, object value = null)
            : base("invoke", title)
        {
            var data = value == null
                ? new Dictionary<string, JsonElement>()
                : value.ToJsonElements();

            foreach (var element in new { type = "task/fetch" }.ToJsonElements())
            {
                data.Add(element.Key, element.Value);
            }

            Value = JsonSerializer.Serialize(data, SerializationConfig.DefaultSerializeOptions);
        }
    }
}
