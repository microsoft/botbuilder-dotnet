using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface IBreakpoints
    {
        bool IsBreakPoint(object item);

        object ItemFor(Protocol.Breakpoint breakpoint);

        IReadOnlyList<Protocol.Breakpoint> SetBreakpoints(Protocol.Source source, IReadOnlyList<Protocol.SourceBreakpoint> sourceBreakpoints);

        IReadOnlyList<Protocol.Breakpoint> SetBreakpoints(IReadOnlyList<Protocol.FunctionBreakpoint> functionBreakpoints);

        IReadOnlyList<Protocol.Breakpoint> ApplyUpdates();
    }

    public sealed class SourceMap : Source.IRegistry, IBreakpoints
    {
        private readonly ICodeModel codeModel;
        private readonly object gate = new object();
        private readonly Dictionary<object, Source.Range> sourceByItem = new Dictionary<object, Source.Range>(ReferenceEquality<object>.Instance);
        private bool dirty = true;

        public SourceMap(ICodeModel codeModel)
        {
            this.codeModel = codeModel ?? throw new ArgumentNullException(nameof(codeModel));
        }

        void Source.IRegistry.Add(object item, Source.Range range)
        {
            if (!Path.IsPathRooted(range.Path))
            {
                throw new ArgumentOutOfRangeException(range.Path);
            }

            lock (gate)
            {
                sourceByItem[item] = range;
                dirty = true;
            }
        }

        bool Source.IRegistry.TryGetValue(object item, out Source.Range range)
        {
            lock (gate)
            {
                if (item != null)
                {
                    return sourceByItem.TryGetValue(item, out range);
                }
                else
                {
                    range = default(Source.Range);
                    return false;
                }
            }
        }

        public sealed class Row
        {
            public Row(Protocol.Source source, Protocol.SourceBreakpoint sourceBreakpoint)
            {
                Source = source;
                SourceBreakpoint = sourceBreakpoint;
            }

            public Row(Protocol.FunctionBreakpoint functionBreakpoint)
            {
                FunctionBreakpoint = functionBreakpoint;
            }

            public Protocol.Source Source { get; }

            public Protocol.SourceBreakpoint SourceBreakpoint { get; }

            public Protocol.FunctionBreakpoint FunctionBreakpoint { get; }

            public Protocol.Breakpoint Breakpoint { get; } = new Protocol.Breakpoint();

            public object item { get; set; }
        }

        private readonly Identifier<Row> rows = new Identifier<Row>();
        private readonly HashSet<object> items = new HashSet<object>(ReferenceEquality<object>.Instance);

        // TODO: incorrect on unix, need to resolve through file system
        // on VSCode Insiders, drive letter casing changes
        private static bool PathEquals(string one, string two) =>
            string.Equals(one, two, StringComparison.CurrentCultureIgnoreCase);

        public static bool Equals(Protocol.Range target, Source.Range source) =>
            (target.source == null && source == null)
            || (PathEquals(target.source.path, source.Path)
                && target.line == source.Start.LineIndex
                && target.endLine == source.After.LineIndex
                && target.column == source.Start.CharIndex
                && target.endColumn == source.After.CharIndex);

        public static void Assign(Protocol.Range target, Source.Range source)
        {
            if (source != null)
            {
                target.source = new Protocol.Source(source.Path);
                target.line = source.Start.LineIndex;
                target.endLine = source.After.LineIndex;
                target.column = source.Start.CharIndex;
                target.endColumn = source.After.CharIndex;
            }
            else
            {
                target.source = null;
                target.line = null;
                target.endLine = null;
                target.column = null;
                target.endColumn = null;
            }
        }

        private bool TryUpdate(Row row, KeyValuePair<object, Source.Range> sourceItem)
        {
            var item = sourceItem.Key;
            var source = sourceItem.Value;
            if (object.Equals(row.item, item) && Equals(row.Breakpoint, source))
            {
                return false;
            }

            row.item = item;
            row.Breakpoint.verified = source != null;
            Assign(row.Breakpoint, source);
            return true;
        }

        private bool TryUpdate(Row row)
        {
            var breakpoint = row.Breakpoint;
            if (breakpoint.id == 0)
            {
                breakpoint.id = this.rows.Add(row);
            }

            IEnumerable<KeyValuePair<object, Source.Range>> options;

            var functionBreakpoint = row.FunctionBreakpoint;
            if (functionBreakpoint != null)
            {
                options = from sourceItem in sourceByItem
                          let item = sourceItem.Key
                          let name = codeModel.NameFor(item)
                          where name.IndexOf(functionBreakpoint.name, StringComparison.CurrentCultureIgnoreCase) >= 0
                          orderby name.Length
                          select sourceItem;
            }
            else
            {
                options = from sourceItem in sourceByItem
                          let source = sourceItem.Value
                          where PathEquals(source.Path, row.Source.path)
                          where source.Start.LineIndex >= row.SourceBreakpoint.line
                          let distance = Math.Abs(source.Start.LineIndex - row.SourceBreakpoint.line)
                          orderby distance
                          select sourceItem;
            }

            options = options.ToArray();
            var best = options.FirstOrDefault();

            return TryUpdate(row, best);
        }

        private void RebuildItems()
        {
            lock (gate)
            {
                items.Clear();
                foreach (var row in rows.Items)
                {
                    var item = row.item;
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }
        }

        private IReadOnlyList<Protocol.Breakpoint> Update()
        {
            lock (gate)
            {
                var changes = new List<Protocol.Breakpoint>();

                foreach (var row in rows.Items)
                {
                    if (TryUpdate(row))
                    {
                        changes.Add(row.Breakpoint);
                    }
                }

                if (changes.Count > 0)
                {
                    RebuildItems();
                }

                return changes;
            }
        }

        IReadOnlyList<Protocol.Breakpoint> IBreakpoints.SetBreakpoints(Protocol.Source source, IReadOnlyList<Protocol.SourceBreakpoint> sourceBreakpoints)
        {
            lock (gate)
            {
                var path = source.path;
                foreach (var row in rows.Items)
                {
                    if (row.FunctionBreakpoint == null && PathEquals(row.Source.path, path))
                    {
                        rows.Remove(row);
                    }
                }

                var breakpoints = new List<Protocol.Breakpoint>(sourceBreakpoints.Count);

                foreach (var sourceBreakpoint in sourceBreakpoints)
                {
                    var row = new Row(source, sourceBreakpoint);
                    TryUpdate(row);
                    breakpoints.Add(row.Breakpoint);
                }

                RebuildItems();

                return breakpoints;
            }
        }

        IReadOnlyList<Protocol.Breakpoint> IBreakpoints.SetBreakpoints(IReadOnlyList<Protocol.FunctionBreakpoint> functionBreakpoints)
        {
            lock (gate)
            {
                foreach (var row in rows.Items)
                {
                    if (row.FunctionBreakpoint != null)
                    {
                        rows.Remove(row);
                    }
                }

                var breakpoints = new List<Protocol.Breakpoint>(functionBreakpoints.Count);

                foreach (var functionBreakpoint in functionBreakpoints)
                {
                    var row = new Row(functionBreakpoint);
                    TryUpdate(row);
                    breakpoints.Add(row.Breakpoint);
                }

                RebuildItems();

                return breakpoints;
            }
        }

        IReadOnlyList<Protocol.Breakpoint> IBreakpoints.ApplyUpdates()
        {
            lock (gate)
            {
                IReadOnlyList<Protocol.Breakpoint> updates = Array.Empty<Protocol.Breakpoint>();
                if (dirty)
                {
                    updates = Update();
                    dirty = false;
                }

                return updates;
            }
        }

        bool IBreakpoints.IsBreakPoint(object item)
        {
            lock (gate)
            {
                return this.items.Contains(item);
            }
        }

        object IBreakpoints.ItemFor(Protocol.Breakpoint breakpoint)
        {
            lock (gate)
            {
                return this.rows[breakpoint.id].item;
            }
        }
    }
}
