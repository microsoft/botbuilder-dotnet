// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Path;
using Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Memory
{
    /// <summary>
    /// Implementation of <see cref="IMemory"/> over JsonObject.
    /// </summary>
    public partial class JsonNodeMemory : IMemory
    {
        private JsonNode _root = null;
        private JsonSerializerContext _serializerContext = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNodeMemory"/> class.
        /// </summary>
        /// <param name="root">The object to wrap.</param>
        /// <param name="serializerContext">Optional serializerContext to support TryEvaluate<![CDATA[<T>]]> overloads.</param>
        public JsonNodeMemory(JsonNode root, JsonSerializerContext serializerContext = null)
        {
            _root = root;
            _serializerContext = serializerContext;
        }

        /// <inheritdoc/>
        public object ConvertTo(Type type, object value)
        {
            if (value == null)
            {
                return null;
            }

            if (_serializerContext != null)
            {
                return JsonSerializer.Deserialize(JsonSerializer.Serialize(value, value.GetType(), _serializerContext), type, _serializerContext);
            }

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IMemory CreateMemoryFrom(object value)
        {
            if (value is JsonNode jnode)
            {
                return new JsonNodeMemory(jnode);
            }
            else if (value is List<object> 
                || value is List<JsonNode>
                || value is Dictionary<string, object> 
                || value is KeyValuePair<string, JsonNode>
                || value is Dictionary<string, JsonNode>
                || value is Dictionary<string, object>)
            {
                return new JsonNodeMemory(ConvertToJsonNode(value));
            }
            else if (value == null)
            {
                return new JsonNodeMemory(null);
            }

            throw new InvalidOperationException("Expected JsonNode");
        }

        /// <inheritdoc/>
        public string JsonSerializeToString(object value)
        {
#pragma warning disable IDE0011, SA1503 // Add braces
            if (value is null) return "null";
            if (value is bool) return JsonSerializer.Serialize(value, SerializerContext.Default.Boolean);
            if (value is int) return JsonSerializer.Serialize(value, SerializerContext.Default.Int32);
            if (value is long) return JsonSerializer.Serialize(value, SerializerContext.Default.Int64);
            if (value is float) return JsonSerializer.Serialize(value, SerializerContext.Default.Single);
            if (value is double) return JsonSerializer.Serialize(value, SerializerContext.Default.Double);
            if (value is string) return JsonSerializer.Serialize(value, SerializerContext.Default.String);
            if (value is decimal) return JsonSerializer.Serialize(value, SerializerContext.Default.Decimal);
            if (value is List<object>) return JsonSerializer.Serialize(value, SerializerContext.Default.ListObject);
            if (value is List<JsonNode>) return JsonSerializer.Serialize(value, SerializerContext.Default.ListJsonNode);
            if (value is KeyValuePair<string, JsonNode>) return JsonSerializer.Serialize(value, SerializerContext.Default.KeyValuePairStringJsonNode);
            if (value is Dictionary<string, object>) return JsonSerializer.Serialize(value, SerializerContext.Default.DictionaryStringObject);
            if (value is Dictionary<string, JsonNode>) return JsonSerializer.Serialize(value, SerializerContext.Default.DictionaryStringJsonNode);
            if (value is JsonNode) return JsonSerializer.Serialize(value, SerializerContext.Default.JsonNode);
#pragma warning restore IDE0011, SA1503 // Add braces

            throw new InvalidOperationException("Expected JsonNode");
        }

        /// <inheritdoc/>
        public JsonNode SerializeToNode(object value)
        {
            // All objects we get should either be JsonNodes or primitive types
            return ConvertToJsonNode(value);
        }

        /// <inheritdoc/>
        public void SetValue(string path, object value)
        {
            var jval = ConvertToJsonNode(value);

            var current = _root;
            foreach (var part in new SimplePathEnumerator(path))
            {
                if (part.Index is int index)
                {
                    if (part.IsLast)
                    {
                        if (current is JsonArray jarray)
                        {
                            var count = jarray.Count;
                            if (index > count)
                            {
                                throw new InvalidOperationException($"{index} index out of range");
                            }
                            else if (index == count)
                            {
                                jarray.Add(jval);
                            }
                            else
                            {
                                jarray[index] = jval;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"set value for an index to a non-list object");
                        }
                    }
                    else
                    {
                        current = current[index];
                    }
                }
                else
                {
                    if (part.IsLast)
                    {
                        current[part.Part] = jval;
                    }
                    else
                    {
                        current = current[part.Part];
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool TryGetValue(string path, out object value)
        {
            value = null;
            var current = _root;
            foreach (var part in new SimplePathEnumerator(path))
            {
                if (part.Index is int index)
                {
                    if (current is JsonArray jarray)
                    {
                        if (index >= 0 && index <= jarray.Count)
                        {
                            current = jarray[index];
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (current is JsonObject jobj)
                    {
                        if (!jobj.TryGetPropertyValue(part.Part, out current))
                        {
                            // SimpleObjectMemory did invariant key lookup for get (not set!), so fall back to that if needed.
                            bool found = false;
                            foreach (var kvp in jobj)
                            {
                                if (kvp.Key.Equals(part.Part, StringComparison.OrdinalIgnoreCase))
                                {
                                    found = true;
                                    current = kvp.Value;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            value = FunctionUtils.ResolveValue(current);
            return true;
        }

        /// <inheritdoc/>
        public string Version()
        {
            return JsonSerializeToString(_root);
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All valid json types are supported by the serializer context")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "All valid json types are supported by the serializer context")]
        private JsonNode ConvertToJsonNode(object value)
        {
            if (value is null)
            {
                return null;
            }
            else if (value is JsonNode node)
            {
                return node.DeepClone();
            }
            
            return JsonSerializer.SerializeToNode(value, value.GetType(), SerializerContext.Default.Options);
        }

        [JsonSerializable(typeof(JsonNode))]
        [JsonSerializable(typeof(JsonObject))]
        [JsonSerializable(typeof(string))]
        [JsonSerializable(typeof(bool))]
        [JsonSerializable(typeof(int))]
        [JsonSerializable(typeof(long))]
        [JsonSerializable(typeof(decimal))]
        [JsonSerializable(typeof(float))]
        [JsonSerializable(typeof(double))]
        [JsonSerializable(typeof(List<object>))]
        [JsonSerializable(typeof(List<JsonNode>))]
        [JsonSerializable(typeof(Dictionary<string, object>))]
        [JsonSerializable(typeof(Dictionary<string, JsonNode>))]
        [JsonSerializable(typeof(KeyValuePair<string, JsonNode>))]
        private partial class SerializerContext : JsonSerializerContext
        { 
        }
    }
}
