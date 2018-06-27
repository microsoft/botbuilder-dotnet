// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents a set of collection of services associated with the <see cref="ITurnContext"/>.
    /// </summary>
    /// <remarks>
    /// TODO: add more details on what kind of services can/should be stored here, by whom and what the lifetime semantics are, etc.
    /// </remarks>
    public sealed class TurnContextServiceCollection : ITurnContextServiceCollection, IDisposable
    {
        private readonly Dictionary<string, object> _services = new Dictionary<string, object>();

        public TurnContextServiceCollection()
        {
        }

        public TService Get<TService>(string key) where TService : class
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if(!_services.TryGetValue(key, out var service))
            {
                // TODO: log that we didn't find the requested service
            }

            return service as TService;
        }

        public void Add<TService>(string key, TService service) where TService : class
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (service == null) throw new ArgumentNullException(nameof(service));

            try
            {
                _services.Add(key, service);
            }
            catch(ArgumentException)
            {
                throw new Exception($"A services is already registered with the specified key: {key}");
            }
        }
        
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _services.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _services.GetEnumerator();

        public void Dispose()
        {
            foreach(var entry in _services)
            {
                if(entry.Value is IDisposable disposableService)
                {
                    disposableService.Dispose();
                }
            }
        }
    }
}
