// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// This class defines the configuration settings used when serializing and deserializing data
    /// contained in objects defined by the Bot Framework Protocol schema.
    /// </summary>
    public static class SerializationConfig
    {
        /// <summary>
        /// The default serialization settings for use in most cases.
        /// </summary>
        public static readonly JsonSerializerOptions DefaultSerializeOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new UtcDateTimeConverter(),
                new DeliveryModesConverter(),
                new ActivityImportanceConverter(),
                new EndOfConversationCodesConverter(),
                new InputHintsConverter(),
                new AttachmentLayoutTypesConverter(),
                new TextFormatTypesConverter(),
                new RoleTypesConverter(),
                new ActivityTypesConverter(),
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        /// <summary>
        /// The default deserialization settings for use in most cases.
        /// </summary>
        public static readonly JsonSerializerOptions DefaultDeserializeOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new UtcDateTimeConverter(),
                new DeliveryModesConverter(),
                new ActivityImportanceConverter(),
                new EndOfConversationCodesConverter(),
                new InputHintsConverter(),
                new AttachmentLayoutTypesConverter(),
                new TextFormatTypesConverter(),
                new RoleTypesConverter(),
                new ActivityTypesConverter(),
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        private class UtcDateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetDateTime().ToUniversalTime();
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToUniversalTime());
            }
        }

        private class DeliveryModesConverter : JsonConverter<DeliveryModes>
        {
            public override DeliveryModes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new DeliveryModes(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, DeliveryModes value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class ActivityImportanceConverter : JsonConverter<ActivityImportance>
        {
            public override ActivityImportance Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new ActivityImportance(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, ActivityImportance value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class EndOfConversationCodesConverter : JsonConverter<EndOfConversationCodes>
        {
            public override EndOfConversationCodes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new EndOfConversationCodes(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, EndOfConversationCodes value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class InputHintsConverter : JsonConverter<InputHints>
        {
            public override InputHints Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new InputHints(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, InputHints value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class AttachmentLayoutTypesConverter : JsonConverter<AttachmentLayoutTypes>
        {
            public override AttachmentLayoutTypes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new AttachmentLayoutTypes(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, AttachmentLayoutTypes value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class TextFormatTypesConverter : JsonConverter<TextFormatTypes>
        {
            public override TextFormatTypes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new TextFormatTypes(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, TextFormatTypes value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class RoleTypesConverter : JsonConverter<RoleTypes>
        {
            public override RoleTypes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new RoleTypes(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, RoleTypes value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class ActivityTypesConverter : JsonConverter<ActivityTypes>
        {
            public override ActivityTypes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new ActivityTypes(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, ActivityTypes value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
