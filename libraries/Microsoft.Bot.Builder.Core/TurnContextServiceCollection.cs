using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents a set of collection of services associated with the <see cref="ITurnContext"/>.
    /// </summary>
    /// <remarks>
    /// TODO: add more details on what kind of services can/should be stored here, by whom and what the lifetime semantics are, etc.
    /// </remarks>
    public interface ITurnContextServiceCollection : IEnumerable<KeyValuePair<string, object>>
    {
        /// <summary>
        /// Add a service with a specified key.
        /// </summary>
        /// <typeparam name="TService">The type of service to be added.</typeparam>
        /// <param name="key">The key to store the service under.</param>
        /// <param name="service">The service to add.</param>
        /// <exception cref="ServiceKeyAlreadyRegisteredException">Thrown when a service is already registered with the specified <paramref name="key"/></exception>
        void Add<TService>(string key, TService service) where TService : class;

        /// <summary>
        /// Get a service by its key.
        /// </summary>
        /// <typeparam name="TService">The type of service to be retrieved.</typeparam>
        /// <param name="key">The key of the service to get.</param>
        /// <returns>The service stored under the specified key.</returns>
        TService Get<TService>(string key) where TService : class;
    }

    public sealed class TurnContextServiceCollection : ITurnContextServiceCollection
    {
        private readonly Dictionary<string, object> _services = new Dictionary<string, object>();

        public TService Get<TService>(string key) where TService : class
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            _services.TryGetValue(key, out var service);

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
                throw new ServiceKeyAlreadyRegisteredException(key);
            }
        }
        
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _services.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _services.GetEnumerator();
    }

    /// <summary>
    /// Thrown to indicate a service is already registered in a <see cref="ITurnContextServiceCollection"/> under the specified key.
    /// </summary>
    [Serializable]
    public class ServiceKeyAlreadyRegisteredException : Exception
    {
        public ServiceKeyAlreadyRegisteredException(string key) : base($"A services is already registered with the specified key: {key}")
        {
        }

        protected ServiceKeyAlreadyRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// Provides a set of convienience methods that extend the behavior of any <see cref="ITurnContextServiceCollection"/>. 
    /// </summary>
    public static class TurnContextServiceCollectionExtensions
    {
        
        /// <summary>
        /// Add a service using its full type name as the key.
        /// </summary>
        /// <typeparam name="TService">The type of service to be added.</typeparam>
        /// <param name="service">The service to add.</param>
        public static void Add<TService>(this ITurnContextServiceCollection serviceCollection, TService service) where TService : class =>
            serviceCollection.Add(typeof(TService).FullName, service);

        /// <summary>
        /// Get a service by type using its full type name as the key.
        /// </summary>
        /// <typeparam name="TService">The type of service to be retrieved.</typeparam>
        /// <returns>The service stored under the specified key.</returns>
        public static TService Get<TService>(this ITurnContextServiceCollection serviceCollection) where TService : class =>
            serviceCollection.Get<TService>(typeof(TService).FullName);


        /// <summary>
        /// Returns all entries in the collection of a specified type.
        /// </summary>
        /// <typeparam name="TService">The type of service to be found.</typeparam>
        /// <param name="serviceCollection">An <see cref="ITurnContextServiceCollection"/> to search for services in.</param>
        /// <returns>All instances of the requested service currently stored in the collection.</returns>
        public static IEnumerable<KeyValuePair<string, TService>> GetServices<TService>(this ITurnContextServiceCollection serviceCollection) where TService : class
        {
            foreach(var entry in serviceCollection)
            {
                if(entry.Value is TService service)
                {
                    yield return new KeyValuePair<string, TService>(entry.Key, service);
                }
            }
        }
    }
}
