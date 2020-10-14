// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Represents a property found in a JSON schema.
    /// </summary>
    public class PropertySchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySchema"/> class.
        /// </summary>
        /// <param name="path">Path to this property.</param>
        /// <param name="schema">JSON schema fragment for this property.</param>
        /// <param name="children">Children properties if any.</param>
        public PropertySchema(string path, JObject schema, List<PropertySchema> children = null)
        {
            Path = path;
            Schema = schema;
            if (children != null)
            {
                Children = children;
                foreach (var child in Children)
                {
                    child.Parent = this;
                }
            }
            else
            {
                Children = new List<PropertySchema>();
            }

            var list = new List<string>();
            var entities = schema["$entities"]?.Value<JArray>() ?? schema["items"]?["$entities"].Value<JArray>();
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    list.Add(entity.Value<string>());
                }
            }

            Entities = list;

            list = new List<string>();
            var expected = schema["$expectedOnly"]?.Value<JArray>();
            if (expected != null)
            {
                foreach (var entity in expected)
                {
                    list.Add(entity.Value<string>());
                }

                ExpectedOnly = list;
            }
        }

        /// <summary>
        /// Gets name for this property.
        /// </summary>
        /// <value>
        /// Name for this property.
        /// </value>
        public string Name => Path.Split('.').Last().Replace("[]", string.Empty);

        /// <summary>
        /// Gets path to schema with [] for arrays and otherwise . for path accessors.
        /// </summary>
        /// <value>
        /// Path to schema with [] for arrays and otherwise . for path accessors.
        /// </value>
        public string Path { get; }

        /// <summary>
        /// Gets JSON Schema object for this property.
        /// </summary>
        /// <value>
        /// JSON Schema object for this property.
        /// </value>
        public JObject Schema { get; }

        /// <summary>
        /// Gets parent schema.
        /// </summary>
        /// <value>Parent property schema if any.</value>
        public PropertySchema Parent { get; private set; }

        /// <summary>
        /// Gets the JSON Schema type for this property.
        /// </summary>
        /// <value>JSON Schema type.</value>
        public string Type => Schema["type"].Value<string>();

        /// <summary>
        /// Gets a value indicating whether type is an array.
        /// </summary>
        /// <value>True if array.</value>
        public bool IsArray => Schema?.Parent?.Parent["type"]?.Value<string>() == "array";

        /// <summary>
        /// Gets a value indicating whether property is an enum.
        /// </summary>
        /// <value>True if enum.</value>
        public bool IsEnum => Schema["enum"] != null;

        /// <summary>
        /// Gets mappings to possible entities for property.
        /// </summary>
        /// <value>List of entity names.</value>
        public IReadOnlyList<string> Entities { get; }

        /// <summary>
        /// Gets list of entities that are only recognized for this property when expected.
        /// </summary>
        /// <value>List of expected only entity names.</value>
        public IReadOnlyList<string> ExpectedOnly { get; }

        /// <summary>
        /// Gets child properties if there are any.
        /// </summary>
        /// <value>
        /// Child properties if there are any.
        /// </value>
        public IReadOnlyList<PropertySchema> Children { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"PropertySchema({Path})";
    }
}
