// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotFramework.Orchestrator;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.AI.Orchestrator.Tests
{
    public class OrchestratorRecognizerTests
    {
        [Fact]
        public async Task LogsTelemetryThrowsArgumentNullExceptionOnNullDialogContext()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();

            var recognizer = new MyRecognizerSubclass("test", "test", new TestLabelResolver()) { TelemetryClient = telemetryClient.Object };
            var activity = MessageFactory.Text("hi");

            await Assert.ThrowsAsync<ArgumentNullException>(() => recognizer.RecognizeAsync(null, activity));
        }

        /// <summary>
        /// Subclass to test <see cref="OrchestratorRecognizer.FillRecognizerResultTelemetryProperties(RecognizerResult, Dictionary{string,string}, DialogContext)"/> functionality.
        /// </summary>
        private class MyRecognizerSubclass : OrchestratorRecognizer
        {
            public MyRecognizerSubclass(string modelFolder, string snapshotFile, ILabelResolver resolverExternal = null) 
                : base(modelFolder, snapshotFile, resolverExternal)
            {
            }

            public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
            {
                var text = activity.Text ?? string.Empty;

                var recognizerResult = new RecognizerResult()
                {
                    Text = text,
                    AlteredText = null,
                };
                recognizerResult.Intents.Add("myTestIntent", new IntentScore { Score = 1.0 });

                TrackRecognizerResult(dialogContext, $"{nameof(MyRecognizerSubclass)}Result", FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties, dialogContext), telemetryMetrics);

                return await Task.FromResult(recognizerResult);
            }
        }

        private class TestLabelResolver : ILabelResolver
        {
            public bool AddExample(in Example example)
            {
                throw new NotImplementedException();
            }

            public bool AddSnapshot(in IReadOnlyList<byte> buffer)
            {
                throw new NotImplementedException();
            }

            public bool AddSnapshot(in IReadOnlyList<byte> buffer, in string labelsPrefix)
            {
                throw new NotImplementedException();
            }

            public IReadOnlyList<byte> CreateSnapshot(bool includeExamples)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public string GetConfigJson()
            {
                throw new NotImplementedException();
            }

            public IReadOnlyList<Result> Score(in string text)
            {
                throw new NotImplementedException();
            }

            public IReadOnlyList<Result> Score(in string text, in LabelType labelType)
            {
                throw new NotImplementedException();
            }

            public void SetRuntimeParams(in string configOrPath, bool reset_all)
            {
                throw new NotImplementedException();
            }
        }
    }
}
