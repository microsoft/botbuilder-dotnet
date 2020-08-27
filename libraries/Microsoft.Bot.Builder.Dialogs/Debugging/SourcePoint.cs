// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// SourcePoint represents the line and character index into the source code or declarative object backing an object in memory.
    /// </summary>
    public class SourcePoint : IEquatable<SourcePoint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourcePoint"/> class.
        /// </summary>
        public SourcePoint()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourcePoint"/> class.
        /// </summary>
        /// <param name="lineIndex">line index.</param>
        /// <param name="charIndex">char index.</param>
        public SourcePoint(int lineIndex, int charIndex)
        {
            this.LineIndex = lineIndex;
            this.CharIndex = charIndex;
        }

        /// <summary>
        /// Gets or sets line number into the source file.
        /// </summary>
        /// <value>
        /// Line number into the source file.
        /// </value>
        public int LineIndex { get; set; }

        /// <summary>
        /// Gets or sets char index on the line from lineindex.
        /// </summary>
        /// <value>
        /// Char index on the line from lineindex.
        /// </value>
        public int CharIndex { get; set; }

        /// <summary>
        /// Get point from JsonReader.
        /// </summary>
        /// <param name="reader">json reader.</param>
        /// <returns>Point for start of current json reader.</returns>
        public static SourcePoint From(JsonReader reader)
            => (reader is IJsonLineInfo info)
            ? new SourcePoint() { LineIndex = info.LineNumber, CharIndex = info.LinePosition }
            : new SourcePoint();

        /// <summary>
        /// Read object as T and return the start/end points for the object that was read.
        /// </summary>
        /// <typeparam name="T">type of object to deserialize.</typeparam>
        /// <param name="reader">reader to read from.</param>
        /// <param name="read">function to process during reading.</param>
        /// <param name="start">result start point for object.</param>
        /// <param name="end">result end point for object.</param>
        /// <returns>deserialized object as type(T).</returns>
        public static T ReadObjectWithSourcePoints<T>(JsonReader reader, Func<JsonReader, T> read, out SourcePoint start, out SourcePoint end)
        {
            start = SourcePoint.From(reader);
            var item = read(reader);
            end = SourcePoint.From(reader);
            if (start.LineIndex == end.LineIndex && start.CharIndex == end.CharIndex)
            {
                if (reader.Value is string text)
                {
                    start.CharIndex -= text.Length;
                }
                else
                {
                    end.LineIndex += item.ToString().Split('\n').Length - 1;
                }
            }

            return item;
        }

        public override string ToString() => $"{LineIndex}:{CharIndex}";

        public SourcePoint DeepClone() => new SourcePoint() { LineIndex = LineIndex, CharIndex = CharIndex };

        public override bool Equals(object obj)
        {
            // Auto-generated
            return Equals(obj as SourcePoint);
        }

        public bool Equals(SourcePoint other)
        {
            // Auto-generated
            return other != null &&
                   LineIndex == other.LineIndex &&
                   CharIndex == other.CharIndex;
        }

        public override int GetHashCode()
        {
            // Auto-generated
            var hashCode = 1680727000;
            hashCode = (hashCode * -1521134295) + LineIndex.GetHashCode();
            hashCode = (hashCode * -1521134295) + CharIndex.GetHashCode();
            return hashCode;
        }
    }
}
