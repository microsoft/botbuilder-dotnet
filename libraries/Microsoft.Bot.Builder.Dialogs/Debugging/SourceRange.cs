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

        /// <summary>
        /// Returns a string that represents the current <see cref="SourceRange"/>.
        /// </summary>
        /// <returns>A string that represents the current <see cref="SourceRange"/>.</returns>
        public override string ToString() => $"{System.IO.Path.GetFileName(Path)}:{StartPoint}->{EndPoint}";

        /// <summary>
        /// Creates a new instance of the <see cref="SourceRange"/>. All properties are recursively cloned.
        /// </summary>
        /// <returns>A new instace of the <see cref="SourceRange"/>.</returns>
        public SourceRange DeepClone()
            => new SourceRange()
            {
                Path = Path,
                Designer = Designer?.DeepClone(),
                StartPoint = StartPoint.DeepClone(),
                EndPoint = EndPoint.DeepClone(),
            };

        /// <summary>
        /// Indicates wether the current <see cref="SourceRange"/> is equal to another object.
        /// </summary>
        /// <param name="obj">An object to compare with this <see cref="SourceRange"/>.</param>
        /// <returns><c>true</c> if the current <see cref="SourceRange"/> is equal to the object parameter; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            // Auto-generated
            return Equals(obj as SourceRange);
        }

        /// <summary>
        /// Indicates wether the current <see cref="SourceRange"/> is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this <see cref="SourceRange"/>.</param>
        /// <returns><c>true</c> if the current <see cref="SourceRange"/> is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(SourceRange other)
        {
            // Auto-generated
            return other != null &&
                   Path == other.Path &&
                   EqualityComparer<SourcePoint>.Default.Equals(StartPoint, other.StartPoint) &&
                   EqualityComparer<SourcePoint>.Default.Equals(EndPoint, other.EndPoint);
        }

        /// <summary>
        /// Creates a hash code for the current <see cref="SourceRange"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="SourceRange"/>.</returns>
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
