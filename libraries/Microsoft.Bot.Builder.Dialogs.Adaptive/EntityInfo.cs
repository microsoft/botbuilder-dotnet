﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Extended information about an entity including $instance data.
    /// </summary>
    /// <remarks>This is surfaced as part of the entity ambiguity events.</remarks>
    public class EntityInfo
    {
        /// <summary>
        /// Gets or sets name of entity.
        /// </summary>
        /// <value>Name of entity.</value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value of entity.
        /// </summary>
        /// <value>Value of entity.</value>
        [JsonProperty("value")]
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets where entity starts in utterance.
        /// </summary>
        /// <value>Start of entity.</value>
        [JsonProperty("start")]
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets where entity ends in utterance.
        /// </summary>
        /// <value>End of entity.</value>
        [JsonProperty("end")]
        public int End { get; set; }

        /// <summary>
        /// Gets or sets score 0-1.0 of entity.
        /// </summary>
        /// <value>Score of entity.</value>
        [JsonProperty("score")]
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets original text that led to entity.
        /// </summary>
        /// <value>Text of entity.</value>
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role the entity played in utterance.
        /// </summary>
        /// <value>Role of entity.</value>
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets type of entity.
        /// </summary>
        /// <value>Type of entity.</value>
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets relative priority of entity compared to other entities with 0 being highest priority.
        /// </summary>
        /// <value>Relative priority of entity.</value>
        [JsonProperty("priority")]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets how much 0-1.0 of the original utterance is covered by entity.
        /// </summary>
        /// <value>Coverage of entity.</value>
        [JsonProperty("coverage")]
        public double Coverage { get; set; }

        /// <summary>
        /// Gets or sets event counter when entity was recognized.
        /// </summary>
        /// <value>Event counter when entity was recognized.</value>
        [JsonProperty("whenRecognized")]
        public uint WhenRecognized { get; set; }

        /// <summary>
        /// True if entities share text in utterance.
        /// </summary>
        /// <param name="entity">Entity to compare.</param>
        /// <returns>True if entities overlap.</returns>
        public bool Overlaps(EntityInfo entity)
            => Start <= entity.End && End >= entity.Start;

        /// <summary>
        /// True if entities come from exactly the same text in the utterance.
        /// </summary>
        /// <param name="entity">Entity to compare.</param>
        /// <returns>True if entities are from the same text.</returns>
        public bool Alternative(EntityInfo entity)
            => Start == entity.Start && End == entity.End;

        /// <summary>
        /// True if entity text completely includes another entity text.
        /// </summary>
        /// <param name="entity">Entity to compare.</param>
        /// <returns>True if entity text completely covers other entity text.</returns>
        public bool Covers(EntityInfo entity)
            => Start <= entity.Start && End >= entity.End && End - Start > entity.End - entity.Start;

        public override string ToString()
            => $"{Name}:{Value} P{Priority} {Score} {Coverage}";
    }
}
