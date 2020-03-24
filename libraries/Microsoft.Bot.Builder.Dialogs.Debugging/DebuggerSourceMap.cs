// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class DebuggerSourceMap : ISourceMap, IBreakpoints
    {
        private readonly ICodeModel codeModel;
        private readonly object gate = new object();
        private readonly Dictionary<object, SourceRange> sourceByItem = new Dictionary<object, SourceRange>(ReferenceEquality<object>.Instance);
        private bool dirty = true;

        private readonly IIdentifier<Row> rows = new Identifier<Row>();
        private readonly HashSet<object> items = new HashSet<object>(ReferenceEquality<object>.Instance);

        public DebuggerSourceMap(ICodeModel codeModel)
        {
            this.codeModel = codeModel ?? throw new ArgumentNullException(nameof(codeModel));
        }

        public static bool Equals(Protocol.Range target, SourceRange source) =>
            (target.Source == null && source == null)
            || (PathEquals(target.Source.Path, source.Path)
                && target.Line == source.StartPoint.LineIndex
                && target.EndLine == source.EndPoint.LineIndex
                && target.Column == source.StartPoint.CharIndex
                && target.EndColumn == source.EndPoint.CharIndex);

        public static void Assign(Protocol.Range target, SourceRange source)
        {
            if (source != null)
            {
                target.Source = new Protocol.Source(source.Path);
                target.Line = source.StartPoint.LineIndex;
                target.EndLine = source.EndPoint.LineIndex;
                target.Column = source.StartPoint.CharIndex;
                target.EndColumn = source.EndPoint.CharIndex;
            }
            else
            {
                target.Source = null;
                target.Line = null;
                target.EndLine = null;
                target.Column = null;
                target.EndColumn = null;
            }
        }

        public static void Assign(Protocol.Range target, string item, string more)
        {
            target.Item = item;
            target.More = more;
        }

        void ISourceMap.Add(object item, SourceRange range)
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

        bool ISourceMap.TryGetValue(object item, out SourceRange range)
        {
            lock (gate)
            {
                if (item != null)
                {
                    return sourceByItem.TryGetValue(item, out range);
                }
                else
                {
                    range = default(SourceRange);
                    return false;
                }
            }
        }

        IReadOnlyList<Protocol.Breakpoint> IBreakpoints.SetBreakpoints(Protocol.Source source, IReadOnlyList<Protocol.SourceBreakpoint> sourceBreakpoints)
        {
            lock (gate)
            {
                var path = source.Path;
                foreach (var row in rows.Items)
                {
                    if (row.FunctionBreakpoint == null && PathEquals(row.Source.Path, path))
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
                return this.rows[breakpoint.Id].Item;
            }
        }

        private static bool PathEquals(string one, string two)
        {
            var ext1 = Path.GetExtension(one).ToLower();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // we want to be case insensitive on windows
                one = one.ToLower();
                two = two.ToLower();
            }

            // if it's resource path, we only care about resource name
            if (ext1 == ".dialog" || ext1 == ".lg" || ext1 == ".lu")
            {
                return Path.GetFileName(one) == Path.GetFileName(two);
            }

            return Path.Equals(Path.GetFullPath(one), Path.GetFullPath(two));
        }

        private bool TryUpdate(Row row, KeyValuePair<object, SourceRange> sourceItem)
        {
            var item = sourceItem.Key;
            var source = sourceItem.Value;
            if (object.Equals(row.Item, item) && Equals(row.Breakpoint, source))
            {
                return false;
            }

            row.Item = item;
            row.Breakpoint.Verified = source != null;
            Assign(row.Breakpoint, source);

            var name = this.codeModel.NameFor(row.Item);
            Assign(row.Breakpoint, name, null);

            return true;
        }

        private bool TryUpdate(Row row)
        {
            var breakpoint = row.Breakpoint;
            if (breakpoint.Id == 0)
            {
                breakpoint.Id = this.rows.Add(row);
            }

            IEnumerable<KeyValuePair<object, SourceRange>> options;

            var functionBreakpoint = row.FunctionBreakpoint;
            if (functionBreakpoint != null)
            {
                options = from sourceItem in sourceByItem
                          let item = sourceItem.Key
                          let name = codeModel.NameFor(item)
                          where name.IndexOf(functionBreakpoint.Name, StringComparison.CurrentCultureIgnoreCase) >= 0
                          orderby name.Length
                          select sourceItem;
            }
            else
            {
                options = from sourceItem in sourceByItem
                          let source = sourceItem.Value
                          where PathEquals(source.Path, row.Source.Path)
                          where source.StartPoint?.LineIndex <= row.SourceBreakpoint.Line && source.EndPoint?.LineIndex >= row.SourceBreakpoint.Line
                          let distance = row.SourceBreakpoint.Line - source.StartPoint.LineIndex
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
                    var item = row.Item;
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

            public object Item { get; set; }
        }
    }
}
