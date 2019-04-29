using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface IEvents
    {
        Protocol.ExceptionBreakpointFilter[] Filters
        {
            get;
        }

        void Reset(IEnumerable<string> filters);

        bool this[string filter]
        {
            get;
            set;
        }
    }

    public sealed class Events : IEvents
    {
        private readonly ConcurrentDictionary<string, bool> stateByFilter = new ConcurrentDictionary<string, bool>();

        public Events(IEnumerable<string> filters = null)
        {
            if (filters == null)
            {
                filters = from field in typeof(DialogContext.DialogEvents).GetFields()
                          where field.FieldType == typeof(string)
                          select (string)field.GetValue(null);

                filters = filters.ToArray();
            }

            foreach (var filter in filters)
            {
                this.stateByFilter.TryAdd(filter, true);
            }
        }

        void IEvents.Reset(IEnumerable<string> filters)
        {
            var index = new HashSet<string>(filters);
            foreach (var filter in stateByFilter.Keys)
            {
                stateByFilter[filter] = index.Contains(filter);
            }
        }

        bool IEvents.this[string filter]
        {
            get => this.stateByFilter.TryGetValue(filter, out var state) ? state : false;
            set => this.stateByFilter[filter] = value;
        }

        Protocol.ExceptionBreakpointFilter[] IEvents.Filters => this.stateByFilter.Select(kv => new Protocol.ExceptionBreakpointFilter() { label = kv.Key, filter = kv.Key, @default = kv.Value }).ToArray();
    }
}
