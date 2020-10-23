// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// TextEntityRecognizer - base class for Text.Recogizers from the text recognizer library.
    /// </summary>
    public abstract class TextEntityRecognizer : EntityRecognizer
    {
        private static JsonSerializer serializer = new JsonSerializer() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEntityRecognizer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public TextEntityRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
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
        public override Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, string text, string locale, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            List<Entity> newEntities = new List<Entity>();
            var culture = Culture.MapToNearestLanguage(locale ?? string.Empty);

            // look for text entities to recognize 
            foreach (var entity in entities.Where(e => e.Type == TextEntity.TypeName).Select(e => e as TextEntity ?? e.GetAs<TextEntity>()))
            {
                var results = Recognize(entity.Text, culture);
                foreach (var result in results)
                {
                    var newEntity = JObject.FromObject(result, serializer).ToObject<Entity>();
                    newEntity.Type = result.TypeName;
                    newEntity.Properties.Remove("TypeName");
                    newEntities.Add(newEntity);
                }
            }

            return Task.FromResult<IEnumerable<Entity>>(newEntities);
        }

        /// <summary>
        /// Text recognizing implementation.
        /// </summary>
        /// <param name="text">Text to recognize.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The recognized <see cref="ModelResult"/> list.</returns>
        protected abstract List<ModelResult> Recognize(string text, string culture);
    }
}
