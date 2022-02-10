// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Internal cached bot state.
    /// </summary>
    public class CachedBotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedBotState"/> class.
        /// </summary>
        /// <param name="state">Initial state for the <see cref="CachedBotState"/>.</param>
        public CachedBotState(IDictionary<string, object> state = null)
        {
            State = state ?? new Dictionary<string, object>();
            Hash = ComputeHash(State);
        }

        /// <summary>
        /// Gets the state as a dictionary of key value pairs.
        /// </summary>
        /// <value>
        /// The state as a dictionary of key value pairs.
        /// </value>
        public IDictionary<string, object> State { get; }

        internal string Hash { get; set; }

        internal static string ComputeHash(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        internal bool IsChanged()
        {
            return Hash != ComputeHash(State);
        }
    }
}
