// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Bot.Builder.Dialogs.Debugging.Base;
using Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels;
using Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers;
using Microsoft.Bot.Builder.Dialogs.Debugging.Protocol;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    internal sealed class DebuggerSourceMap : ISourceMap, IBreakpoints
    {
        private readonly ICodeModel _codeModel;
        private readonly object _gate = new object();
        private readonly HashSet<object> _items = new HashSet<object>(ReferenceEquality<object>.Instance);

        private readonly IIdentifier<Row> _rows = new Identifier<Row>();
        private readonly Dictionary<object, SourceRange> _sourceByItem = new Dictionary<object, SourceRange>(ReferenceEquality<object>.Instance);
        private bool _dirty = true;

        public DebuggerSourceMap(ICodeModel codeModel)
        {
            _codeModel = codeModel ?? throw new ArgumentNullException(nameof(codeModel));
        }

        public static bool Equals(Range target, SourceRange source) =>
            (target.Source == null && source == null)
            || (PathEquals(target.Source.Path, source.Path)
                && target.Line == source.StartPoint.LineIndex
                && target.EndLine == source.EndPoint.LineIndex
                && target.Column == source.StartPoint.CharIndex
                && target.EndColumn == source.EndPoint.CharIndex);

        public static void Assign(Range target, SourceRange source)
        {
            if (source != null)
            {
                target.Designer = source.Designer;
                target.Source = new Source(source.Path);
                target.Line = source.StartPoint.LineIndex;
                target.EndLine = source.EndPoint.LineIndex;
                target.Column = source.StartPoint.CharIndex;
                target.EndColumn = source.EndPoint.CharIndex;
            }
            else
            {
                target.Designer = null;
                target.Source = null;
                target.Line = null;
                target.EndLine = null;
                target.Column = null;
                target.EndColumn = null;
            }
        }

        public static void Assign(Range target, string item, string more)
        {
            target.Item = item;
            target.More = more;
        }

        void ISourceMap.Add(object item, SourceRange range)
        {
            if (range.Path != null && !Path.IsPathRooted(range.Path))
            {
                throw new ArgumentOutOfRangeException(range.Path);
            }

            lock (_gate)
            {
                // Map works on a last one wins basis. When we refresh the item for a range, 
                // just keep the last range

                // Remove old item instance for this range. 
                // Find the entry for the current range
                var rangeItemEntry = _sourceByItem.FirstOrDefault(kv => kv.Value.Equals(range) && kv.Key.GetType().Equals(item.GetType()));

                if (rangeItemEntry.Key != null)
                {
                    // If found, remove the outdated item from the map
                    _sourceByItem.Remove(rangeItemEntry.Key);
                }

                _sourceByItem[item] = range;
                _dirty = true;
            }
        }

        bool ISourceMap.TryGetValue(object item, out SourceRange range)
        {
            lock (_gate)
            {
                if (item != null)
                {
                    return _sourceByItem.TryGetValue(item, out range);
                }

                range = default;
                return false;
            }
        }

        void IBreakpoints.Clear()
        {
            lock (_gate)
            {
                _rows.Clear();
                _items.Clear();
                _dirty = true;
            }
        }

        IReadOnlyList<Breakpoint> IBreakpoints.SetBreakpoints(Source source, IReadOnlyList<SourceBreakpoint> sourceBreakpoints)
        {
            lock (_gate)
            {
                var path = source.Path;
                foreach (var row in _rows.Items)
                {
                    if (row.FunctionBreakpoint == null && PathEquals(row.Source.Path, path))
                    {
                        _rows.Remove(row);
                    }
                }

                var breakpoints = new List<Breakpoint>(sourceBreakpoints.Count);

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

        IReadOnlyList<Breakpoint> IBreakpoints.SetBreakpoints(IReadOnlyList<FunctionBreakpoint> functionBreakpoints)
        {
            lock (_gate)
            {
                foreach (var row in _rows.Items)
                {
                    if (row.FunctionBreakpoint != null)
                    {
                        _rows.Remove(row);
                    }
                }

                var breakpoints = new List<Breakpoint>(functionBreakpoints.Count);

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

        IReadOnlyList<Breakpoint> IBreakpoints.ApplyUpdates()
        {
            lock (_gate)
            {
                IReadOnlyList<Breakpoint> updates = Array.Empty<Breakpoint>();
                if (_dirty)
                {
                    updates = Update();
                    _dirty = false;
                }

                return updates;
            }
        }

        bool IBreakpoints.IsBreakPoint(object item)
        {
            lock (_gate)
            {
                return _items.Contains(item);
            }
        }

        object IBreakpoints.ItemFor(Breakpoint breakpoint)
        {
            lock (_gate)
            {
                return _rows[breakpoint.Id].Item;
            }
        }

        private static bool PathEquals(string one, string two)
        {
            var ext1 = Path.GetExtension(one).ToLowerInvariant();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // we want to be case insensitive on windows
                one = one.ToLowerInvariant();
                two = two.ToLowerInvariant();
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
            if (Equals(row.Item, item) && Equals(row.Breakpoint, source))
            {
                return false;
            }

            row.Item = item;
            row.Breakpoint.Verified = source != null;
            Assign(row.Breakpoint, source);

            var name = _codeModel.NameFor(row.Item);
            Assign(row.Breakpoint, name, null);

            return true;
        }

        private bool TryUpdate(Row row)
        {
            var breakpoint = row.Breakpoint;
            if (breakpoint.Id == 0)
            {
                breakpoint.Id = _rows.Add(row);
            }

            IEnumerable<KeyValuePair<object, SourceRange>> options;

            var functionBreakpoint = row.FunctionBreakpoint;
            if (functionBreakpoint != null)
            {
                options = from sourceItem in _sourceByItem
                    let item = sourceItem.Key
                    let name = _codeModel.NameFor(item)
                    where name.IndexOf(functionBreakpoint.Name, StringComparison.CurrentCultureIgnoreCase) >= 0
                    orderby name.Length
                    select sourceItem;
            }
            else
            {
                options = from sourceItem in _sourceByItem
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
            lock (_gate)
            {
                _items.Clear();
                foreach (var row in _rows.Items)
                {
                    var item = row.Item;
                    if (item != null)
                    {
                        _items.Add(item);
                    }
                }
            }
        }

        private IReadOnlyList<Breakpoint> Update()
        {
            lock (_gate)
            {
                var changes = new List<Breakpoint>();

                foreach (var row in _rows.Items)
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
    }
}
