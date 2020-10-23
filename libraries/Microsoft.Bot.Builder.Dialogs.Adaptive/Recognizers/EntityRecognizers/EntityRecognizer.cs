// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.IdentityModel.Logging;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Entity recognizers base class.
    /// </summary>
    public class EntityRecognizer : Recognizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRecognizer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public EntityRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <inheritdoc/>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            // Identify matched intents
            var text = activity.Text ?? string.Empty;
            var locale = activity.Locale ?? "en-us";

            var recognizerResult = new RecognizerResult()
            {
                Text = text,
            };

            if (string.IsNullOrWhiteSpace(text))
            {
                // nothing to recognize, return empty recognizerResult
                return recognizerResult;
            }

            // add entities from regexrecgonizer to the entities pool
            var entityPool = new List<Entity>();

            var textEntity = new TextEntity(text);
            textEntity.Properties["start"] = 0;
            textEntity.Properties["end"] = text.Length;
            textEntity.Properties["score"] = 1.0;

            entityPool.Add(textEntity);

            // process entities using EntityRecognizerSet
            var newEntities = await this.RecognizeEntitiesAsync(dialogContext, entityPool, cancellationToken).ConfigureAwait(false);
            if (newEntities.Any())
            {
                entityPool.AddRange(newEntities);
            }

            // map entityPool of Entity objects => RecognizerResult entity format
            recognizerResult.Entities = new JObject();

            foreach (var entityResult in entityPool)
            {
                // add value
                JToken values;
                if (!recognizerResult.Entities.TryGetValue(entityResult.Type, StringComparison.OrdinalIgnoreCase, out values))
                {
                    values = new JArray();
                    recognizerResult.Entities[entityResult.Type] = values;
                }

                // The Entity type names are not consistent, map everything to camelcase so we can process them cleaner.
                dynamic entity = JObject.FromObject(entityResult);
                ((JArray)values).Add(entity.text);

                // get/create $instance
                JToken instanceRoot;
                if (!recognizerResult.Entities.TryGetValue("$instance", StringComparison.OrdinalIgnoreCase, out instanceRoot))
                {
                    instanceRoot = new JObject();
                    recognizerResult.Entities["$instance"] = instanceRoot;
                }

                // add instanceData
                JToken instanceData;
                if (!((JObject)instanceRoot).TryGetValue(entityResult.Type, StringComparison.OrdinalIgnoreCase, out instanceData))
                {
                    instanceData = new JArray();
                    instanceRoot[entityResult.Type] = instanceData;
                }

                dynamic instance = new JObject();
                instance.startIndex = entity.start;
                instance.endIndex = entity.end;
                instance.score = (double)1.0;
                instance.text = entity.text;
                instance.type = entity.type;
                instance.resolution = entity.resolution;
                ((JArray)instanceData).Add(instance);
            }

            return recognizerResult;
        }

        /// <summary>
        /// Recognizes entities from an <see cref="Entity"/> list.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="entities">The enumerated <see cref="Entity"/> to be recognized.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> from the task.</param>
        /// <returns>Recognized <see cref="Entity"/> list.</returns>
        public virtual Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            return this.RecognizeEntitiesAsync(dialogContext, dialogContext.Context.Activity, entities, cancellationToken);
        }

        /// <summary>
        /// Recognizes entities from an <see cref="Entity"/> list.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="activity">The dialog's <see cref="Activity"/>.</param>
        /// <param name="entities">The enumerated <see cref="Entity"/> to be recognized.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> from the task.</param>
        /// <returns>Recognized <see cref="Entity"/> list.</returns>
        public virtual async Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, Activity activity, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                return await this.RecognizeEntitiesAsync(dialogContext, activity.Text, activity.Locale, entities, cancellationToken).ConfigureAwait(false);
            }

            return new List<Entity>();
        }

        /// <summary>
        /// Recognizes entities from an <see cref="Entity"/> list.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="text">Text to recognize.</param>
        /// <param name="locale">Locale to use.</param>
        /// <param name="entities">The enumerated <see cref="Entity"/> to be recognized.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> from the task.</param>
        /// <returns>Recognized <see cref="Entity"/> list.</returns>
        public virtual Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, string text, string locale, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Entity>>(Array.Empty<Entity>());
        }
    }
}
