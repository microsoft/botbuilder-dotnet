// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1402 // File may only contain a single type. Breaking these out would make too many separate files for handling just one test.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using static Microsoft.Bot.Builder.Azure.CosmosDbPartitionedStorage;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    /// <summary>
    /// <para>
    /// This records the responses to container requests (ReadItemAsync, WriteItemAsync, etc) from Cosmos into .txt files in ../tests/Recordings.
    /// Playback/Recording is determined by _recordingMode in CosmosDbPartitionStorageTests by the environment variable, "COSMOS_RECORDING_MODE".
    /// Set to "playback" for Playback. Set to "record" for Record.
    /// </para>
    /// <para>
    /// When recording, each DocumentStoreItem is added to _recordingsQueue. Once each test completes, the queue is written to the disk.
    /// </para>
    /// </summary>
    internal class CosmosTestRecorder
    {
        private const string RecordingsDir = "Recordings";
        private readonly Dictionary<string, Queue<DocumentStoreItem>> _documentStoreItemQueue = new Dictionary<string, Queue<DocumentStoreItem>>();
        private readonly JsonSerializerSettings _serializerSettings;

        public CosmosTestRecorder(string mode)
        {
            Mode = mode;

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.None,
            };
        }

        public string RecordingFileName { get; set; }

        public string Mode { get; set; }

        internal async Task WriteRecordingsToFiles()
        {
            foreach (var testName in _documentStoreItemQueue.Keys)
            {
                var documentsQueue = _documentStoreItemQueue[testName];
                await WriteToFile(documentsQueue, testName).ConfigureAwait(false);
            }
        }

        internal async Task WriteToFile(Queue<DocumentStoreItem> data, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = RecordingFileName;
            }

            var path = GetRecordingFilePath(fileName);

            using var file = new StreamWriter(path);
            var recordings = SafeJsonConvert.SerializeObject(data, _serializerSettings);
            await file.WriteAsync(recordings).ConfigureAwait(false);
        }

        internal async Task<DocumentStoreItem> GetRecording(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = RecordingFileName;
            }

            var path = GetRecordingFilePath(fileName);

            // We've already loaded the recording file.
            if (_documentStoreItemQueue.TryGetValue(fileName, out var recordings))
            {
                return recordings.Dequeue();
            }

            // Load the recording file and save it to the queue dict.
            using var file = new StreamReader(path);
            var recordingsInFile = await file.ReadToEndAsync().ConfigureAwait(false);

            recordings = SafeJsonConvert.DeserializeObject<Queue<DocumentStoreItem>>(recordingsInFile, _serializerSettings);
            _documentStoreItemQueue[fileName] = recordings;
            return recordings.Dequeue();
        }

        internal void AddRecordingToQueue(DocumentStoreItem document, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = RecordingFileName;
            }

            if (_documentStoreItemQueue.TryGetValue(fileName, out var responses))
            {
                responses.Enqueue(document);
            }
            else
            {
                var queue = new Queue<DocumentStoreItem>();
                queue.Enqueue(document);
                _documentStoreItemQueue.Add(fileName, queue);
            }
        }

        private string GetRecordingFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            return Path.Combine(GetRecordingsPath(), $"{fileName}") + ".txt";
        }

        private string GetRecordingsPath()
        {
            // CurrentDirectory is in ./bin, so we need to back out a few directories
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", RecordingsDir));
        }
    }

    #pragma warning disable SA1204 // Static elements should appear before instance elements
    internal static class RecordingMode
    {
        /// <summary>
        /// Play back recorded HTTP responses instead of making calls to Emulator or Cosmos Service. Use 0 in environment variable.
        /// </summary>
        internal const string Playback = "playback";

        /// <summary>
        /// Record HTTP responses from Cosmos Emulator or Cosmos Service.  Use 1 in environment variable.
        /// </summary>
        internal const string Record = "record";

        /// <summary>
        /// Run the tests like normal, without any recording or mocks.
        /// </summary>
        internal const string Wild = "wild";
    }
}
