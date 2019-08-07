// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Defines a user predicted entity that extends an already existing one.
    /// </summary>
    public class ExternalEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalEntity"/> class.
        /// </summary>
        public ExternalEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalEntity"/> class.
        /// </summary>
        /// <param name="entity">The name of the entity to extend.</param>
        /// <param name="start">The start character index of the predicted entity.</param>
        /// <param name="length">The length of the predicted entity.</param>
        /// <param name="resolution">A user supplied custom resolution to return as the entity's prediction.</param>
        public ExternalEntity(string entity, int start, int length, object resolution = null)
        {
            Entity = entity;
            Start = start;
            Length = length;
            Resolution = resolution;
        }

        /// <summary>
        /// Gets or sets the name of the entity to extend.
        /// </summary>
        /// <value>
        /// The name of the entity to extend.
        /// </value>
        [JsonProperty(PropertyName = "entityName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the start character index of the predicted entity.
        /// </summary>
        /// <value>
        /// The start character index of the predicted entity.
        /// </value>
        [JsonProperty(PropertyName = "startIndex")]
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the length of the predicted entity.
        /// </summary>
        /// <value>
        /// The length of the predicted entity.
        /// </value>
        [JsonProperty(PropertyName = "entityLength")]
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets a user supplied custom resolution to return as the entity's prediction.
        /// </summary>
        /// <value>
        /// A user supplied custom resolution to return as the entity's prediction.
        /// </value>
        [JsonProperty(PropertyName = "resolution")]
        public object Resolution { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails.
        /// </exception>
        public virtual void Validate()
        {
            if (Entity == null || Length == 0)
            {
                throw new Microsoft.Rest.ValidationException($"ExternalEntity requires an EntityName and EntityLength > 0");
            }
        }
    }
}
