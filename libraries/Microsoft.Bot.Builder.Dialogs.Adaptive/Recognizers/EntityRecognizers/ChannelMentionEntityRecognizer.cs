// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizer which maps channel activity.Entities of type mention into <see cref="RecognizerResult" /> format.
    /// </summary>
    /// <remarks>
    /// This makes it easy to pass explicit mentions from channels like Teams/Skype to LUIS models.
    /// The generated entity is named 'channelMention' with resolution {name,id} like this:
    /// "entities": {
    ///   "channelMention": [
    ///      {
    ///         "id": "28:0047c760-1f42-4a78-b1bd-9ecd95ec3615"
    ///         "name":"Tess"
    ///      }
    ///   ],
    ///   "$instance": {
    ///     "channelMention": [
    ///        {
    ///            "startIndex": 10,
    ///            "endIndex": 13,
    ///            "score": 1.0,
    ///            "text": "tess"
    ///         }
    ///      ]
    ///   }
    /// }.
    /// </remarks>
    public class ChannelMentionEntityRecognizer : Recognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ChannelMentionEntityRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelMentionEntityRecognizer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public ChannelMentionEntityRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <inheritdoc/>
        public override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var result = new RecognizerResult();

            // promote external mention entities from the activity into recognizer result
            if (activity.Entities != null)
            {
                if (result.Entities == null)
                {
                    result.Entities = new JObject();
                }

                dynamic entities = result.Entities;

                // convert activity.entities entity that looks like this:
                // {
                //    "type": "mention",
                //    "mentioned": {
                //        "id": "28:0047c760-1f42-4a78-b1bd-9ecd95ec3615",
                //        "name": "Tess"
                //    },
                //    "text": "<at>Tess</at>"
                // },
                int iStart = 0;
                foreach (dynamic entity in activity.Entities.Where(e => e.Type == "mention").Select(e => JObject.FromObject(e)))
                {
                    // into recognizeresult that looks like this:
                    // "entities": {
                    //   "channelMention": [
                    //      {
                    //         "id": "28:0047c760-1f42-4a78-b1bd-9ecd95ec3615"
                    //         "name":"Tess"
                    //      }
                    //   ],
                    //   "$instance": {
                    //     "channelMention": [
                    //        {
                    //            "startIndex": 10,
                    //            "endIndex": 14, 
                    //            "score": 1.0,
                    //            "text": "tess"
                    //         }
                    //      ]
                    //   }
                    // }
                    if (entities.channelMention == null)
                    {
                        entities.channelMention = new JArray();
                    }

                    entities.channelMention.Add(entity.mentioned);

                    dynamic instance = entities["$instance"];
                    if (instance == null)
                    {
                        instance = new JObject();
                        entities["$instance"] = instance;
                    }

                    if (instance.channelMention == null)
                    {
                        instance.channelMention = new JArray();
                    }

                    string mentionedText = (string)entity.text;
                    iStart = activity.Text.IndexOf(mentionedText, iStart, System.StringComparison.InvariantCulture);
                    if (iStart >= 0)
                    {
                        dynamic mentionData = new JObject();
                        mentionData.startIndex = iStart;
                        mentionData.endIndex = iStart + mentionedText.Length;
                        mentionData.text = mentionedText;
                        mentionData.score = 1.0;
                        instance.channelMention.Add(mentionData);

                        // note, we increment so next pass through continues after the token we just processed.
                        iStart++;
                    }
                }
            }

            return Task.FromResult(result);
        }
    }
}
