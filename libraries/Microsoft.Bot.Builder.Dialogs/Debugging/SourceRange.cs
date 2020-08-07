// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Range represents a file, starting point and end point of text .
    /// </summary>
    public class SourceRange : IEquatable<SourceRange>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceRange"/> class.
        /// </summary>
        public SourceRange()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceRange"/> class.
        /// </summary>
        /// <param name="path">path.</param>
        /// <param name="startLine">start line index.</param>
        /// <param name="startChar">start char index.</param>
        /// <param name="endLine">end line index.</param>
        /// <param name="endChar">end line char.</param>
        public SourceRange(string path, int startLine, int startChar, int endLine, int endChar)
        {
            this.Path = path;
            this.StartPoint = new SourcePoint(startLine, startChar);
            this.EndPoint = new SourcePoint(endLine, endChar);
        }

        /// <summary>
        /// Gets or sets the optional designer information.
        /// </summary>
        /// <value>
        /// Optional designer information.
        /// </value>
        public JToken Designer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets path to source file.
        /// </summary>
        /// <value>
        /// Path to source file.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets start point in the source file.
        /// </summary>
        /// <value>
        /// Start point in the source file.
        /// </value>
        public SourcePoint StartPoint { get; set; }

        /// <summary>
        /// Gets or sets end point in the source file.
        /// </summary>
        /// <value>
        /// End point in the source file.
        /// </value>
        public SourcePoint EndPoint { get; set; }

        public override string ToString() => $"{System.IO.Path.GetFileName(Path)}:{StartPoint}->{EndPoint}";

        public SourceRange DeepClone()
            => new SourceRange()
            {
                Path = Path,
                Designer = Designer?.DeepClone(),
                StartPoint = StartPoint.DeepClone(),
                EndPoint = EndPoint.DeepClone(),
            };

        public override bool Equals(object obj)
        {
            // Auto-generated
            return Equals(obj as SourceRange);
        }

        public bool Equals(SourceRange other)
        {
            // Auto-generated
            return other != null &&
                   Path == other.Path &&
                   EqualityComparer<SourcePoint>.Default.Equals(StartPoint, other.StartPoint) &&
                   EqualityComparer<SourcePoint>.Default.Equals(EndPoint, other.EndPoint);
        }

        public override int GetHashCode()
        {
            // Auto-generated
            var hashCode = -65866367;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = (hashCode * -1521134295) + EqualityComparer<SourcePoint>.Default.GetHashCode(StartPoint);
            hashCode = (hashCode * -1521134295) + EqualityComparer<SourcePoint>.Default.GetHashCode(EndPoint);
            return hashCode;
        }
    }
}
