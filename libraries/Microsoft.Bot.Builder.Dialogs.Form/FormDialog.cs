using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class FormDialog : AdaptiveDialog
    {
        public FormDialog(DialogSchema inputSchema, DialogSchema outputSchema)
        {
            InputSchema = inputSchema;
            OutputSchema = outputSchema;
            if (outputSchema.Schema["type"].Value<string>() != "object")
            {
                throw new ArgumentException("Forms must be an object schema.");
            }

            GenerateRules();
        }

        public DialogSchema InputSchema { get; }

        public DialogSchema OutputSchema { get; }

        public void AnalyzeSchema(Func<string, JToken, bool> analyzer)
        {
            // AnalyzeSchema(OutputSchema, string.Empty, analyzer);
        }

        public JObject SchemaFor(string propertyPath)
        {
            JObject schema = null;
            AnalyzeSchema((path, token) =>
            {
                bool stop = false;
                if (path == propertyPath)
                {
                    stop = true;
                    schema = token as JObject;
                }

                return stop;
            });
            return schema;
        }

        protected bool AnalyzeSchema(JToken token, string path, Func<string, JToken, bool> analyzer)
        {
            bool stop;
            if (token is JObject jobj)
            {
                if (!(stop = analyzer(path, token)))
                {
                    // Arrays terminate schema from a slot mapping standpoint. 
                    // We can handle mapping multiple leaf values, but not nested ones unless there is a sub-dialog.
                    if (!path.EndsWith("[]"))
                    {
                        foreach (var prop in jobj.Properties())
                        {
                            var parent = prop.Parent;
                            var grand = parent?.Parent;
                            var newPath = path;
                            if (grand != null && grand is JProperty grandProp && grandProp.Name == "properties")
                            {
                                newPath = path == string.Empty ? prop.Name : $"{path}.{prop.Name}";
                                var type = prop.Value["type"];
                                if (type != null && type.Value<string>() == "array")
                                {
                                    newPath += "[]";
                                }
                            }

                            if (stop = AnalyzeSchema(prop.Value, newPath, analyzer))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                stop = analyzer(path, token);
            }

            return stop;
        }

        // For simple singleton slot:
        //  Set values
        //      count(@@foo) == 1 -> foo == @foo
        //      count(@@foo) > 1 -> "Which {@@foo} do you want for {slotName}"
        //  Constraints (which are more specific)
        //      count(@@foo) == 1 && @foo < 0 -> "{@foo} is too small for {slotname}"
        //      count(@@foo) > 1 && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        // For simple array slot:
        //  Set values:
        //      @@foo -> foo = @@foo
        //  Constraints: (which are more specific)
        //      @@foo && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0) are too small for {slotname}"
        //  Modification--based on intent?
        //      add: @@foo && @intent == add -> Append(@@foo, foo)
        //      // This is to make this more specific than both the simple constraint and the intent
        //      add: @@foo && count(where(@@foo, foo, foo < 0)) > 0 && @intent == add -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        //      delete: @@foo @intent == delete -> Delete(@@foo, foo)
        // For structured singleton slot
        //  count(@@foo) == 1 -> Apply child constraints, i.e. make a new singleton object to apply child property rule sets to it.
        //  count(@@foo) > 1 -> "Which one did you want?" with replacing @@foo with the singleton selection
        //
        // Children slots can either:
        // * Refer to parent structure which turns into count(first(parent).child) == 1
        // * Refer to independent entity, i.e. count(datetime) > 1
        //
        // Assumptions:
        // * In order to map structured entities to structured slots, parent structures must be singletons before child can map them.
        // * We will only generate a single instance of the form.  (Although there can be multiple ones inside.)
        // * You can map directly, but then must deal with that complexity of structures.  For example if you have multiple flight(origin, destination) and you want to map to hotel(location)
        //   you have to figure out how to deal with multiple flight structures and the underlying entity structures.
        // * For now only leaves can be arrays.  If you need more, I think it is a subform.
        //
        // 1) Find all most specific matches
        // 2) Identify any slots that compete for the same entity.  Select by in expected, then keep as slot ambiguous.
        // 3) For each entity either: a) Do its set, b) queue up clarification, c) surface as unhandled
        // 
        // Two cases:
        // 1) Flat entity resolution, treat properties as independent.
        // 2) Hierarchical, the first level you get to count(@@flight) == 1, then for count(first(@@flight).origin) == 1
        // We know which is which by entity path, i.e. flight.origin -> hierarchical whereas origin is flat.
        protected void GenerateRules()
        {
            foreach (var child in OutputSchema.Property.Children)
            {
                GenerateRules(child);
            }
        }

        protected void GenerateRules(PropertySchema property)
        {
            if (property.Children.Count() > 0)
            {
                throw new ArgumentException($"{property.Path} is a complex object and that is not supported yet.");
            }

            foreach (var mapping in property.Mappings)
            {
                var entity = mapping.Value<string>()?.Replace("[]", string.Empty);
                if (entity != null)
                {
                    if (property.IsArray)
                    {
                        AddRule(new FormRule(
                            constraint: $"{entity}",
                            steps: new List<IDialog>
                            {
                                    new SetProperty
                                    {
                                        Property = FormPath(property.Path),
                                        Value = entity
                                    }
                            }));
                    }
                    else
                    {
                        AddRule(new FormRule(
                            constraint: $"count({entity}) == 1",
                            steps: new List<IDialog>
                            {
                                    new SetProperty
                                    {
                                        Property = FormPath(property.Path),
                                        Value = entity
                                    }
                            }));

                        // TODO: Add disambiguation and constraint rules
                    }
                }
                else
                {
                    // TODO: How to load IRule?
                }
            }
        }

        protected string FormPath(string schemaPath) => $"^form.{schemaPath.Replace("[]", string.Empty)}";

        protected JToken Mappings(string path, JObject schema)
        {
            JArray mapping = schema["mappings"] as JArray;
            if (mapping == null)
            {
                var simplePath = path.Replace("[]", string.Empty);
                if (path.Contains("."))
                {
                    // Default mapping is to isomorphic structure corresponding to parent
                    mapping = new JArray($"dialog.form.{simplePath}");
                }
                else
                {
                    mapping = new JArray($"@@{simplePath}");
                }
            }

            return mapping;
        }
    }
}
