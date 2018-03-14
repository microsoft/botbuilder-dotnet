//// 
//// Copyright (c) Microsoft. All rights reserved.
//// Licensed under the MIT license.
//// 
//// Microsoft Bot Framework: http://botframework.com
//// 
//// Bot Builder SDK GitHub:
//// https://github.com/Microsoft/BotBuilder
//// 
//// Copyright (c) Microsoft Corporation
//// All rights reserved.
//// 
//// MIT License:
//// Permission is hereby granted, free of charge, to any person obtaining
//// a copy of this software and associated documentation files (the
//// "Software"), to deal in the Software without restriction, including
//// without limitation the rights to use, copy, modify, merge, publish,
//// distribute, sublicense, and/or sell copies of the Software, and to
//// permit persons to whom the Software is furnished to do so, subject to
//// the following conditions:
//// 
//// The above copyright notice and this permission notice shall be
//// included in all copies or substantial portions of the Software.
//// 
//// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
//// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//using Microsoft.Bot.Builder.Classic.Dialogs;
//using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Microsoft.Bot.Builder.Classic.FormFlow.Json
//{
//    // No need to document overrides of interface methods
//#pragma warning disable CS1591

//    /// <summary>
//    /// %Field defined through JSON Schema.
//    /// </summary>
//    public class FieldJson : Field<JObject>
//    {
//        /// <summary>
//        /// Construct a field from a JSON schema.
//        /// </summary>
//        /// <param name="builder">Form builder.</param>
//        /// <param name="name">Name of field within schema.</param>
//        public FieldJson(FormBuilderJson builder, string name)
//            : base(name, FieldRole.Value)
//        {
//            _builder = builder;
//            bool optional;
//            var fieldSchema = FieldSchema(name, out optional);
//            var eltSchema = ElementSchema(fieldSchema);
//            ProcessAnnotations(fieldSchema, eltSchema);
//            var fieldName = name.Split('.').Last();
//            JToken date;
//            if (eltSchema.TryGetValue("DateTime", out date) && date.Value<bool>())
//            {
//                SetType(typeof(DateTime));
//            }
//            else
//            {
//                SetType(eltSchema["enum"] != null && eltSchema["enum"].Any() ? null : ToType(eltSchema));
//            }
//            SetAllowsMultiple(IsType(fieldSchema, "array"));
//            SetFieldDescription(ProcessDescription(fieldSchema, Language.CamelCase(fieldName)));
//            var terms = Strings(fieldSchema, "Terms");
//            JToken maxPhrase;
//            if (terms != null && fieldSchema.TryGetValue("MaxPhrase", out maxPhrase))
//            {
//                terms = (from seed in terms
//                         from gen in Language.GenerateTerms(seed, (int)maxPhrase)
//                         select gen).ToArray<string>();
//            }
//            SetFieldTerms(terms ?? Language.GenerateTerms(Language.CamelCase(fieldName), 3));
//            ProcessEnum(eltSchema);
//            SetOptional(optional);
//            SetIsNullable(IsType(fieldSchema, "null"));
//        }

//        #region IFieldState
//        public override object GetValue(JObject state)
//        {
//            object result = null;
//            var val = state.SelectToken(_name);
//            if (val != null)
//            {
//                if (_type == null)
//                {
//                    if (_allowsMultiple)
//                    {
//                        result = val.ToObject<string[]>();
//                    }
//                    else
//                    {
//                        result = (string)val;
//                    }
//                }
//                else
//                {
//                    result = val.ToObject(_type);
//                }
//            }
//            return result;
//        }

//        public override void SetValue(JObject state, object value)
//        {
//            var jvalue = JToken.FromObject(value);
//            var current = state.SelectToken(_name);
//            if (current == null)
//            {
//                var step = state;
//                var steps = _name.Split('.');
//                foreach (var part in steps.Take(steps.Count() - 1))
//                {
//                    var next = step.GetValue(part);
//                    if (next == null)
//                    {
//                        var nextStep = new JObject();
//                        step.Add(part, nextStep);
//                        step = nextStep;
//                    }
//                    else
//                    {
//                        step = (JObject)next;
//                    }
//                }
//                step.Add(steps.Last(), jvalue);
//            }
//            else
//            {
//                current.Replace(jvalue);
//            }
//        }

//        public override bool IsUnknown(JObject state)
//        {
//            return state.SelectToken(_name) == null;
//        }

//        public override void SetUnknown(JObject state)
//        {
//            var token = state.SelectToken(_name);
//            if (token != null)
//            {
//                token.Parent.Remove();
//            }
//        }

//        #endregion

//        internal IEnumerable<MessageOrConfirmation> Before { get; set; }

//        internal IEnumerable<MessageOrConfirmation> After { get; set; }

//        protected JObject FieldSchema(string path, out bool optional)
//        {
//            var schema = _builder.Schema;
//            var parts = path.Split('.');
//            var required = true;
//            foreach (var part in parts)
//            {
//                required = required && (schema["required"] == null || ((JArray)schema["required"]).Any((val) => (string)val == part));
//                schema = (JObject)((JObject)schema["properties"])[part];
//                if (part == null)
//                {
//                    throw new MissingFieldException($"{part} is not a property in your schema.");
//                }
//            }
//            optional = !required;
//            return schema;
//        }

//        protected Type ToType(JObject schema)
//        {
//            Type type = null;
//            if (IsType(schema, "boolean")) type = typeof(bool);
//            else if (IsType(schema, "integer")) type = typeof(long);
//            else if (IsType(schema, "number")) type = typeof(double);
//            else if (IsType(schema, "string")) type = typeof(string);
//            else
//            {
//                throw new ArgumentException($"{schema} does not have a valid C# type.");
//            }
//            return type;
//        }

//        protected string[] Strings(JObject schema, string field)
//        {
//            string[] result = null;
//            JToken array;
//            if (schema.TryGetValue(field, out array))
//            {
//                result = array.ToObject<string[]>();
//            }
//            return result;
//        }

//        protected string AString(JObject schema, string field)
//        {
//            string result = null;
//            JToken astring;
//            if (schema.TryGetValue(field, out astring))
//            {
//                result = (string)astring;
//            }
//            return result;
//        }

//        protected void ProcessAnnotations(JObject fieldSchema, JObject eltSchema)
//        {
//            ProcessTemplates(_builder.Schema);
//            Before = ProcessMessages("Before", fieldSchema);
//            ProcessTemplates(fieldSchema);
//            ProcessPrompt(fieldSchema);
//            ProcessNumeric(fieldSchema);
//            ProcessPattern(fieldSchema);
//            ProcessActive(fieldSchema);
//            ProcessDefine(fieldSchema);
//            ProcessValidation(fieldSchema);
//            ProcessNext(fieldSchema);
//            After = ProcessMessages("After", fieldSchema);
//        }

//        protected void ProcessDefine(JObject schema)
//        {
//            if (schema["Define"] != null)
//            {
//                SetDefine(_builder.DefineScript(this, (string)schema["Define"]));
//            }
//        }

//        protected void ProcessValidation(JObject schema)
//        {
//            if (schema["Validate"] != null)
//            {
//                SetValidate(_builder.ValidateScript(this, (string)schema["Validate"]));
//            }
//        }

//        protected void ProcessNext(JObject schema)
//        {
//            if (schema["Next"] != null)
//            {
//                SetNext(_builder.NextScript(this, (string)schema["Next"]));
//            }
//        }

//        protected void ProcessActive(JObject schema)
//        {
//            if (schema["Active"] != null)
//            {
//                var script = (string)schema["Active"];
//                SetActive(_builder.ActiveScript(this, (string)schema["Active"]));
//            }
//        }

//        internal class MessageOrConfirmation
//        {
//            public bool IsMessage;
//            public PromptAttribute Prompt;
//            public string ActiveScript;
//            public string MessageScript;
//            public IEnumerable<string> Dependencies;
//        }

//        internal IEnumerable<MessageOrConfirmation> ProcessMessages(string fieldName, JObject fieldSchema)
//        {
//            JToken array;
//            if (fieldSchema.TryGetValue(fieldName, out array))
//            {
//                foreach (var message in array.Children<JObject>())
//                {
//                    var info = new MessageOrConfirmation();
//                    if (GetPrompt("Message", message, info))
//                    {
//                        info.IsMessage = true;
//                        yield return info;
//                    }
//                    else if (GetPrompt("Confirm", message, info))
//                    {
//                        info.IsMessage = false;
//                        yield return info;
//                    }
//                    else
//                    {
//                        throw new ArgumentException($"{message} is not Message or Confirm");
//                    }
//                }
//            }
//        }

//        internal bool GetPrompt(string fieldName, JObject message, MessageOrConfirmation info)
//        {
//            bool found = false;
//            JToken val;
//            if (message.TryGetValue(fieldName, out val))
//            {
//                if (val is JValue)
//                {
//                    info.MessageScript = (string)val;
//                }
//                else if (val is JArray)
//                {
//                    info.Prompt = (PromptAttribute)ProcessTemplate(message, new PromptAttribute((from msg in val select (string)msg).ToArray()));
//                }
//                else
//                {
//                    throw new ArgumentException($"{val} must be string or array of strings.");
//                }
//                if (message["Active"] != null)
//                {
//                    info.ActiveScript = (string)message["Active"];
//                }
//                if (message["Dependencies"] != null)
//                {
//                    info.Dependencies = (from dependent in message["Dependencies"] select (string)dependent);
//                }
//                found = true;
//            }
//            return found;
//        }

//        protected void ProcessTemplates(JObject schema)
//        {
//            JToken templates;
//            if (schema.TryGetValue("Templates", out templates))
//            {
//                foreach (JProperty template in templates.Children())
//                {
//                    TemplateUsage usage;
//                    if (Enum.TryParse<TemplateUsage>(template.Name, out usage))
//                    {
//                        ReplaceTemplate((TemplateAttribute)ProcessTemplate(template.Value, new TemplateAttribute(usage)));
//                    }
//                }
//            }
//        }

//        protected void ProcessPrompt(JObject schema)
//        {
//            JToken prompt;
//            if (schema.TryGetValue("Prompt", out prompt))
//            {
//                SetPrompt((PromptAttribute)ProcessTemplate(prompt, new PromptAttribute()));
//            }
//        }

//        protected void ProcessNumeric(JObject schema)
//        {
//            JToken token;
//            double min = -double.MaxValue, max = double.MaxValue;
//            if (schema.TryGetValue("minimum", out token)) min = (double)token;
//            if (schema.TryGetValue("maximum", out token)) max = (double)token;
//            if (min != -double.MaxValue || max != double.MaxValue)
//            {
//                SetLimits(min, max);
//            }
//        }

//        protected void ProcessPattern(JObject schema)
//        {
//            JToken token;
//            if (schema.TryGetValue("pattern", out token))
//            {
//                SetPattern((string)token);
//            }
//        }

//        protected void ProcessEnum(JObject schema)
//        {
//            if (schema["enum"] != null)
//            {
//                var enums = (from val in (JArray)schema["enum"] select (string)val);
//                var toDescription = new Dictionary<string, DescribeAttribute>();
//                var toTerms = new Dictionary<string, string[]>();
//                var toMaxPhrase = new Dictionary<string, int>();
//                JToken values;
//                if (schema.TryGetValue("Values", out values))
//                {
//                    foreach (JProperty prop in values.Children())
//                    {
//                        var key = prop.Name;
//                        if (!enums.Contains(key))
//                        {
//                            throw new ArgumentException($"{key} is not in enumeration.");
//                        }
//                        var desc = (JObject)prop.Value;
//                        JToken description;
//                        if (desc.TryGetValue("Describe", out description))
//                        {
//                            toDescription.Add(key, ProcessDescription(desc, Language.CamelCase(key)));
//                        }
//                        JToken terms;
//                        if (desc.TryGetValue("Terms", out terms))
//                        {
//                            toTerms.Add(key, terms.ToObject<string[]>());
//                        }
//                        JToken maxPhrase;
//                        if (desc.TryGetValue("MaxPhrase", out maxPhrase))
//                        {
//                            toMaxPhrase.Add(key, (int)maxPhrase);
//                        }
//                    }
//                }
//                foreach (var key in enums)
//                {
//                    DescribeAttribute description;
//                    if (!toDescription.TryGetValue(key, out description))
//                    {
//                        description = new DescribeAttribute(Language.CamelCase(key));
//                    }
//                    AddDescription(key, description);

//                    string[] terms;
//                    int maxPhrase;
//                    if (!toTerms.TryGetValue(key, out terms))
//                    {
//                        terms = Language.GenerateTerms(description.Description, 3);
//                    }
//                    else if (toMaxPhrase.TryGetValue(key, out maxPhrase))
//                    {
//                        terms = (from seed in terms
//                                 from gen in Language.GenerateTerms(seed, maxPhrase)
//                                 select gen).ToArray<string>();
//                    }
//                    AddTerms(key, terms);
//                }
//            }
//        }

//        protected TemplateBaseAttribute ProcessTemplate(JToken template, TemplateBaseAttribute attribute)
//        {
//            if (template["Patterns"] != null)
//            {
//                attribute.Patterns = template["Patterns"].ToObject<string[]>();
//            }
//            attribute.AllowDefault = ProcessEnum<BoolDefault>(template, "AllowDefault");
//            attribute.ChoiceCase = ProcessEnum<CaseNormalization>(template, "ChoiceCase");
//            attribute.ChoiceFormat = (string)template["ChoiceFormat"];
//            attribute.ChoiceLastSeparator = (string)template["ChoiceLastSeparator"];
//            attribute.ChoiceParens = ProcessEnum<BoolDefault>(template, "ChoiceParens");
//            attribute.ChoiceSeparator = (string)template["ChoiceSeparator"];
//            attribute.ChoiceStyle = ProcessEnum<ChoiceStyleOptions>(template, "ChoiceStyle");
//            attribute.Feedback = ProcessEnum<FeedbackOptions>(template, "Feedback");
//            attribute.FieldCase = ProcessEnum<CaseNormalization>(template, "FieldCase");
//            attribute.LastSeparator = (string)template["LastSeparator"];
//            attribute.Separator = (string)template["Separator"];
//            attribute.ValueCase = ProcessEnum<CaseNormalization>(template, "ValueCase");
//            return attribute;
//        }

//        protected DescribeAttribute ProcessDescription(JObject schema, string defaultDesc)
//        {
//            // Simple string or object
//            // {Description=, Image=, Title=, SubTitle=}
//            var desc = new DescribeAttribute();
//            JToken jdesc;
//            if (schema.TryGetValue("Describe", out jdesc))
//            {
//                if (jdesc.Type == JTokenType.String)
//                {
//                    desc.Description = jdesc.Value<string>();
//                }
//                else
//                {
//                    var jdescription = jdesc["Description"];
//                    if (jdescription != null)
//                    {
//                        desc.Description = jdescription.Value<string>();
//                    }
//                    else
//                    {
//                        desc.Description = defaultDesc;
//                    }

//                    var jimage = jdesc["Image"];
//                    if (jimage != null)
//                    {
//                        desc.Image = jimage.Value<string>();
//                    }

//                    var jtitle = jdesc["Title"];
//                    if (jtitle != null)
//                    {
//                        desc.Title = jtitle.Value<string>();
//                    }

//                    var jsubTitle = jdesc["SubTitle"];
//                    if (jsubTitle != null)
//                    {
//                        desc.SubTitle = jsubTitle.Value<string>();
//                    }
//                }
//            }
//            else
//            {
//                desc.Description = defaultDesc;
//            }
//            return desc;
//        }

//        protected T ProcessEnum<T>(JToken template, string name)
//        {
//            T result = default(T);
//            var value = template[name];
//            if (value != null)
//            {
//                result = (T)Enum.Parse(typeof(T), (string)value);
//            }
//            return result;
//        }


//        internal static bool IsType(JObject schema, string type)
//        {
//            bool isType = false;
//            var jtype = schema["type"];
//            if (jtype != null)
//            {
//                if (jtype is JArray)
//                {
//                    isType = jtype.Values().Contains(type);
//                }
//                else
//                {
//                    isType = (string)jtype == type;
//                }
//            }
//            return isType;
//        }

//        internal static bool IsPrimitiveType(JObject schema)
//        {
//            var isPrimitive = schema["enum"] != null && schema["enum"].Any();
//            if (!isPrimitive)
//            {
//                isPrimitive =
//                    IsType(schema, "boolean")
//                    || IsType(schema, "integer")
//                    || IsType(schema, "number")
//                    || IsType(schema, "string")
//                    || (schema["DateTime"] != null && (bool)schema["DateTime"]);
//            }
//            return isPrimitive;
//        }

//        internal static JObject ElementSchema(JObject schema)
//        {
//            JObject result = schema;
//            if (IsType(schema, "array"))
//            {
//                var items = schema["items"];
//                if (items is JArray)
//                {
//                    result = (JObject)((JArray)items).First();
//                }
//                else
//                {
//                    result = (JObject)items;
//                }
//            }
//            return result;
//        }

//        protected FormBuilderJson _builder;
//    }
//}
