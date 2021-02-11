// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Newtonsoft.Json;
using LuisV2 = Microsoft.Bot.Builder.AI.Luis;

namespace Microsoft.Bot.Builder.AI.LuisV3
{
    /// <summary>
    /// Defines an extension for a list entity.
    /// </summary>
    public class DynamicList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicList"/> class.
        /// </summary>
        public DynamicList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicList"/> class.
        /// </summary>
        /// <param name="entity">The name of the list entity to extend.</param>
        /// <param name="requestLists">The lists to append on the extended list entity.</param>
        public DynamicList(string entity, IList<ListElement> requestLists)
        {
            Entity = entity;
            List = requestLists;
        }

        /// <summary>
        /// Gets or sets the name of the list entity to extend.
        /// </summary>
        /// <value>
        /// The name of the list entity to extend.
        /// </value>
        [JsonProperty(PropertyName = "listEntityName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the lists to append on the extended list entity.
        /// </summary>
        /// <value>
        /// The lists to append on the extended list entity.
        /// </value>
        [JsonProperty(PropertyName = "requestLists")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public IList<ListElement> List { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails.
        /// </exception>
        public virtual void Validate()
        {
            // Required: ListEntityName, RequestLists
            if (Entity == null || List == null)
            {
                throw new Microsoft.Rest.ValidationException($"DynamicList requires Entity and List to be defined.");
            }

            foreach (var elt in List)
            {
                elt.Validate();
            }
        }
    }
}
