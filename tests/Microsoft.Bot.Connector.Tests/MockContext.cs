// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Azure.Test.HttpRecorder;
using Microsoft.Azure.Test.HttpRecorder.ProcessRecordings;

namespace Microsoft.Bot.Connector.Tests
{
    /// <summary>
    /// A coordinator for tracking and undoing WAML operations.  Usage pattern is
    /// using(MockContext.Create())
    /// {
    ///   maml stuff
    /// }
    /// You can also manually call the Dispose() or UndoAll() methods to undo all 'undoable' operations since the
    /// UndoContext was created.
    /// Call: MockContext.Commit() to remove all undo information.
    /// </summary>
    public class MockContext : IDisposable
    {
        //prevent multiple dispose events
        private bool disposed = false;
        private List<ResourceGroupCleaner> undoHandlers = new List<ResourceGroupCleaner>();

        internal bool OptimizeTestRecordingFile { get; set; } = false;

        /// <summary>
        /// Initialize a new MockContext.
        /// </summary>
        /// <param name="className">The class name to identify the mock server.</param>
        /// <returns>Returns a new MockContext.</returns>
        /// <param name="methodName">The name method used for the test.</param>
        public static MockContext Start(
            string className,
            [System.Runtime.CompilerServices.CallerMemberName]
            string methodName = "testframework_failed")
        {
            var context = new MockContext();
            if (HttpMockServer.FileSystemUtilsObject == null)
            {
                HttpMockServer.FileSystemUtilsObject = new FileSystemUtils();
            }

            HttpMockServer.Initialize(className, methodName);
            if (HttpMockServer.Mode != HttpRecorderMode.Playback)
            {
                context.disposed = false;
            }

            return context;
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Stop recording and Discard all undo information.
        /// </summary>
        public void Stop()
        {
            if (HttpMockServer.Mode != HttpRecorderMode.Playback)
            {
                foreach (var undoHandler in undoHandlers)
                {
                    undoHandler.DeleteResourceGroups().ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }

            string recordedFilePath = HttpMockServer.Flush();

            if (HttpMockServer.Mode == HttpRecorderMode.Record)
            {
                // this check should be removed once we make the optimizatoin default
                if (OptimizeTestRecordingFile)
                {
                    ProcessRecordedFiles procRecFile = new ProcessRecordedFiles(recordedFilePath);
                    procRecFile.CompactLroPolling();
                    procRecFile.SerializeCompactData();
                }
            }
        }

        /// <summary>
        /// Dispose only if we have not previously been disposed.
        /// </summary>
        /// <param name="disposing">true if we should dispose, otherwise false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                this.Stop();
                this.disposed = true;
            }
        }     
    }
}
