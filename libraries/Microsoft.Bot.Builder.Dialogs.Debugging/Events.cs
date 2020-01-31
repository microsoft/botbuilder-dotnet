// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class Events<TDialogEvents> : IEvents
        where TDialogEvents : DialogEvents
    {
        private readonly ConcurrentDictionary<string, bool> stateByFilter = new ConcurrentDictionary<string, bool>();

        public Events(IEnumerable<string> filters = null)
        {
            if (filters == null)
            {
                filters = from field in typeof(TDialogEvents)
                          .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                          where field.FieldType == typeof(string)
                          select (string)field.GetValue(null);

                filters = filters.ToArray();
            }

            foreach (var filter in filters)
            {
                this.stateByFilter.TryAdd(filter, true);
            }

            this.stateByFilter["EndDialog"] = false;
        }

        Protocol.ExceptionBreakpointFilter[] IEvents.Filters =>
            this.stateByFilter.Select(kv => new Protocol.ExceptionBreakpointFilter() { Label = kv.Key, Filter = kv.Key, Default = kv.Value }).ToArray();

        bool IEvents.this[string filter]
        {
            get => this.stateByFilter.TryGetValue(filter, out var state) ? state : false;
            set => this.stateByFilter[filter] = value;
        }

        void IEvents.Reset(IEnumerable<string> filters)
        {
            var index = new HashSet<string>(filters);
            foreach (var filter in stateByFilter.Keys)
            {
                stateByFilter[filter] = index.Contains(filter);
            }
        }
    }
}
