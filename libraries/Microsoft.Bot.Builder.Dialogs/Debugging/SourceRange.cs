// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Range represents a file, starting point and end point of text 
    /// </summary>
    public class SourceRange
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
        /// <param name="path">path</param>
        /// <param name="startLine">start line index</param>
        /// <param name="startChar">start char index</param>
        /// <param name="endLine">end line index</param>
        /// <param name="endChar">end line char</param>
        public SourceRange(string path, int startLine, int startChar, int endLine, int endChar)
        {
            this.Path = path;
            this.StartPoint = new SourcePoint(startLine, startChar);
            this.EndPoint = new SourcePoint(endLine, endChar);
        }

        /// <summary>
        /// Gets or sets path to source file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets start point in the source file
        /// </summary>
        public SourcePoint StartPoint { get; set; }

        /// <summary>
        /// Gets or sets end point in the source file
        /// </summary>
        public SourcePoint EndPoint { get; set; }

        public override string ToString() => $"{System.IO.Path.GetFileName(Path)}:{StartPoint}->{EndPoint}";
    }
}
