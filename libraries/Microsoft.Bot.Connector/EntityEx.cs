// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Connector
{
    public partial class Entity
    {
        /// <summary>
        /// Extension data for overflow of properties
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; set; } = new JObject();

        /// <summary>
        /// Retrieve internal payload.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAs<T>()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(this));
        }

        /// <summary>
        /// Set internal payload.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void SetAs<T>(T obj)
        {
            var entity = JsonConvert.DeserializeObject<Entity>(JsonConvert.SerializeObject(obj));
            this.Type = entity.Type;
            this.Properties = entity.Properties;
        }
    }
}
