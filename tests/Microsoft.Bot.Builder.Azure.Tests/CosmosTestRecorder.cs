using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Rest.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
#pragma warning disable SA1402 // File may only contain a single type

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public enum RecordingMode
    {
        /// <summary>
        /// Play back recorded HTTP responses instead of making calls to Emulator or Cosmos Service.
        /// </summary>
        Playback,

        /// <summary>
        /// Record HTTP responses from Cosmos Emulator or Cosmos Service.
        /// </summary>
        Record
    }

    public class CosmosTestRecorder : RequestHandler
    {
        private readonly string _recordingsDir = "Recordings";
        private readonly Dictionary<string, Queue<SerializableResponseMessage>> _recordingsQueue = new Dictionary<string, Queue<SerializableResponseMessage>>();
        private readonly JsonSerializerSettings _serializerSettings;

        public CosmosTestRecorder(RecordingMode mode)
        {
            Mode = mode;

            _serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>() { new MemoryStreamJsonConverter() },
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.All,
                ConstructorHandling = ConstructorHandling.Default,
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
        }

        public string RecordingFileName { get; set; }

        public RecordingMode Mode { get; set; }

        public override async Task<ResponseMessage> SendAsync(RequestMessage request, CancellationToken cancellationToken)
        {
            if (Mode == RecordingMode.Playback)
            {
                if (string.IsNullOrEmpty(RecordingFileName))
                {
                    throw new ArgumentNullException("You must set RecordingFileName before trying to read test.");
                }

                var recording = await GetRecording(RecordingFileName, request).ConfigureAwait(false);

                // TODO: DELETE
                //var test = ResponseMessageToString(recording);

                return recording ?? throw new IOException($"Unable to find recording file: {RecordingFileName}");
            }
            else
            {
                var result = await base.SendAsync(request, cancellationToken).ConfigureAwait(true);

                if (Mode == RecordingMode.Record)
                {
                    var dirPath = GetRecordingsPath();
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    SaveRecording(RecordingFileName, result);
                }

                return result;
            }
        }

        public async Task WriteRecordingsToFiles()
        {
            foreach (var testName in _recordingsQueue.Keys)
            {
                var responses = _recordingsQueue[testName];

                var path = GetRecordingFilePath(testName);
                using var file = new StreamWriter(path);
                var recordings = SafeJsonConvert.SerializeObject(responses, _serializerSettings);
                await file.WriteAsync(recordings).ConfigureAwait(false);
            }
        }

        // TODO: DELETE
        public string ResponseMessageToString(ResponseMessage response)
        {
            var str = SafeJsonConvert.SerializeObject(response, _serializerSettings);
            return str;
        }

        // TODO: DELETE
        // TODO: Serialization is a bitch. This is a way to Quick-Test how it goes. Up next,
        // try making a SerializableResponseMessage. Convert to that prior to Write, Deserialize to it on Read,
        // then use new ResponseMessage(), Content =, Headers.Add, etc until we have a match
        // TODO: Also create SerializableRequestMessage
        //public async Task QuickTest()
        //{
        //    var status = HttpStatusCode.OK;
        //    var messageMode = HttpMethod.Put;
        //    var fakeUrl = "http://www.fake.url";
        //    var fakeError = "fakeError";
        //    var fakeName = "fakeName";
        //    var fakeValue = "fakeValue";

        //    var response = new ResponseMessage(status, new RequestMessage(messageMode, new Uri(fakeUrl)), fakeError);
        //    response.Headers.Add(fakeName, fakeValue);

        //    response.Content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("testString"));

        //    await WriteResponse(response);

        //    var fromFile = await GetResponse();
        //    Assert.AreEqual(status, fromFile.StatusCode);
        //    Assert.AreEqual(messageMode, fromFile.RequestMessage.Method);
        //    Assert.AreEqual(fakeUrl, fromFile.RequestMessage.RequestUri.AbsoluteUri);
        //    Assert.AreEqual(fakeError, fromFile.ErrorMessage);
        //    Assert.AreEqual(fakeValue, fromFile.Headers[fakeName]);
        //}

        //// TODO: DELETE
        //private async Task WriteResponse(ResponseMessage response)
        //{
        //    var serializeableResponse = new SerializableResponseMessage(response);

        //    var path = GetRecordingFilePath("quickTest");
        //    using var file = new StreamWriter(path);
        //    var recordings = SafeJsonConvert.SerializeObject(serializeableResponse, _serializerSettings);
        //    await file.WriteAsync(recordings).ConfigureAwait(false);
        //}

        //// TODO: DELETE
        //private async Task<ResponseMessage> GetResponse()
        //{
        //    var path = GetRecordingFilePath("quickTest");
        //    using var file = new StreamReader(path);
        //    var recordingsInFile = await file.ReadToEndAsync().ConfigureAwait(true);
        //    var recording = SafeJsonConvert.DeserializeObject<SerializableResponseMessage>(recordingsInFile, _serializerSettings);
        //    return recording.GetCosmosResponseMessage();
        //}

        private async Task<ResponseMessage> GetRecording(string fileName, RequestMessage request)
        {
            var path = GetRecordingFilePath(fileName);

            if (_recordingsQueue.TryGetValue(fileName, out var recordings))
            {
                return recordings.Dequeue().GetCosmosResponseMessage();
            }

            using var file = new StreamReader(path);
            var recordingsInFile = await file.ReadToEndAsync().ConfigureAwait(false);

            recordings = SafeJsonConvert.DeserializeObject<Queue<SerializableResponseMessage>>(recordingsInFile, _serializerSettings);
            var recording = recordings.Dequeue().GetCosmosResponseMessage();

            _recordingsQueue[fileName] = recordings;
            return recording;
        }

        private void SaveRecording(string fileName, ResponseMessage response)
        {
            if (_recordingsQueue.TryGetValue(fileName, out var responses))
            {
                responses.Enqueue(new SerializableResponseMessage(response));
            }
            else
            {
                var queue = new Queue<SerializableResponseMessage>();
                queue.Enqueue(new SerializableResponseMessage(response));
                _recordingsQueue.Add(fileName, queue);
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

        // CurrentDirectory is in ./bin, so we need to back out a few directories
        private string GetRecordingsPath()
        {
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", _recordingsDir));
        }
    }

    [Serializable]
    public class SerializableMessageBase
    {
        [JsonProperty]
        public Stream Content { get; set; }

        [JsonProperty]
        public Headers Headers { get; set; }

        public Stream CreateStreamCopy(Stream source)
        {
            var stream = new MemoryStream();

            if (source != null)
            {
                source.CopyTo(stream);
                source.Position = 0;
                stream.Position = 0;
            }

            return stream;
        }

        public void CopyHeaders(Headers source, Headers destination)
        {
            foreach (var headerKey in source)
            {
                destination[headerKey] = source[headerKey];
            }
        }
    }

    [Serializable]
    public class SerializableResponseMessage : SerializableMessageBase
    {
        public SerializableResponseMessage()
        {
        }

        public SerializableResponseMessage(ResponseMessage response)
        {
            StatusCode = response.StatusCode;
            Headers = response.Headers;
            ErrorMessage = response.ErrorMessage;
            RequestMessage = new SerializableRequestMessage(response.RequestMessage);
            Content = CreateStreamCopy(response.Content);
        }

        [JsonProperty]
        public HttpStatusCode StatusCode { get; set; }

        [JsonProperty]
        public SerializableRequestMessage RequestMessage { get; set; }

        [JsonProperty]
        public string ErrorMessage { get; set; }

        public RequestMessage GetCosmosRequestMessage()
        {
            var cosmosRequestMessage = new RequestMessage(RequestMessage.Method, RequestMessage.RequestUri);
            cosmosRequestMessage.Content = CreateStreamCopy(RequestMessage.Content);
            CopyHeaders(RequestMessage.Headers, cosmosRequestMessage.Headers);

            return cosmosRequestMessage;
        }

        public ResponseMessage GetCosmosResponseMessage()
        {
            var cosmosResponseMessage = new ResponseMessage(StatusCode, GetCosmosRequestMessage(), ErrorMessage);
            cosmosResponseMessage.Content = CreateStreamCopy(Content);
            CopyHeaders(Headers, cosmosResponseMessage.Headers);

            return cosmosResponseMessage;
        }
    }

    [Serializable]
    public class SerializableRequestMessage : SerializableMessageBase
    {
        public SerializableRequestMessage()
        {
        }

        public SerializableRequestMessage(RequestMessage request)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            Headers = request.Headers;
            Content = CreateStreamCopy(request.Content);
        }

        [JsonProperty]
        public HttpMethod Method { get; set; }

        [JsonProperty]
        public Uri RequestUri { get; set; }
    }
}
