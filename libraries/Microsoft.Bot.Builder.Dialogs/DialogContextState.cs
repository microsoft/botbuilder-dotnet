using System;
using System.Collections;
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
        public Dictionary<string, object> User { get; set; }

        [JsonProperty(PropertyName = "conversation")]
        public Dictionary<string, object> Conversation { get; set; }

        [JsonProperty(PropertyName = "dialog")]
        public Dictionary<string, object> Dialog { get; set; }

        [JsonProperty(PropertyName = "entities")]
        public Dictionary<string, object> Entities { get; set; }

        [JsonProperty(PropertyName = "turn")]
        public Dictionary<string, object> Turn { get; set; }
    }

    public class DialogContextState : IDictionary<string, object>
    {
        private const string TurnEntities = "turn_entities";
        private readonly DialogContext dialogContext;

        public DialogContextState(DialogContext dc, Dictionary<string, object> userState, Dictionary<string, object> conversationState, Dictionary<string, object> turnState)
        {
            this.dialogContext = dc ?? throw new ArgumentNullException(nameof(dc));
            this.User = userState;
            this.Conversation = conversationState;
            this.Turn = turnState;
        }

        [JsonProperty(PropertyName = "user")]
        public Dictionary<string, object> User { get; set; }

        [JsonProperty(PropertyName = "conversation")]
        public Dictionary<string, object> Conversation { get; set; }

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

                return (Dictionary<string, object>)instance.State;
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

        [JsonProperty(PropertyName = "entities")]
        public Dictionary<string, object> Entities
        {
            get
            {
                var entities = dialogContext.Context.TurnState.Get<object>(TurnEntities);
                if (entities == null)
                {
                    entities = new Dictionary<string, object>();
                    dialogContext.Context.TurnState.Add(TurnEntities, entities);
                }

                return entities as Dictionary<string, object>;
            }
        }

        [JsonProperty(PropertyName = "turn")]
        public Dictionary<string, object> Turn { get; set; }

        public ICollection<string> Keys => new[] { "user", "conversation", "dialog", "turn", "entities" };

        public ICollection<object> Values => new[] { User, Conversation, Dialog, Turn, Entities };

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
            // If the json path does not exist
            string[] segments = pathExpression.Split('.');
            dynamic current = this;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                if (current is IDictionary<string, object> curDict)
                {
                    if (!curDict.ContainsKey(segment))
                    {
                        curDict[segment] = new Dictionary<string, object>();
                    }
                    current = curDict[segment];
                }
            }

            current[segments.Last()] = value;
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return this.Keys.Contains(key.ToLower());
        }

        public bool Remove(string key)
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
                case "turn":
                    value = this.Turn;
                    return true;
                case "entities":
                    value = this.Entities;
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
            yield return new KeyValuePair<string, object>("turn", this.Turn);
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

    }
}
