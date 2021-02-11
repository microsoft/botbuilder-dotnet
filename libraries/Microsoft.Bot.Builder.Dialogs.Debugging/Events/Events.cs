// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Bot.Builder.Dialogs.Debugging.Protocol;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Events
{
    internal sealed class Events<TDialogEvents> : IEvents
        where TDialogEvents : DialogEvents
    {
        private readonly ConcurrentDictionary<string, bool> _stateByFilter = new ConcurrentDictionary<string, bool>();

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
                _stateByFilter.TryAdd(filter, true);
            }

            _stateByFilter["EndDialog"] = false;
        }

        ExceptionBreakpointFilter[] IEvents.Filters =>
            _stateByFilter
            .Select(kv => new ExceptionBreakpointFilter
            {
                Label = kv.Key,
                Filter = kv.Key,
                Default = kv.Value
            })

            // ensure consistency for UI and trace oracle tests
            .OrderBy(f => f.Filter)
            .ToArray();

        bool IEvents.this[string filter]
        {
            get => _stateByFilter.TryGetValue(filter, out var state) ? state : false;
            set => _stateByFilter[filter] = value;
        }

        void IEvents.Reset(IEnumerable<string> filters)
        {
            var index = new HashSet<string>(filters);
            foreach (var filter in _stateByFilter.Keys)
            {
                _stateByFilter[filter] = index.Contains(filter);
            }
        }
    }
}
