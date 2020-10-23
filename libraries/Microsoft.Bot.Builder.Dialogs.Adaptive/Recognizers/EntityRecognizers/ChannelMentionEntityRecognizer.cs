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
    /// Recognizer which maps activity.Entities passed by a channel of @type=mention into <see cref="RecognizerResult" /> format.
    /// </summary>
    /// <remarks>
    /// This makes it easy to pass explicit mentions from channels like Teams/Skype to LUIS models.
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
                    //   "mention": [
                    //      "28:0047c760-1f42-4a78-b1bd-9ecd95ec3615"
                    //   ],
                    //   "$instance": {
                    //     "mention": [
                    //        {
                    //            "startIndex": 10,
                    //            "endIndex": 13,
                    //            "score": 1.0,
                    //            "text": "@tom",
                    //            "type": "mention",
                    //            "resolution": {
                    //              "value": "@tom"
                    //            }
                    //         }
                    //      ]
                    //   }
                    // }
                    if (entities.mention == null)
                    {
                        entities.mention = new JArray();
                    }

                    entities.mention.Add(entity.mentioned.id ?? entity.mentioned.name);

                    dynamic instance = entities["$instance"];
                    if (instance == null)
                    {
                        instance = new JObject();
                        entities["$instance"] = instance;
                    }

                    if (instance.mention == null)
                    {
                        instance.mention = new JArray();
                    }

                    string mentionedText = (string)entity.text;
                    iStart = activity.Text.IndexOf(mentionedText, iStart, System.StringComparison.InvariantCulture);
                    if (iStart >= 0)
                    {
                        dynamic mentionData = new JObject();
                        mentionData.type = "mention";
                        mentionData.startIndex = iStart;
                        mentionData.endIndex = iStart + mentionedText.Length - 1;
                        mentionData.text = mentionedText;
                        mentionData.score = 1.0;
                        if (entity.mentioned.name != null)
                        {
                            mentionData.resolution = new JObject();
                            mentionData.resolution.value = entity.mentioned.name;
                        }

                        instance.mention.Add(mentionData);

                        // note, we increment so next pass through continues after the token we just processed.
                        iStart++;
                    }
                }
            }

            return Task.FromResult(result);
        }
    }
}
