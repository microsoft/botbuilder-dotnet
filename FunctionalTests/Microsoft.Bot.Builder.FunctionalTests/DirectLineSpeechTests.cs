// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Dialog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    [TestClass]

    [TestCategory("FunctionalTests")]
    [Ignore("DirectLine Speech tests require updates to the REST API and CLI to be able to properly provision a bot.")]
    public class DirectLineSpeechTests
    {
        private static readonly string SoundFileMessage = "Tell me a joke";
        private static readonly string FromUser = "DirectLineSpeechTestUser";
        private static readonly string SpeechRegion = "westus2";
        private static readonly string SoundFilePath = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Assets{Path.DirectorySeparatorChar}TellMeAJoke.wav";
        private static string speechSubscription = null;
        private static string speechBotSecret = null;
        private List<MessageRecord> _messages = new List<MessageRecord>();
        private List<ActivityRecord> _activities = new List<ActivityRecord>();
        private WaveOutEvent _player = new WaveOutEvent();
        private Queue<WavQueueEntry> _playbackStreams = new Queue<WavQueueEntry>();

        private enum Sender
        {
            Bot,
            User,
            Channel,
        }

        [TestMethod]
        public async Task SendDirectLineSpeechVoiceMessage()
        {
            GetEnvironmentVars();

            // Make sure the sound clip exists
            Assert.IsTrue(File.Exists(SoundFilePath));

            // Create a Dialog Service Config for use with the Direct Line Speech Connector
            var config = DialogServiceConfig.FromBotSecret(speechBotSecret, speechSubscription, SpeechRegion);
            config.SpeechRecognitionLanguage = "en-us";
            config.SetProperty(PropertyId.Conversation_From_Id, FromUser);

            // Create a new Dialog Service Connector for the above configuration and register to receive events
            var connector = new DialogServiceConnector(config, AudioConfig.FromWavFileInput(SoundFilePath));
            connector.ActivityReceived += Connector_ActivityReceived;

            // Open a connection to Direct Line Speech channel. No await because the call will block until the connection closes.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            connector.ConnectAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            // Send the message activity to the bot.
            await connector.ListenOnceAsync();

            // Give the bot time to respond.
            System.Threading.Thread.Sleep(1000);

            // Read the bot's message.
            var botAnswer = _messages.LastOrDefault();

            // Cleanup
            await connector.DisconnectAsync();
            connector.Dispose();

            // Assert
            Assert.IsNotNull(botAnswer);
            Assert.AreEqual(string.Format("Echo: {0}.", SoundFileMessage), botAnswer.Message);
        }

        /// <summary>
        /// Get the values for the environment variables.
        /// </summary>
        private void GetEnvironmentVars()
        {
            // The secret for the test bot and DLS channel.
            speechBotSecret = Environment.GetEnvironmentVariable("SPEECHBOTSECRET");
            if (string.IsNullOrWhiteSpace(speechBotSecret))
            {
                Assert.Inconclusive("Environment variable 'SPEECHBOTSECRET' not found.");
            }

            // The cog services key for use with DLS.
            speechSubscription = Environment.GetEnvironmentVariable("SPEECHSUBSCRIPTION");
            if (string.IsNullOrWhiteSpace(speechSubscription))
            {
                Assert.Inconclusive("Environment variable 'SPEECHSUBSCRIPTION' not found.");
            }
        }

        private void Connector_ActivityReceived(object sender, ActivityReceivedEventArgs e)
        {
            var json = e.Activity;
            var activity = JsonConvert.DeserializeObject<Activity>(json);

            if (e.HasAudio)
            {
                var audio = e.Audio;
                var stream = new ProducerConsumerStream();

                Task.Run(() =>
                {
                    var buffer = new byte[800];
                    uint bytesRead = 0;
                    while ((bytesRead = audio.Read(buffer)) > 0)
                    {
                        stream.Write(buffer, 0, (int)bytesRead);
                    }
                });

                var channelData = activity.GetChannelData<SpeechChannelData>();
                var id = channelData?.ConversationalAiData?.RequestInfo?.InteractionId;
                if (!string.IsNullOrEmpty(id))
                {
                    System.Diagnostics.Debug.WriteLine($"Expecting TTS stream {id}");
                }

                var wavStream = new RawSourceWaveStream(stream, new WaveFormat(16000, 16, 1));
                _playbackStreams.Enqueue(new WavQueueEntry(id, false, stream, wavStream));

                if (_player.PlaybackState != PlaybackState.Playing)
                {
                    Task.Run(() => PlayFromAudioQueue());
                }
            }

            var cardsToBeRendered = new List<AdaptiveCard>();
            if (activity.Attachments?.Any() is true)
            {
                cardsToBeRendered = activity.Attachments
                    .Where(x => x.ContentType == AdaptiveCard.ContentType)
                    .Select(x =>
                    {
                        var parseResult = AdaptiveCard.FromJson(x.Content.ToString());
                        return parseResult.Card;
                    })
                    .Where(x => x != null)
                    .ToList();
            }

            _activities.Add(new ActivityRecord(json, activity, Sender.Bot));
            _messages.Add(new MessageRecord(activity.Text, Sender.Bot));
        }

        private bool PlayFromAudioQueue()
        {
            WavQueueEntry entry = null;
            lock (this._playbackStreams)
            {
                if (this._playbackStreams.Count > 0)
                {
                    entry = this._playbackStreams.Peek();
                }
            }

            if (entry != null)
            {
                System.Diagnostics.Debug.WriteLine($"START playing {entry.Id}");
                this._player.Init(entry.Reader);
                this._player.Play();
                return true;
            }

            return false;
        }

        private class MessageRecord
        {
            public MessageRecord(string msg, Sender from, IEnumerable<AdaptiveCard> cards = null) => (this.From, this.Message, this.AdaptiveCards) = (from, msg, cards);

            public Sender From { get; set; }

            public string Message { get; set; }

            public IEnumerable<AdaptiveCard> AdaptiveCards { get; private set; }
        }

        private class RequestDetails
        {
            [JsonProperty("interactionId")]
            public string InteractionId { get; set; }
        }

        private class ConvAiData
        {
            [JsonProperty("requestInfo")]
            public RequestDetails RequestInfo { get; set; }
        }

        private class SpeechChannelData
        {
            [JsonProperty("conversationalAiData")]
            public ConvAiData ConversationalAiData { get; set; }
        }

        private class WavQueueEntry
        {
            public WavQueueEntry(string id, bool playInitiated, ProducerConsumerStream stream, RawSourceWaveStream reader) =>
                (this.Id, this.PlayInitiated, this.Stream, this.Reader) = (id, playInitiated, stream, reader);

            public string Id { get; }

            public bool PlayInitiated { get; set; } = false;

            public bool SynthesisFinished { get; set; } = false;

            public ProducerConsumerStream Stream { get; }

            public RawSourceWaveStream Reader { get; }
        }

        private class ActivityRecord
        {
            public ActivityRecord(string json, IActivity activity, Sender sender) => (this.Json, this.Activity, this.From) = (json, activity, sender);

            public Sender From { get; set; }

            public IActivity Activity { get; set; }

            public string Json { get; set; }

            public DateTime Time { get; set; } = DateTime.Now;
        }

        private class ProducerConsumerStream : Stream
        {
            private readonly MemoryStream _innerStream = new MemoryStream();
            private readonly object _lockable = new object();

            private bool _disposed = false;

            private long _readPosition = 0;

            private long _writePosition = 0;

            public ProducerConsumerStream()
            {
            }

            ~ProducerConsumerStream()
            {
                this.Dispose(false);
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override long Length
            {
                get
                {
                    lock (this._lockable)
                    {
                        return this._innerStream.Length;
                    }
                }
            }

            public override long Position
            {
                get
                {
                    lock (this._lockable)
                    {
                        return this._innerStream.Position;
                    }
                }

                set
                {
                    throw new NotSupportedException();
                }
            }

            public override void Flush()
            {
                lock (this._lockable)
                {
                    this._innerStream.Flush();
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                lock (this._lockable)
                {
                    this._innerStream.Position = this._readPosition;
                    int red = this._innerStream.Read(buffer, offset, count);
                    this._readPosition = this._innerStream.Position;

                    return red;
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                // Seek is for read only
                return this._readPosition;
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                lock (this._lockable)
                {
                    this._innerStream.Position = this._writePosition;
                    this._innerStream.Write(buffer, offset, count);
                    this._writePosition = this._innerStream.Position;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (this._disposed)
                {
                    return;
                }

                if (disposing)
                {
                    // Free managed objects help by this instance
                    if (this._innerStream != null)
                    {
                        this._innerStream.Dispose();
                    }
                }

                // Free any unmanaged objects here.
                this._disposed = true;

                // Call the base class implementation.
                base.Dispose(disposing);
            }
        }
    }
}
