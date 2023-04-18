// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    public class SurfaceConverter : JsonConverter
    {
        /// <summary>
        /// Gets a value indicating whether this Converter can write JSON.
        /// </summary>
        /// <value>true if this Converter can write JSON; otherwise, false.</value>
        public override bool CanWrite => false;

        /// <summary>
        /// Gets a value indicating whether this Converter can read JSON.
        /// </summary>
        /// <value>true if this Converter can read JSON; otherwise, false.</value>
        public override bool CanRead => true;

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Surface);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var surfaceType = jsonObject.GetValue("surface", StringComparison.OrdinalIgnoreCase)?.ToObject<SurfaceType>();

            Surface parsedSurface;
            switch (surfaceType)
            {
                case SurfaceType.MeetingStage:
                    var contentType = jsonObject.GetValue("contentType", StringComparison.OrdinalIgnoreCase)?.ToObject<ContentType>();
                    parsedSurface = CreateMeetingStageSurfaceWithContentType(contentType);
                    break;
                case SurfaceType.MeetingTabIcon:
                    parsedSurface = new MeetingTabIconSurface();
                    break;
                default:
                    throw new ArgumentException($"Invalid surface type: {surfaceType}");
            }

            serializer.Populate(jsonObject.CreateReader(), parsedSurface);
            return parsedSurface;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The Newtonsoft.Json.JsonWriter to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        private static Surface CreateMeetingStageSurfaceWithContentType(ContentType? contentType)
        {
            switch (contentType)
            {
                case ContentType.Task:
                    return new MeetingStageSurface<TaskModuleContinueResponse>();
                default:
                    throw new ArgumentException($"Invalid content type: {contentType}");
            }
        }
    }
}
