// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Classic.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Bridge
{
    public class DictionaryDataBag : IBotDataBag
    {
        private readonly Dictionary<string, object> bag;
        public DictionaryDataBag()
            : this(new Dictionary<string, object>())
        {
        }
        public DictionaryDataBag(Dictionary<string, object> bag)
        {
            this.bag = bag;
        }

        int IBotDataBag.Count { get { return this.bag.Count; } }

        void IBotDataBag.SetValue<T>(string key, T value)
        {
            this.bag[key] = value;
        }

        bool IBotDataBag.ContainsKey(string key)
        {
            return this.bag.ContainsKey(key);
        }

        bool IBotDataBag.TryGetValue<T>(string key, out T value)
        {
            object boxed;
            bool found = this.bag.TryGetValue(key, out boxed);
            if (found)
            {
                if (boxed is T)
                {
                    value = (T)boxed;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        bool IBotDataBag.RemoveValue(string key)
        {
            return this.bag.Remove(key);
        }

        void IBotDataBag.Clear()
        {
            this.bag.Clear();
        }
    }
}
