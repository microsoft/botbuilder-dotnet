// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public static class Source
    {
        public interface IRegistry
        {
            void Add(object item, Range range);

            bool TryGetValue(object item, out Range range);
        }

        public struct Point
        {
            public int LineIndex { get; set; }

            public int CharIndex { get; set; }

            public static Point From(JsonReader reader)
                => (reader is IJsonLineInfo info)
                ? new Point() { LineIndex = info.LineNumber, CharIndex = info.LinePosition }
                : new Point();

            public static T Read<T>(JsonReader reader, Func<JsonReader, T> read, out Point start, out Point after)
            {
                start = Point.From(reader);
                var item = read(reader);
                after = Point.From(reader);
                if (start.LineIndex == after.LineIndex && start.CharIndex == after.CharIndex)
                {
                    if (reader.Value is string text)
                    {
                        start.CharIndex -= text.Length;
                    }
                }

                return item;
            }

            public override string ToString() => $"{LineIndex}:{CharIndex}";
        }

        public sealed class Range
        {
            public string Path { get; set; }

            public Point Start { get; set; }

            public Point After { get; set; }

            public override string ToString() => $"{System.IO.Path.GetFileName(Path)}:{Start}->{After}";
        }

        public sealed class NullRegistry : IRegistry
        {
            public static readonly IRegistry Instance = new NullRegistry();

            private NullRegistry()
            {
            }

            void IRegistry.Add(object item, Range range)
            {
            }

            bool IRegistry.TryGetValue(object item, out Range range)
            {
                range = default(Range);
                return false;
            }
        }
    }
}
