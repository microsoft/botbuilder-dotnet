// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Memory
{
    /// <summary>
    /// Memory interface.
    /// </summary>
    public interface IMemory
    {
        /// <summary>
        /// Set value to a given path.
        /// </summary>
        /// <param name="path">memory path.</param>
        /// <param name="value">Value to set.</param>
        void SetValue(string path, object value);

        /// <summary>
        /// Try get value from a given path, it can be a simple identifier like "a", or
        /// a combined path like "a.b", "a.b[2]", "a.b[2].c", inside [] is guaranteed to be a int number or a string.
        /// </summary>
        /// <param name="path">memory path.</param>
        /// <param name="value">resolved value.</param>
        /// <returns> true if the memory contains an element with the specified key; otherwise, false.</returns>
        bool TryGetValue(string path, out object value);

        /// <summary>
        /// Version is used to identify whether the a particular memory instance has been updated or not.
        /// If version is not changed, the caller may choose to use the cached result instead of recomputing everything.
        /// </summary>
        /// <returns>A string indicates the version.</returns>
        string Version();

        /// <summary>
        /// Create an IMemory from an object that was returned from this IMemory's TryGetValue.
        /// If upgrading from old AdaptiveExpression implementations, return MemoryFactory.Create(value).
        /// </summary>
        /// <param name="value">object to wrap.</param>
        /// <returns>IMemory.</returns>
        IMemory CreateMemoryFrom(object value);

        /// <summary>
        /// Serialize a value sourced from this IMemory into a string.
        /// If upgrading from old AdaptiveExpression implementations, return JsonSerializer.Serialize(value).
        /// </summary>
        /// <param name="value">object to serialize.</param>
        /// <returns>json string.</returns>
        string JsonSerializeToString(object value);

        /// <summary>
        /// Serialize a value sourced from this IMemory into a JsonNode.
        /// If upgrading from old AdaptiveExpression implementations, return value == null ? null : JsonSerializer.SerializeToNode(value).
        /// </summary>
        /// <param name="value">object to serialize.</param>
        /// <returns>json node.</returns>
        JsonNode SerializeToNode(object value);

        /// <summary>
        /// When an expression evaluates to an object that is not the exact type TryEvaluate<![CDATA[<T>]]> needs,
        /// it asks the IMemory to convert the value to the specific type. The return value will be cast to T so it must be
        /// of the correct type.
        /// A default implementation can do JsonSerializer.Deserialize(JsonSerializer.SerializeToNode(value), type).
        /// </summary>
        /// <param name="type">type to convert to.</param>
        /// <param name="value">value to convert.</param>
        /// <returns>value converted to type.</returns>
        object ConvertTo(Type type, object value);
    }
}
