// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    internal class SchemaHelper
    {
        public SchemaHelper(JObject schema)
        {
            Schema = schema;
            Property = CreateProperty(Schema);
        }

        public JObject Schema { get; }

        public PropertySchema Property { get; }

        public static SchemaHelper ReadSchema(string path)
            => new SchemaHelper(JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path)));

        public IEnumerable<PropertySchema> Properties()
            => Property.Children;

        public JArray Required()
            => Schema["required"] as JArray ?? new JArray(Property.Children.Select(c => c.Name));

        public PropertySchema PathToSchema(string path)
        {
            var property = Property;
            if (path != null)
            {
                var steps = path.Split('.', '[');
                var step = 0;
                while (property != null && step < steps.Length)
                {
                    var found = false;
                    foreach (var child in property.Children)
                    {
                        if (child.Name == steps[step])
                        {
                            property = child;
                            while (++step < steps.Length && (steps[step] == "." || steps[step] == "[]"))
                            {
                            }

                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        property = null;
                    }
                }
            }

            return property;
        }

        public void Analyze(Func<string, JToken, bool> analyzer)
        {
            Analyze(string.Empty, Schema, analyzer);
        }

        private PropertySchema CreateProperty(JObject schema, string path = "")
        {
            var type = schema["type"].Value<string>();
            var children = new List<PropertySchema>();
            if (type == "array")
            {
                path += "[]";
                var items = schema["items"];
                if (items != null)
                {
                    if (items is JObject itemsSchema)
                    {
                        // Copy parent $ properties like $entities
                        foreach (JProperty prop in schema.Properties())
                        {
                            if (prop.Name.StartsWith("$", StringComparison.Ordinal))
                            {
                                itemsSchema[prop.Name] = prop.Value;
                            }
                        }

                        schema = itemsSchema;
                        type = schema["type"].Value<string>();
                    }
                    else
                    {
                        throw new ArgumentException($"{path} has an items array which is not supported");
                    }
                }
            }

            if (type == "object")
            {
                foreach (JProperty prop in schema["properties"])
                {
                    if (!prop.Name.StartsWith("$", StringComparison.Ordinal))
                    {
                        var newPath = path.Length == 0 ? prop.Name : $"{path}.{prop.Name}";
                        children.Add(CreateProperty((JObject)prop.Value, newPath));
                    }
                }
            }

            return new PropertySchema(path, schema, children);
        }

        private bool Analyze(string path, JToken token, Func<string, JToken, bool> analyzer)
        {
            var stop = false;
            if (token is JObject jobj)
            {
                if (!(stop = analyzer(path, token)))
                {
                    foreach (var prop in jobj.Properties())
                    {
                        var parent = prop.Parent;
                        var grand = parent?.Parent;
                        var newPath = path;
                        if (grand != null && grand is JProperty grandProp && grandProp.Name == "properties")
                        {
                            newPath = path.Length == 0 ? prop.Name : $"{path}.{prop.Name}";
                        }

                        if (stop = Analyze(newPath, prop.Value, analyzer))
                        {
                            break;
                        }
                    }
                }
            }
            else if (token is JArray jarr)
            {
                foreach (var elt in jarr.Children())
                {
                    if (stop = Analyze(path, elt, analyzer))
                    {
                        break;
                    }
                }
            }
            else
            {
                stop = analyzer(path, token);
            }

            return stop;
        }
    }
}
