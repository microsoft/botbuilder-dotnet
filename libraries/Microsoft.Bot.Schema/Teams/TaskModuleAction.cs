// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.Teams
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
            JToken data;
            if (value == null)
            {
                data = new JObject();
            }
            else
            {
                if (value is string)
                {
                    data = JObject.Parse(value as string);
                }
                else
                {
                    data = JObject.FromObject(value, JsonSerializer.Create(new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.None
                    }));
                }
            }

            data["type"] = "task/fetch";
            this.Value = data.ToString();
        }
    }
}
