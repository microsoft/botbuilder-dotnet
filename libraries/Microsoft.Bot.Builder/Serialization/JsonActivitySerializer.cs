// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Serialization
{
    /// <summary>
    /// Serializes <see cref="Activity"/> instances to and from their JSON representation.
    /// </summary>
    public sealed class JsonActivitySerializer : IActivitySerializer
    {
        private const int JsonWriterBufferSize = 8192;

        private static readonly JsonSerializerSettings ActivityJsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ReadOnlyJsonContractResolver(),
            Converters = new List<JsonConverter> { new Iso8601TimeSpanConverter() },
        };

        private static readonly JsonSerializer ActivityJsonSerializer = JsonSerializer.Create(ActivityJsonSerializerSettings);

        public JsonActivitySerializer()
        {
        }

        /// <inheritdoc />
        public async Task<Activity> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var activityJsonToken = default(JToken);

            using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                activityJsonToken = await JToken.ReadFromAsync(jsonReader, cancellationToken).ConfigureAwait(false);
            }

            return await DeserializeAsync((JObject)activityJsonToken, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SerializeAsync(Activity activity, Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var activityJsonObject = JObject.FromObject(activity);

            using (var writer = new JsonTextWriter(new StreamWriter(stream, Encoding.UTF8, JsonWriterBufferSize, leaveOpen: true)))
            {
                await activityJsonObject.WriteToAsync(writer, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<Activity> DeserializeAsync(JObject jsonObject, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (jsonObject == null)
            {
                throw new ArgumentNullException(nameof(jsonObject));
            }

            if (jsonObject.Type != JTokenType.Object)
            {
                throw new ActivitySerializationException($"Can't deserialize from the specified JSON token. Was expecting {JTokenType.Object}, but found {jsonObject.Type}.");
            }

            var activityTypeToken = jsonObject["type"] ?? throw new ActivitySerializationException("Cannot determine activity type because the required property \"type\" was found on the root JSON object.");
            var activityRuntimeType = ActivityTypeConverter.GetRuntimeType(activityTypeToken.Value<string>());

            return (Activity)jsonObject.ToObject(activityRuntimeType);
        }
    }
}
