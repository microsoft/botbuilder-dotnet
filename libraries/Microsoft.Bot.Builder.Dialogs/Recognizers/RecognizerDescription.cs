// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Description of the intents and entities a recognizer can return.
    /// </summary>
    public class RecognizerDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecognizerDescription"/> class.
        /// </summary>
        /// <param name="intents">Intents.</param>
        /// <param name="entities">Entities.</param>
        /// <param name="dynamicLists">Dynamically defined entities</param>
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
        public IReadOnlyList<IntentDescription> Intents { get; }

        /// <summary>
        /// Gets the entities that can be recognized.
        /// </summary>
        /// <value>List of <see cref="EntityDescription"/>.</value>
        public IReadOnlyList<EntityDescription> Entities { get; }

        /// <summary>
        /// Gets a list of the dynamically defined entities that can be recognized.
        /// </summary>
        /// <value>List of <see cref="DynamicList"/>.</value>
        public IReadOnlyList<DynamicList> DynamicLists { get; }
    }
}
