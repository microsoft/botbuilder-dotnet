#if FORMFLOW_JSON
// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Json
{
    // No need to document overrides of interface methods
#pragma warning disable CS1591

    #region Documentation
    /// <summary>Build a form by specifying messages, fields and confirmations through JSON Schema or programatically.</summary>
    /// <remarks>
    /// Define  a form via [JSON Schema](http://json-schema.org/latest/json-schema-validation.html)
    /// with optional additional annotations that correspond to the attributes provided for C#.  
    /// %FormFlow makes use of a number of standard [JSON Schema](http://json-schema.org/latest/json-schema-validation.html) keywords include:
    /// * `type` -- Defines the fields type.
    /// * `enum` -- Defines the possible field values.
    /// * `minimum` -- Defines the minimum allowed value as described in <see cref="NumericAttribute"/>.
    /// * `maximum` -- Defines the maximum allowed value as described in <see cref="NumericAttribute"/>.
    /// * `required` -- Defines what fields are required.
    /// * `pattern` -- For string fields will be used to validate the entered pattern as described in <see cref="PatternAttribute"/>.
    /// 
    /// Templates and prompts use the same vocabulary as <see cref="TemplateAttribute"/> and <see cref="PromptAttribute"/>.  
    /// The property names are the same and the values are the same as those in the underlying C# enumeration.  
    /// For example to define a template to override the <see cref="TemplateUsage.NotUnderstood"/> template
    /// and specify a TemplateBaseAttribute.ChoiceStyle, you would put this in your schema: 
    /// ~~~
    /// "Templates":{ "NotUnderstood": { Patterns: ["I don't get it"], "ChoiceStyle":"Auto"}}
    /// ~~~
    /// 
    /// %Extensions defined at the root fo the schema
    /// * `OnCompletion: script` -- C# script with arguments (<see cref="IDialogContext"/> context, JObject state) for completing form.
    /// * `References: [assemblyReference, ...]` -- Define references to include in scripts.  Paths should be absolute, or relative to the current directory.  By default Microsoft.Bot.Builder.Classic.dll is included.
    /// * `Imports: [import, ...]` -- Define imports to include in scripts with usings. By default these namespaces are included: Microsoft.Bot.Builder.Classic, Microsoft.Bot.Builder.Classic.Dialogs, Microsoft.Bot.Builder.Classic.FormFlow, Microsoft.Bot.Builder.Classic.FormFlow.Advanced, System.Collections.Generic, System.Linq
    /// 
    /// %Extensions defined at the root of a schema or as a peer of the "type" property.  
    /// * `Templates:{TemplateUsage: { Patterns:[string, ...], &lt;args&gt; }, ...}` -- Define templates.
    /// * `Prompt: { Patterns:[string, ...] &lt;args&gt;}` -- Define a prompt.
    /// 
    /// %Extensions that are found in a property description as peers to the "type" property of a JSON Schema.
    /// * `DateTime:bool` -- Marks a field as being a DateTime field.
    /// * `Describe:string` -- Description of a field as described in <see cref="DescribeAttribute"/>.
    /// * `Terms:[string,...]` -- Regular expressions for matching a field value as described in <see cref="TermsAttribute"/>.
    /// * `MaxPhrase:int` -- This will run your terms through <see cref="Language.GenerateTerms(string, int)"/> to expand them.
    /// * `Values:{ string: {Describe:string, Terms:[string, ...], MaxPhrase}, ...}` -- The string must be found in the types "enum" and this allows you to override the automatically generated descriptions and terms.  If MaxPhrase is specified the terms are passed through <see cref="Language.GenerateTerms(string, int)"/>.
    /// * `Active:script` -- C# script with arguments (JObject state)->bool to test to see if field/message/confirm is active.
    /// * `Validate:script` -- C# script with arguments (JObject state, object value)->ValidateResult for validating a field value.
    /// * `Define:script` -- C# script with arguments (JObject state, Field&lt;JObject&gt; field) for dynamically defining a field.  
    /// * `Before:[confirm|message, ...]` -- Messages or confirmations before the containing field.
    /// * `After:[confirm|message, ...]` -- Messages or confirmations after the containing field.
    /// * `{Confirm:script|[string, ...], ...templateArgs}` -- With Before/After define a confirmation through either C# script with argument (JObject state) or through a set of patterns that will be randomly selected with optional template arguments.
    /// * `{Message:script|[string, ...] ...templateArgs}` -- With Before/After define a message through either C# script with argument (JObject state) or through a set of patterns that will be randomly selected with optional template arguments.
    /// * `Dependencies`:[string, ...]` -- Fields that this field, message or confirm depends on.
    /// 
    /// Scripts can be any C# code you would find in a method body.  You can add references through "References" and using through "Imports". Special global variables include:
    /// * `choice` -- internal dispatch for script to execute.
    /// * `state` -- JObject form state bound for all scripts.
    /// * `ifield` -- <see cref="IField{JObject}"/> to allow reasoning over the current field for all scripts except %Message/Confirm prompt builders.
    /// * `value` -- object value to be validated for Validate.
    /// * `field` -- <see cref="Field{JObject}"/> to allow dynamically updating a field in Define.
    /// * `context` -- <see cref="IDialogContext"/> context to allow posting results in OnCompletion.
    /// 
    /// %Fields defined through this class have the same ability to extend or override the definitions
    /// programatically as any other field.  They can also be localized in the same way.
    /// </remarks>
    #endregion
    public sealed class FormBuilderJson : FormBuilderBase<JObject>
    {
        /// <summary>
        /// Create a JSON form builder.
        /// </summary>
        /// <param name="schema">JSON Schema that defines form.</param>
        public FormBuilderJson(JObject schema)
        {
            _schema = schema;
            ProcessOptions();
            ProcessOnCompletion();
        }

        public override IForm<JObject> Build(Assembly resourceAssembly = null, string resourceName = null)
        {
            if (!_form.Fields.Any())
            {
                // No fieldss means add default field and confirmation
                AddRemainingFields();
                Confirm(new PromptAttribute(Configuration.Template(TemplateUsage.Confirmation)));
            }
            // Build all code into a single assembly and cache because assemblies have no GC.
            var builder = new StringBuilder("switch (choice) {");
            int choice = 1;
            var entries = new List<CallScript>();
            lock (_scripts)
            {
                foreach (var entry in _scripts)
                {
                    if (entry.Value.Script == null)
                    {
                        entry.Value.Choice = choice++;
                        builder.AppendLine();
                        builder.Append($"case {entry.Value.Choice}: {{{entry.Key}}}; break;");
                        entries.Add(entry.Value);
                    }
                }
                if (entries.Any())
                {
                    // Define does not need to return a result.
                    builder.AppendLine();
                    builder.AppendLine("}");
                    builder.Append("return null;");
                    var fun = Compile<ScriptGlobals, object>(builder.ToString());
                    foreach (var entry in entries)
                    {
                        entry.Script = fun;
                    }
                }
            }
            return base.Build(resourceAssembly, resourceName);
        }

        public override IFormBuilder<JObject> Field(string name, ActiveDelegate<JObject> active = null, ValidateAsyncDelegate<JObject> validate = null)
        {
            var field = new FieldJson(this, name);
            field.SetActive(active);
            field.SetValidate(validate);
            AddSteps(field.Before);
            Field(field);
            AddSteps(field.After);
            return this;
        }

        public override IFormBuilder<JObject> Field(string name, string prompt, ActiveDelegate<JObject> active = null, ValidateAsyncDelegate<JObject> validate = null)
        {
            return Field(name, new PromptAttribute(prompt), active, validate);
        }

        public override IFormBuilder<JObject> Field(string name, PromptAttribute prompt, ActiveDelegate<JObject> active = null, ValidateAsyncDelegate<JObject> validate = null)
        {
            var field = new FieldJson(this, name);
            field.SetPrompt(prompt);
            if (active != null)
            {
                field.SetActive(active);
            }
            if (validate != null)
            {
                field.SetValidate(validate);
            }
            return Field(field);
        }

        public override IFormBuilder<JObject> AddRemainingFields(IEnumerable<string> exclude = null)
        {
            var exclusions = (exclude == null ? Array.Empty<string>() : exclude.ToArray());
            var fields = new List<string>();
            Fields(_schema, null, fields);
            foreach (var field in fields)
            {
                if (!exclusions.Contains(field) && !HasField(field))
                {
                    Field(field);
                }
            }
            return this;
        }

        #region Class specific methods
        public JObject Schema
        {
            get { return _schema; }
        }

        internal MessageDelegate<JObject> MessageScript(string script)
        {
            return script != null ? new MessageDelegate<JObject>(AddScript(null, script).MessageScript) : null;
        }

        internal ActiveDelegate<JObject> ActiveScript(IField<JObject> field, string script)
        {
            return script != null ? new ActiveDelegate<JObject>(AddScript(field, script).ActiveScript) : null;
        }

        internal DefineAsyncDelegate<JObject> DefineScript(IField<JObject> field, string script)
        {
            return script != null ? new DefineAsyncDelegate<JObject>(AddScript(field, script).DefineScriptAsync) : null;
        }

        internal ValidateAsyncDelegate<JObject> ValidateScript(IField<JObject> field, string script)
        {
            return script != null ? new ValidateAsyncDelegate<JObject>(AddScript(field, script).ValidateScriptAsync) : null;
        }

        internal NextDelegate<JObject> NextScript(IField<JObject> field, string script)
        {
            return script != null ? new NextDelegate<JObject>(AddScript(field, script).NextScript) : null;
        }

        internal OnCompletionAsyncDelegate<JObject> OnCompletionScript(string script)
        {
            return script != null ? new OnCompletionAsyncDelegate<JObject>(AddScript(null, script).OnCompletionAsync) : null;
        }
        #endregion

        #region Implementation
        private void ProcessOptions()
        {
            JToken references;
            var assemblies = new List<string>() { "Microsoft.Bot.Builder.Classic.dll" };
            if (_schema.TryGetValue("References", out references))
            {
                foreach (JToken reference in references.Children())
                {
                    assemblies.Add((string)reference);
                }
            }
            JToken importsChildren;
            var imports = new List<string>();
            if (_schema.TryGetValue("Imports", out importsChildren))
            {
                foreach (JToken import in importsChildren.Children())
                {
                    imports.Add((string)import);
                }
            }
            var dir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            _options = CodeAnalysis.Scripting.ScriptOptions.Default
                .AddReferences((from assembly in assemblies select System.IO.Path.Combine(dir, assembly)).ToArray())
                .AddImports("Microsoft.Bot.Builder.Classic", "Microsoft.Bot.Builder.Classic.Dialogs",
                            "Microsoft.Bot.Builder.Classic.FormFlow", "Microsoft.Bot.Builder.Classic.FormFlow.Advanced",
                            "System.Collections.Generic", "System.Linq")
                .AddImports(imports.ToArray());
        }

        private void ProcessOnCompletion()
        {
            if (_schema["OnCompletion"] != null)
            {
                OnCompletion(OnCompletionScript((string)_schema["OnCompletion"]));
            }
        }

        private CallScript AddScript(IField<JObject> field, string script)
        {
            CallScript call;
            lock (_scripts)
            {
                if (!_scripts.TryGetValue(script, out call))
                {
                    call = new CallScript { Field = field };
                    _scripts[script] = call;
                }
            }
            return call;
        }

        // NOTE: Compiling code creates an assembly which cannot be GC whereas EvaluateAsync does not.
        private ScriptRunner<R> Compile<G, R>(string code)
        {
            try
            {
                var script = CSharpScript.Create<R>(code, _options, typeof(G));
                return script.CreateDelegate();
            }
            catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException ex)
            {
                throw CompileException(ex, code);
            }
        }

        private async Task<T> EvaluateAsync<T>(string code, object globals)
        {
            try
            {
                return await CSharpScript.EvaluateAsync<T>(code, _options, globals);
            }
            catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException ex)
            {
                throw CompileException(ex, code);
            }
        }

        private Exception CompileException(CompilationErrorException ex, string code)
        {
            Exception result = ex;
            var match = System.Text.RegularExpressions.Regex.Match(ex.Message, @"\(\s*(?<line>\d+)\s*,\s*(?<column>\d+)\s*\)\s*:\s*(?<message>.*)");
            if (match.Success)
            {
                var lineNumber = int.Parse(match.Groups["line"].Value) - 1;
                var column = int.Parse(match.Groups["column"].Value) - 1;
                var line = code.Split('\n')[lineNumber];
                var minCol = Math.Max(0, column - 20);
                var maxCol = Math.Min(line.Length, column + 20);
                var msg = line.Substring(minCol, column - minCol) + "^" + line.Substring(column, maxCol - column);
                result = new ArgumentException(match.Groups["message"].Value + ": " + msg);
            }
            return result;
        }

        private void AddSteps(IEnumerable<FieldJson.MessageOrConfirmation> steps)
        {
            foreach (var step in steps)
            {
                var active = ActiveScript(null, step.ActiveScript);
                if (step.IsMessage)
                {
                    if (step.MessageScript != null)
                    {
                        Message(MessageScript(step.MessageScript), active, step.Dependencies);
                    }
                    else
                    {
                        Message(step.Prompt, active, step.Dependencies);
                    }
                }
                else
                {
                    if (step.MessageScript != null)
                    {
                        Confirm(MessageScript(step.MessageScript), active, step.Dependencies);
                    }
                    else
                    {
                        Confirm(step.Prompt, active, step.Dependencies);
                    }
                }
            }
        }

        private void Fields(JObject schema, string prefix, IList<string> fields)
        {
            if (schema["properties"] != null)
            {
                foreach (JProperty property in schema["properties"])
                {
                    var path = (prefix == null ? property.Name : $"{prefix}.{property.Name}");
                    var childSchema = (JObject)property.Value;
                    var eltSchema = FieldJson.ElementSchema(childSchema);
                    if (FieldJson.IsPrimitiveType(eltSchema))
                    {
                        fields.Add(path);
                    }
                    else
                    {
                        Fields(childSchema, path, fields);
                    }
                }
            }
        }

        private void FieldPaths(Type type, string path, List<string> paths)
        {
            var newPath = (path == string.Empty ? path : path + ".");
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => !f.IsDefined(typeof(IgnoreFieldAttribute))))
            {
                TypePaths(field.FieldType, newPath + field.Name, paths);
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanRead && property.CanWrite)
                {
                    TypePaths(property.PropertyType, newPath + property.Name, paths);
                }
            }
        }

        private void TypePaths(Type type, string path, List<string> paths)
        {
            if (type.IsClass)
            {
                if (type == typeof(string))
                {
                    paths.Add(path);
                }
                else if (type.IsIEnumerable())
                {
                    var elt = type.GetGenericElementType();
                    if (elt.IsEnum)
                    {
                        paths.Add(path);
                    }
                    else
                    {
                        // TODO: What to do about enumerations of things other than enums?
                    }
                }
                else
                {
                    FieldPaths(type, path, paths);
                }
            }
            else if (type.IsEnum)
            {
                paths.Add(path);
            }
            else if (type == typeof(bool))
            {
                paths.Add(path);
            }
            else if (type.IsIntegral())
            {
                paths.Add(path);
            }
            else if (type.IsDouble())
            {
                paths.Add(path);
            }
            else if (type.IsNullable() && type.IsValueType)
            {
                paths.Add(path);
            }
            else if (type == typeof(DateTime))
            {
                paths.Add(path);
            }
        }

        private delegate Task<object> CallAsyncDelegate(ScriptGlobals globals);
        private class CallScript
        {
            public int Choice;
            public ScriptRunner<object> Script;
            public IField<JObject> Field;

            public async Task<PromptAttribute> MessageScript(JObject state)
            {
                return (PromptAttribute)await Script(new ScriptGlobals { choice = Choice, state = state, ifield = Field });
            }

            public bool ActiveScript(JObject state)
            {
                return (bool)Script(new ScriptGlobals { choice = Choice, state = state, ifield = Field }).Result;
            }

            public async Task<ValidateResult> ValidateScriptAsync(JObject state, object value)
            {
                return (ValidateResult)await Script(new ScriptGlobals { choice = Choice, state = state, value = value, ifield = Field });
            }

            public async Task<bool> DefineScriptAsync(JObject state, Field<JObject> field)
            {
                return (bool)await Script(new ScriptGlobals { choice = Choice, state = state, field = field, ifield = Field });
            }

            public NextStep NextScript(object value, JObject state)
            {
                return (NextStep)Script(new ScriptGlobals { choice = Choice, value = value, state = state, ifield = Field }).Result;
            }

            public async Task OnCompletionAsync(IDialogContext context, JObject state)
            {
                await Script(new ScriptGlobals { choice = Choice, context = context, state = state, ifield = Field });
            }
        }

        private readonly JObject _schema;
        private ScriptOptions _options;
        private static Dictionary<string, CallScript> _scripts = new Dictionary<string, CallScript>();
        #endregion
    }
}

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    /// <summary>
    /// Global values to pass into scripts defined using <see cref="Microsoft.Bot.Builder.Classic.FormFlow.Json.FormBuilderJson"/>.
    /// </summary>
    public class ScriptGlobals
    {
        /// <summary>
        /// Which script to execute.
        /// </summary>
        public int choice;

        /// <summary>
        /// Current form state.
        /// </summary>
        public JObject state;

        /// <summary>
        /// Value to be validated.
        /// </summary>
        public object value;

        /// <summary>
        /// Current field if any.
        /// </summary>
        public IField<JObject> ifield;

        /// <summary>
        /// Field to be dynamically defined.
        /// </summary>
        public Field<JObject> field;

        /// <summary>
        /// Dialog context for OnCompletionAsync handlers.
        /// </summary>
        public IDialogContext context;
    }
}
#endif