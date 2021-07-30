// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Description of the intents and entities a recognizer can return.
    /// </summary>
    public partial class RecognizerDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecognizerDescription"/> class.
        /// </summary>
        /// <param name="intents">Intents.</param>
        /// <param name="entities">Entities.</param>
        /// <param name="dynamicLists">Dynamically defined entities.</param>
        public RecognizerDescription(IEnumerable<IntentDescription> intents = null, IEnumerable<EntityDescription> entities = null, IEnumerable<DynamicList> dynamicLists = null)
        {
            Intents = intents != null ? intents.ToList() : new List<IntentDescription>();
            Entities = entities != null ? entities.ToList() : new List<EntityDescription>();
            DynamicLists = dynamicLists != null ? dynamicLists.ToList() : new List<DynamicList>();
        }

        /// <summary>
        /// Gets the intents that can be recognized.
        /// </summary>
        /// <value>List of <see cref="IntentDescription"/>.</value>
        [JsonProperty("intents")]
        public IReadOnlyList<IntentDescription> Intents { get; }

        /// <summary>
        /// Gets the entities that can be recognized.
        /// </summary>
        /// <value>List of <see cref="EntityDescription"/>.</value>
        [JsonProperty("entities")]
        public IReadOnlyList<EntityDescription> Entities { get; }

        /// <summary>
        /// Gets a list of the dynamically defined entities that can be recognized.
        /// </summary>
        /// <value>List of <see cref="DynamicList"/>.</value>
        [JsonProperty("dynamicLists")]
        public IReadOnlyList<DynamicList> DynamicLists { get; }

        /// <summary>
        /// Merge multiple recognizer descriptions into one.
        /// </summary>
        /// <param name="descriptions">Enumerable of descriptions.</param>
        /// <returns>Union of descriptions.</returns>
        public static RecognizerDescription MergeDescriptions(IEnumerable<RecognizerDescription> descriptions)
        {
            var intents = new List<IntentDescription>();
            var entities = new List<EntityDescription>();
            var lists = new List<DynamicList>();
            foreach (var description in descriptions)
            {
                intents.AddRange(description.Intents);
                entities.AddRange(description.Entities);
                lists.AddRange(description.DynamicLists);
            }

            return new RecognizerDescription(intents, entities, lists);
        }
        
        /// <inheritdoc/>
        public override string ToString()
            => $"RecognizerDescription({ListString(Intents.Select(e => e.Name))}, {ListString(Entities.Select(e => e.Name))}, {ListString(DynamicLists.Select(e => e.Entity))})";

        private string ListString(IEnumerable<string> list)
            => $"[" + string.Join(", ", list) + "]";
    }
}
