// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents a set of collection of services associated with the <see cref="ITurnContext"/>.
    /// </summary>
    /// <remarks>
    /// TODO: add more details on what kind of services can/should be stored here, by whom and what the lifetime semantics are, etc.
    /// </remarks>
    public sealed class TurnContextServiceCollection : Dictionary<string, object>, IDisposable
    {
        public TurnContextServiceCollection()
        {
        }

        public TService Get<TService>(string key) where TService : class
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if(TryGetValue(key, out var service))
            {
                if (service is TService result)
                {
                    return result;
                }
            }

            // return null if either the key or type don't match
            return null;
        }

        public TService Get<TService>() where TService : class
        {
            return Get<TService>(typeof(TService).FullName);
        }

        public void Add<TService>(string key, TService service) where TService : class
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (service == null) throw new ArgumentNullException(nameof(service));

            // note this can throw with a duplicate key
            base.Add(key, service);
        }

        public void Add<TService>(TService service) where TService : class
        {
            Add(typeof(TService).FullName, service);
        }

        public void Dispose()
        {
            foreach(var entry in Values)
            {
                if(entry is IDisposable disposableService)
                {
                    disposableService.Dispose();
                }
            }
        }
    }
}
