using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines the shape of the state object returned by calling DialogContext.State.ToJson()
    /// </summary>
    public class DialogContextVisibleState
    {
        [JsonProperty(PropertyName = "user")]
        public StateMap User { get; set; }

        [JsonProperty(PropertyName = "conversation")]
        public StateMap Conversation { get; set; }

        [JsonProperty(PropertyName = "dialog")]
        public StateMap Dialog { get; set; }

        [JsonProperty(PropertyName = "entities")]
        public StateMap Entities { get; set; }
    }

    public class DialogContextState
    {
        private const string TurnEntities = "turn_entities";
        private readonly DialogContext dialogContext;

        [JsonProperty(PropertyName = "user")]
        public StateMap User { get; set; }

        [JsonProperty(PropertyName = "conversation")]
        public StateMap Conversation { get; set; }

        [JsonProperty(PropertyName = "dialog")]
        public StateMap Dialog
        {
            get
            {
                var instance = dialogContext.ActiveDialog;

                if (instance == null)
                {
                    if (dialogContext.ParentContext != null)
                    {
                        instance = dialogContext.ParentContext.ActiveDialog;
                    }
                    else
                    {
                        throw new Exception("DialogContext.State.Dialog: no active or parent dialog instance.");
                    }
                }

                return (StateMap)instance.State;
            }

            set
            {
                var instance = dialogContext.ActiveDialog;

                if (instance == null)
                {
                    if (dialogContext.ParentContext != null)
                    {
                        instance = dialogContext.ParentContext.ActiveDialog;
                    }
                    else
                    {
                        throw new Exception("DialogContext.State.Dialog: no active or parent dialog instance.");
                    }
                }

                instance.State = value;

            }
        }

        [JsonProperty(PropertyName = "entities")]
        public StateMap Entities
        {
            get
            {
                var entities = dialogContext.Context.TurnState.Get<object>(TurnEntities);
                if (entities == null)
                {
                    entities = new StateMap();
                    dialogContext.Context.TurnState.Add(TurnEntities, entities);
                }

                return entities as StateMap;
            }
        }

        public DialogContextState(DialogContext dc, StateMap userState, StateMap conversationState)
        {
            this.dialogContext = dc ?? throw new ArgumentNullException(nameof(dc));
            this.User = userState;
            this.Conversation = conversationState;
        }

        public DialogContextVisibleState ToJson()
        {
            var instance = dialogContext.ActiveDialog;

            if (instance == null)
            {
                if (dialogContext.ParentContext != null)
                {
                    instance = dialogContext.ParentContext.ActiveDialog;
                }
            }

            return new DialogContextVisibleState()
            {
                Conversation = this.Conversation,
                User = this.User,
                Dialog = (StateMap)instance?.State,
            };
        }

        public IEnumerable<JToken> Query(string pathExpression)
        {
            JToken json = JToken.FromObject(this);

            return json.SelectTokens(pathExpression);
        }

        public T GetValue<T>(string pathExpression, T defaultValue = default(T))
        {
            return GetValue<T>(this, pathExpression, defaultValue);
        }

        public T GetValue<T>(object o, string pathExpression, T defaultValue = default(T))
        {
            var result = JToken.FromObject(o).SelectToken(pathExpression);

            if (result != null)
            {
                return result.ToObject<T>();
            }
            else
            {
                return defaultValue;
            }
        }

        public void SetValue(string pathExpression, object value)
        {
            // Obtain JToken from the current state
            var thisJToken = JToken.Parse(JsonConvert.SerializeObject(this));

            // JsonPath replace
            var resultToken = thisJToken.ReplacePath(pathExpression, value);

            // Rehydrate and copy state properties to currentobject
            var resultContextState = resultToken.ToObject<DialogContextVisibleState>();
            foreach (var kv in resultContextState.User)
            {
                this.User[kv.Key] = kv.Value;
            }

            foreach (var kv in resultContextState.Conversation)
            {
                if (!this.Conversation.ContainsKey(kv.Key))
                {
                    this.Conversation.Add(kv.Key, kv.Value);
                }
            }

            foreach (var kv in resultContextState.Dialog)
            {
                if (!this.Dialog.ContainsKey(kv.Key))
                {
                    this.Dialog.Add(kv.Key, kv.Value);
                }
            }

            // this.User = resultContextState.User;
            // this.Conversation = resultContextState.Conversation;
            // this.Dialog = resultContextState.Dialog;
        }
    }

    public static class JsonExtensions
    {
        public static JToken ReplacePath(this JToken root, string path, object newValue)
        {
            if (root == null || path == null)
            {
                throw new ArgumentNullException();
            }

            var tokens = root.SelectTokens(path).ToList();

            // If the json path does not exist
            if (tokens?.Count == 0)
            {
                string[] segments = path.Split('.');
                var current = root;

                for (int i = 0; i < segments.Length; i++)
                {
                    var segment = segments[i];
                    if (current.Type == JTokenType.Object)
                    {
                        var currentObject = current as JObject;

                        if (!currentObject.Children().Any(c => c.Type == JTokenType.Property && (c as JProperty).Name == segment))
                        {
                            currentObject.Add(segment, i == segments.Length - 1 ? JToken.FromObject(newValue) : new JObject());
                        }

                        current = currentObject[segment];
                    }
                }
            }
            else
            {
                foreach (var value in tokens)
                {
                    if (value == root)
                    {
                        root = JToken.FromObject(newValue);
                    }
                    else
                    {
                        value.Replace(JToken.FromObject(newValue));
                    }
                }
            }

            return root;
        }

        public static string ReplacePath(string jsonString, string path, object newValue)
        {
            return JToken.Parse(jsonString).ReplacePath(path, newValue).ToString();
        }
    }
}
