using Jurassic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using System.Reflection;
using System.Dynamic;
using System.Linq;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class CSharpStep : DialogCommand
    {
        private static List<MetadataReference> refs = new List<MetadataReference>{
                    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location)};

        private string script;
        private Script<object> compiledScript;

        /// <summary>
        /// Javascript bound to memory run function(user, conversation, dialog, turn)
        /// </summary>
        /// <example>
        /// if (user.age > 18)
        ///     return dialog.lastResult;
        /// return null;
        /// </example>
        public string Script { get { return script; } set { LoadScript(value); } }

        [JsonConstructor]
        public CSharpStep([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Exception error = null;
            var result = await compiledScript.RunAsync((object)dc.State, (exception) =>
            {
                error = exception;
                return true;
            });
            if (error != null)
            {
                await dc.Context.SendActivityAsync(error.Message);
                return await dc.EndDialogAsync().ConfigureAwait(false);
            }
            return await dc.EndDialogAsync(result.ReturnValue, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{nameof(CSharpStep)}({this.script.GetHashCode()})";
        }

        private void LoadScript(string value)
        {
            if (File.Exists(value))
            {
                this.script = File.ReadAllText(value);
            }
            else
            {
                this.script = value;
            }

            this.compiledScript = CSharpScript.Create(this.script,
                options: ScriptOptions.Default.AddReferences(refs)
                            .AddImports("System.Dynamic"),
                globalsType: typeof(DialogContextState));
        }
    }
}
