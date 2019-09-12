using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jurassic;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class JavascriptAction : Dialog
    {
        private ScriptEngine scriptEngine;
        private string script;

        [JsonConstructor]
        public JavascriptAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.scriptEngine = new ScriptEngine();

            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets javascript bound to memory run function(user, conversation, dialog, turn).
        /// </summary>
        /// <example>
        /// example inline script:
        ///        if (user.age > 18)
        ///              return dialog.lastResult;
        ///          return null;
        /// Example file script.js:
        /// function doAction(user, conversation, dialog, turn) {
        ///    if (user.age)
        ///        return user.age* 7;
        ///    return 0;
        /// }.
        /// </example>
        /// <value>
        /// Javascript bound to memory run function(user, conversation, dialog, turn).
        /// </value>
        public string Script
        {
            get { return script; } set { LoadScript(value); }
        }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // map state into json
            dynamic payload = new JObject();
            payload.state = dc.State.GetMemorySnapshot();

            // payload.property = (this.Property != null) ? dc.GetValue<object>(this.Property) : null;
            string payloadJson = JsonConvert.SerializeObject(payload);
            var responseJson = scriptEngine.CallGlobalFunction<string>("callAction", payloadJson);

            if (!string.IsNullOrEmpty(responseJson))
            {
                dynamic response = JsonConvert.DeserializeObject(responseJson);
                dc.State.SetValue(ScopePath.USER, response.state.user);
                dc.State.SetValue(ScopePath.CONVERSATION, response.state.conversation);
                dc.State.SetValue(ScopePath.DIALOG, response.state.dialog);
                dc.State.SetValue(ScopePath.TURN, response.state.turn);
                return dc.EndDialogAsync((object)response.result, cancellationToken: cancellationToken);
            }

            return dc.EndDialogAsync(cancellationToken: cancellationToken);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({this.script.GetHashCode()})";
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

            // define the function
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(script);
            sb.AppendLine(@"function callAction(payloadJson) { 
	                var payload = JSON.parse(payloadJson);

                    // run script
	                payload.result = doAction(payload.state.user, 
                        payload.state.conversation, 
                        payload.state.dialog, 
                        payload.state.turn);

                    return JSON.stringify(payload, null, 4);
                }");

            scriptEngine.Evaluate(sb.ToString());
        }
    }
}
