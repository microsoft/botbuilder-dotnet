// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels;
using Microsoft.Bot.Builder.Dialogs.Debugging.Protocol;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests
{
    public sealed class DebuggerSourceMapTests
    {
        [Fact]
        public void DebuggerSourceMap_Constructor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new DebuggerSourceMap(null);
            });
        }

        [Fact]
        public void DebuggerSourceMap_Constructor_Works()
        {
            var codeModel = new CodeModel();
            Assert.NotNull(new DebuggerSourceMap(codeModel));
        }

        [Fact]
        public void DebuggerSourceMap_Assign_Item()
        {
            var stackFrame = new StackFrame
            {
                Id = 1,
                Name = "TestFrame"
            };

            DebuggerSourceMap.Assign(stackFrame, "item", "more");

            Assert.Equal("item", stackFrame.Item);
            Assert.Equal("more", stackFrame.More);
        }

        [Fact]
        public void DebuggerSourceMap_Assign_Null_SourceRange()
        {
            var stackFrame = new StackFrame
            {
                Id = 1,
                Name = "TestFrame"
            };

            DebuggerSourceMap.Assign(stackFrame, null);

            Assert.Null(stackFrame.Designer);
            Assert.Null(stackFrame.Source);
            Assert.Null(stackFrame.Line);
            Assert.Null(stackFrame.EndLine);
            Assert.Null(stackFrame.Column);
            Assert.Null(stackFrame.EndColumn);
        }

        [Fact]
        public void DebuggerSourceMap_Assign_SourceRange()
        {
            var stackFrame = new StackFrame
            {
                Id = 1,
                Name = "TestFrame"
            };
            var sourceRange = new SourceRange
            {
                Designer = "testDesigner",
                Path = "test.path",
                StartPoint = new SourcePoint(0, 0),
                EndPoint = new SourcePoint(10, 10)
            };

            DebuggerSourceMap.Assign(stackFrame, sourceRange);

            Assert.Equal(sourceRange.Designer, stackFrame.Designer);
            Assert.Equal(sourceRange.Path, stackFrame.Source.Path);
            Assert.Equal(sourceRange.StartPoint.LineIndex, stackFrame.Line);
            Assert.Equal(sourceRange.EndPoint.LineIndex, stackFrame.EndLine);
            Assert.Equal(sourceRange.StartPoint.CharIndex, stackFrame.Column);
            Assert.Equal(sourceRange.EndPoint.CharIndex, stackFrame.EndColumn);
        }

        [Fact]
        public void DebuggerSourceMap_Equals_Null_Source()
        {
            var stackFrame = new StackFrame
            {
                Id = 1,
                Name = "TestFrame",
                Source = null
            };

            Assert.True(DebuggerSourceMap.Equals(stackFrame, null));
        }

        [Fact]
        public void DebuggerSourceMap_Equals_False()
        {
            var stackFrame = new StackFrame
            {
                Id = 1,
                Name = "TestFrame",
                Source = new Source("test.source.path")
            };
            var sourceRange = new SourceRange
            {
                Designer = "testDesigner",
                Path = "test.path",
                StartPoint = new SourcePoint(0, 0),
                EndPoint = new SourcePoint(10, 10)
            };

            Assert.False(DebuggerSourceMap.Equals(stackFrame, sourceRange));
        }

        [Fact]
        public void DebuggerSourceMap_Equals_True()
        {
            var stackFrame = new StackFrame
            {
                Id = 1,
                Name = "TestFrame"
            };
            var sourceRange = new SourceRange
            {
                Designer = "testDesigner",
                Path = "path/test",
                StartPoint = new SourcePoint(0, 0),
                EndPoint = new SourcePoint(10, 10)
            };

            DebuggerSourceMap.Assign(stackFrame, sourceRange);

            Assert.True(DebuggerSourceMap.Equals(stackFrame, sourceRange));
        }

        [Fact]
        public void DebuggerSourceMap_Equals_True_With_Resources()
        {
            var stackFrame = new StackFrame
            {
                Id = 1,
                Name = "TestFrame"
            };
            var sourceRange = new SourceRange
            {
                Designer = "testDesigner",
                Path = "path/test.dialog",
                StartPoint = new SourcePoint(0, 0),
                EndPoint = new SourcePoint(10, 10)
            };

            DebuggerSourceMap.Assign(stackFrame, sourceRange);

            Assert.True(DebuggerSourceMap.Equals(stackFrame, sourceRange));
        }

        [Fact]
        public void DebuggerSourceMap_Add_InvalidPath_Throws()
        {
            var codeModel = new CodeModel();
            ISourceMap sourceMap = new DebuggerSourceMap(codeModel);

            var sourceRange = new SourceRange
            {
                Path = "test.path"
            };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                sourceMap.Add("test-item", sourceRange);
            });
        }

        [Fact]
        public void DebuggerSourceMap_Add()
        {
            var codeModel = new CodeModel();
            ISourceMap sourceMap = new DebuggerSourceMap(codeModel);
            const string item = "test-item";

            var sourceRange = new SourceRange
            {
                Designer = "testDesigner",
                Path = "/test/path",
                StartPoint = new SourcePoint(0, 0),
                EndPoint = new SourcePoint(10, 10)
            };

            sourceMap.Add(item, sourceRange);

            Assert.True(sourceMap.TryGetValue(item, out _));
        }

        [Fact]
        public void DebuggerSourceMap_Add_With_Remove()
        {
            var codeModel = new CodeModel();
            ISourceMap sourceMap = new DebuggerSourceMap(codeModel);
            const string item = "test-item";

            var sourceRange = new SourceRange
            {
                Designer = "testDesigner",
                Path = "/test/path",
                StartPoint = new SourcePoint(0, 0),
                EndPoint = new SourcePoint(10, 10)
            };

            // Add the item to the map.
            sourceMap.Add(item, sourceRange);
            Assert.True(sourceMap.TryGetValue(item, out _));

            // Add the same item so it removes it from the map before adding it again.
            sourceMap.Add(item, sourceRange);
            Assert.True(sourceMap.TryGetValue(item, out _));
        }

        [Fact]
        public void DebuggerSourceMap_SetBreakpoints_Source()
        {
            var codeModel = new CodeModel();
            IBreakpoints sourceMap = new DebuggerSourceMap(codeModel);
            var breakpointList = new List<SourceBreakpoint>
            {
                new SourceBreakpoint { Line = 1 }
            };

            var source = new Source("/test-path")
            {
                Name = "testSource"
            };

            var breakpoints = sourceMap.SetBreakpoints(source, breakpointList);
            Assert.Single(breakpoints);
        }

        [Fact]
        public void DebuggerSourceMap_SetBreakpoints_Function()
        {
            var codeModel = new CodeModel();
            IBreakpoints sourceMap = new DebuggerSourceMap(codeModel);
            var breakpointList = new List<FunctionBreakpoint>
            {
                new FunctionBreakpoint { Name = "test-breakpoint" }
            };

            var breakpoints = sourceMap.SetBreakpoints(breakpointList);
            Assert.Single(breakpoints);
        }
    }
}
