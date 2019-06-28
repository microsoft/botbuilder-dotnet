using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines the shape of the state object returned by calling DialogContext.State.ToJson()
    /// </summary>
    public class DialogContextVisibleState
    {
        [JsonProperty(PropertyName = "user")]
        public Dictionary<string, object> User { get; set; }

        [JsonProperty(PropertyName = "conversation")]
        public Dictionary<string, object> Conversation { get; set; }

        [JsonProperty(PropertyName = "dialog")]
        public Dictionary<string, object> Dialog { get; set; }
    }

    public class DialogContextState : IDictionary<string, object>
    {
        private static JsonSerializerSettings expressionCaseSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly DialogContext dialogContext;

        public DialogContextState(DialogContext dc, Dictionary<string, object> settings, Dictionary<string, object> userState, Dictionary<string, object> conversationState, Dictionary<string, object> turnState)
        {
            this.dialogContext = dc ?? throw new ArgumentNullException(nameof(dc));
            this.Settings = settings;
            this.User = userState;
            this.Conversation = conversationState;
            this.Turn = turnState;
        }

        /// <summary>
        /// Common state properties paths.
        /// </summary>
        public const string DIALOG_OPTIONS = "dialog.options";
        public const string DIALOG_VALUE = "dialog.value";

        public const string TURN_ACTIVITY = "turn.activity";
        public const string TURN_RECOGNIZED = "turn.recognized";
        public const string TURN_TOPINTENT = "turn.recognized.intent";
        public const string TURN_TOPSCORE = "turn.recognized.score";
        public const string TURN_STEPCOUNT = "turn.stepCount";
        public const string TURN_DIALOGEVENT = "turn.dialogEvent";

        public const string STEP_OPTIONS_PROPERTY = "dialog.step.options";

        /// <summary>
        /// Gets or sets settings for the application.
        /// </summary>
        [JsonProperty(PropertyName = "settings")]
        public Dictionary<string, object> Settings { get; set; }

        /// <summary>
        /// Gets or sets state associated with the active user in the turn.
        /// </summary>
        [JsonProperty(PropertyName = "user")]
        public Dictionary<string, object> User { get; set; }

        /// <summary>
        /// Gets or sets state assocaited with the active conversation for the turn.
        /// </summary>
        [JsonProperty(PropertyName = "conversation")]
        public Dictionary<string, object> Conversation { get; set; }

        /// <summary>
        /// Gets or sets state associated with the active dialog for the turn.
        /// </summary>
        [JsonProperty(PropertyName = "dialog")]
        public Dictionary<string, object> Dialog
        {
            get
            {
                var instance = dialogContext.ActiveDialog;

                if (instance == null)
                {
                    if (dialogContext.Parent != null)
                    {
                        instance = dialogContext.Parent.ActiveDialog;
                    }
                    else
                    {
                        throw new Exception("DialogContext.State.Dialog: no active or parent dialog instance.");
                    }
                }

                return instance.State;
            }

            set
            {
                var instance = dialogContext.ActiveDialog;

                if (instance == null)
                {
                    if (dialogContext.Parent != null)
                    {
                        instance = dialogContext.Parent.ActiveDialog;
                    }
                    else
                    {
                        throw new Exception("DialogContext.State.Dialog: no active or parent dialog instance.");
                    }
                }

                instance.State = value;

            }
        }

        /// <summary>
        /// Gets access to the callstack of dialog state.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<object> CallStack
        {
            get
            {
                // get each state on the current stack.
                foreach (var instance in this.dialogContext.Stack)
                {
                    if (instance.State != null)
                    {
                        yield return instance.State;
                    }
                }

                // switch to parent stack and enumerate it's state objects
                if (this.dialogContext.Parent != null)
                {
                    foreach (var state in dialogContext.Parent.State.CallStack)
                    {
                        yield return state;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets state associated with the current turn only (this is non-persisted).
        /// </summary>
        [JsonProperty(PropertyName = "turn")]
        public Dictionary<string, object> Turn { get; set; }

        public ICollection<string> Keys => new[] { "user", "conversation", "dialog", "callstack", "turn", "settings" };

        public ICollection<object> Values => new object[] { User, Conversation, Dialog, CallStack, Turn };

        public int Count => 3;

        public bool IsReadOnly => true;

        public object this[string key]
        {
            get
            {
                if (TryGetValue(key, out object result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                System.Diagnostics.Trace.TraceError("DialogContextState doesn't support adding/changinge the base properties");
            }
        }

        public DialogContextVisibleState ToJson()
        {
            var instance = dialogContext.ActiveDialog;

            if (instance == null)
            {
                if (dialogContext.Parent != null)
                {
                    instance = dialogContext.Parent.ActiveDialog;
                }
            }

            return new DialogContextVisibleState()
            {
                Conversation = this.Conversation,
                User = this.User,
                Dialog = (Dictionary<string, object>)instance?.State,
            };
        }

        public IEnumerable<JToken> Query(string pathExpression)
        {
            JToken json = JToken.FromObject(this);

            return json.SelectTokens(pathExpression);
        }

        public string ResolvePathShortcut(string path)
        {
            path = path.Trim();
            if (path.Length == 0)
            {
                return path;
            }

            string name = path.Substring(1);

            switch (path[0])
            {
                case '#':
                    // #BookFlight == turn.recognized.intents.BookFlight
                    return $"turn.recognized.intents.{name}";

                case '@':
                    // @city == turn.recognized.entities.city
                    return $"turn.recognized.entities.{name}";

                case '$':
                    // $title == dialog.title
                    return $"dialog.{name}";

                case '%':
                    // %xxx == dialog.instance.xxx
                    return $"dialog.instance.{name}";

                case '^':
                    // ^xxx == dialog.options.xxx
                    return $"dialog.options.{name}";

                default:
                    return path;
            }
        }

        public T GetValue<T>(string pathExpression)
        {
            return ObjectPath.GetValue<T>(this, ResolvePathShortcut(pathExpression));
        }

        public T GetValue<T>(string pathExpression, T defaultVal)
        {
            if (ObjectPath.TryGetValue<T>(this, ResolvePathShortcut(pathExpression), out var val))
            {
                return val;
            }

            return defaultVal;
        }

        public bool TryGetValue<T>(string pathExpression, out T val)
        {
            return ObjectPath.TryGetValue(this, ResolvePathShortcut(pathExpression), out val);
        }

        public bool HasValue(string pathExpression)
        {
            return ObjectPath.HasValue(this, ResolvePathShortcut(pathExpression));
        }

        public void SetValue(string pathExpression, object value)
        {
            if (value is Task)
            {
                throw new Exception($"{pathExpression} = You can't pass an unresolved Task to SetValue");
            }

            // If the json path does not exist
            pathExpression = ResolvePathShortcut(pathExpression);
            var segments = pathExpression.Split('.').Select(segment => segment.ToLower()).ToArray();
            dynamic current = this;

            for (var i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                if (current is IDictionary<string, object> curDict)
                {
                    if (!curDict.TryGetValue(segment, out var segVal) || segVal == null)
                    {
                        curDict[segment] = new Dictionary<string, object>();
                    }

                    current = curDict[segment];
                }
            }

            if (value is JToken || value is JObject || value is JArray)
            {
                current[segments.Last()] = (JToken)value;
            }
            else if (value == null)
            {
                current[segments.Last()] = null;
            }
            else if (value is string || value is byte || value is bool ||
                    value is Int16 || value is Int32 || value is Int64 ||
                    value is UInt16 || value is UInt32 || value is UInt64 ||
                    value is Decimal || value is float || value is double)
            {
                current[segments.Last()] = JValue.FromObject(value);
            }
            else
            {
                current[segments.Last()] = (JToken)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, expressionCaseSettings));
            }
        }

        public void RemoveValue(string pathExpression)
        {
            pathExpression = ResolvePathShortcut(pathExpression);

            // If the json path does not exist
            string[] segments = pathExpression.Split('.').Select(segment => segment.ToLower()).ToArray();
            dynamic current = this;

            var deleted = false;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                if (current is IDictionary<string, object> curDict)
                {
                    if (!curDict.TryGetValue(segment, out var segVal) || segVal == null)
                    {
                        deleted = true;
                        break;
                    }

                    current = curDict[segment];
                }
            }

            if (!deleted)
            {
                current.Remove(segments.Last());
            }
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return this.Keys.Contains(key.ToLower());
        }

        public bool Remove(string pathExpression)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            value = null;
            switch (key.ToLower())
            {
                case "user":
                    value = this.User;
                    return true;
                case "conversation":
                    value = this.Conversation;
                    return true;
                case "dialog":
                    value = this.Dialog;
                    return true;
                case "callstack":
                    value = this.CallStack;
                    return true;
                case "settings":
                    value = this.Settings;
                    return true;
                case "turn":
                    value = this.Turn;
                    return true;
            }

            return false;
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            yield return new KeyValuePair<string, object>("user", this.User);
            yield return new KeyValuePair<string, object>("conversation", this.Conversation);
            yield return new KeyValuePair<string, object>("dialog", this.Dialog);
            yield return new KeyValuePair<string, object>("callstack", this.CallStack);
            yield return new KeyValuePair<string, object>("settings", this.Settings);
            yield return new KeyValuePair<string, object>("turn", this.Turn);
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

    }
}
