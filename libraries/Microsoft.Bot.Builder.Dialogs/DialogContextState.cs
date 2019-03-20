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
    }

    public class DialogContextState : IDictionary<string, object>
    {
        private readonly DialogContext dialogContext;

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

        [JsonProperty(PropertyName ="turn")]
        public Dictionary<string, object> Turn { get; set; }

        public ICollection<string> Keys => new[] { "user", "conversation", "dialog", "turn" };

        public ICollection<object> Values => new[] { User, Conversation, Dialog, Turn };

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
                throw new NotImplementedException();
            }
        }

        public DialogContextState(DialogContext dc, Dictionary<string, object> userState, Dictionary<string, object> conversationState, Dictionary<string, object> turnState)
        {
            this.dialogContext = dc ?? throw new ArgumentNullException(nameof(dc));
            this.User = userState;
            this.Conversation = conversationState;
            this.Turn = turnState;
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
            // Obtain JToken from the current state
            var thisJToken = JToken.Parse(JsonConvert.SerializeObject(this));

            // JsonPath replace
            var resultToken = thisJToken.ReplacePath(pathExpression, value);

            // Rehydrate and copy state properties to currentobject
            var resultContextState = resultToken.ToObject<DialogContextVisibleState>();
            foreach (var kv in resultContextState.User)
            {
                if (!this.User.ContainsKey(kv.Key))
                {
                    this.User.Add(kv.Key, kv.Value);
                }
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

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
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
