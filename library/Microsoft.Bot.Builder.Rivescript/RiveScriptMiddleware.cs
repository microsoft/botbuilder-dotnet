using RiveScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Rivescript
{
    public class RiveScriptOptions
    {
        public bool Utf8 { get; set; } = false;
        public bool Debug { get; set; } = false;
        public bool Strict { get; set; } = false;
    }

    public class RivescriptMiddleware : IMiddleware, IReceiveActivity, IObjectHandler
    {
        private readonly RiveScript.RiveScript _engine;
        public const string RivescriptState = "rivescript";

        public RivescriptMiddleware(string path) : this(path, new RiveScriptOptions())
        {
        }
        public RivescriptMiddleware(string path, RiveScriptOptions options)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            _engine = CreateRivescript(path.Trim(), options);                         
        }

        public string onCall(string name, RiveScript.RiveScript rs, string[] args)
        {
            var methodInfo = this.GetType().GetMethod(name);
            if (methodInfo != null)
                // call it
                return methodInfo.Invoke(this, new object[] { _engine, args })?.ToString();

            return null;
        }

        public bool onLoad(string name, string[] code)
        {
            return true;
        }

        public static IDictionary<string,string> StateDictionary(BotContext context)
        {
            BotAssert.AssertStorage(context); 

            IDictionary<string, string> state;
            if (context.State.User[RivescriptState] == null)
            {
                state = new Dictionary<string, string>();
                context.State.User[RivescriptState] = state;
            }
            else
            {
                state = context.State.User[RivescriptState];
            }
             
            return state; 
        }

        public async Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            IDictionary<string, string> userVars;
            userVars = context.State.User[RivescriptState] ?? new Dictionary<string, string>();
            _engine.setUservars(context.Request.From.Id, userVars);

            var reply = _engine.reply(context.Request.From.Id, context.Request.Text);

            IDictionary<string, string> stateAfterReply = _engine.getUserVars(context.Request.From.Id);
            context.State.User[RivescriptState] = stateAfterReply;
            context.Reply(reply);

            return new ReceiveResponse(false); 
        }
        
        private RiveScript.RiveScript CreateRivescript(string path, RiveScriptOptions options)
        {
            RiveScript.RiveScript engine = new RiveScript.RiveScript(options.Debug, options.Utf8, options.Strict);

            // set ourselves as a "script language" so that we can do the glue via reflection
            engine.setHandler("dialog", this);

            // only define methods from child class
            var ignoreMethods = new List<string>(typeof(RivescriptMiddleware).GetMethods().Select(m => m.Name));

            // build method bindings using the "dialog" language
            StringBuilder sb = new StringBuilder();
            foreach (var method in this.GetType().GetMethods())
            {
                if (!ignoreMethods.Contains(method.Name))
                {
                    // this is  "dialog" language method definition.  Turns out that's all we need
                    sb.AppendLine($"> object {method.Name} dialog");
                    sb.AppendLine($"< object\n");
                }
            }
            // load method bindings
            engine.stream(sb.ToString());

            // load referred path
            if (Directory.Exists(path))
                engine.loadDirectory(path);
            else
                engine.loadFile(path);

            // sort 
            engine.sortReplies();

            return engine;
        }
    }
}