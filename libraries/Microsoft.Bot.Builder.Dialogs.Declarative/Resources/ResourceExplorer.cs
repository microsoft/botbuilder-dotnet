// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public delegate void ResourceChangedEventHandler(IResource[] resources);

    /// <summary>
    /// Class which gives standard access to content resources.
    /// </summary>
    public class ResourceExplorer : IDisposable
    {
        private const string RefPropertyName = "$copy";

        private readonly List<JsonConverter> converters = new List<JsonConverter>();
        private readonly Dictionary<Type, ICustomDeserializer> builders = new Dictionary<Type, ICustomDeserializer>();
        private readonly Dictionary<string, Type> kinds = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> names = new Dictionary<Type, string>();
        private List<IResourceProvider> resourceProviders = new List<IResourceProvider>();
        private CancellationTokenSource cancelReloadToken = new CancellationTokenSource();
        private ConcurrentBag<IResource> changedResources = new ConcurrentBag<IResource>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceExplorer"/> class.
        /// </summary>
        public ResourceExplorer()
        {
            foreach (var component in ComponentRegistration.Registrations.Value.OfType<IComponentDeclarativeTypes>())
            {
                if (component != null)
                {
                    // add types
                    foreach (var typeRegistration in component.GetDeclarativeTypes())
                    {
                        RegisterType(typeRegistration.Kind, typeRegistration.Type, typeRegistration.CustomDeserializer);
                    }
                }
            }
        }

        public ResourceExplorer(IEnumerable<IResourceProvider> providers)
            : this()
        {
            this.resourceProviders = providers.ToList();
        }

        /// <summary>
        /// Event which fires when a resource is changed.
        /// </summary>
        public event ResourceChangedEventHandler Changed;

        /// <summary>
        /// Gets the resource providers.
        /// </summary>
        /// <value>
        /// The resource providers.
        /// </value>
        public IEnumerable<IResourceProvider> ResourceProviders
        {
            get { return this.resourceProviders; }
        }

        /// <summary>
        /// Add a resource provider to the resources managed by the resource explorer.
        /// </summary>
        /// <param name="resourceProvider">resource provider.</param>
        /// <returns>resource explorer so that you can fluently call multiple methods on the resource explorer.</returns>
        public ResourceExplorer AddResourceProvider(IResourceProvider resourceProvider)
        {
            resourceProvider.Changed += ResourceProvider_Changed;

            if (this.resourceProviders.Any(r => r.Id == resourceProvider.Id))
            {
                throw new ArgumentException($"{resourceProvider.Id} has already been added as a resource");
            }

            this.resourceProviders.Add(resourceProvider);
            return this;
        }

        /// <summary>
        /// Create Type from resource.
        /// </summary>
        /// <typeparam name="T">type to create.</typeparam>
        /// <param name="resourceId">resourceId to bind to.</param>
        /// <returns>created type.</returns>
        public T LoadType<T>(string resourceId)
        {
            return this.LoadType<T>(this.GetResource(resourceId));
        }

        /// <summary>
        /// Create Type from resource.
        /// </summary>
        /// <typeparam name="T">type to create.</typeparam>
        /// <param name="resource">resource to bind to.</param>
        /// <returns>created type.</returns>
        public T LoadType<T>(IResource resource)
        {
            return LoadTypeAsync<T>(resource).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create Type from resource.
        /// </summary>
        /// <typeparam name="T">type to create.</typeparam>
        /// <param name="resource">resource to bind to.</param>
        /// <returns>task which will resolve to created type.</returns>
        public async Task<T> LoadTypeAsync<T>(IResource resource)
        {
            string id = resource.Id;
            var paths = new Stack<string>();
            if (resource is FileResource fileResource)
            {
                id = fileResource.FullName;
                paths.Push(fileResource.FullName);
            }

            string json = null;
            try
            {
                json = await resource.ReadTextAsync();

                var result = Load<T>(json, paths);
                if (result is Dialog dlg)
                {
                    // dialog id's are resource ids
                    dlg.Id = resource.Id;
                }

                return result;
            }
            catch (Exception err)
            {
                if (err.InnerException is SyntaxErrorException)
                {
                    throw new SyntaxErrorException(err.InnerException.Message)
                    {
                        Source = $"{id}{err.InnerException.Source}"
                    };
                }

                throw new Exception($"{id} error: {err.Message}\n{err.InnerException?.Message}");
            }
        }

        /// <summary>
        /// Get resources of a given type.
        /// </summary>
        /// <param name="fileExtension">File extension filter.</param>
        /// <returns>The resources.</returns>
        public IEnumerable<IResource> GetResources(string fileExtension)
        {
            foreach (var resourceProvider in this.resourceProviders)
            {
                foreach (var resource in resourceProvider.GetResources(fileExtension))
                {
                    yield return resource;
                }
            }
        }

        /// <summary>
        /// Get resource by id.
        /// </summary>
        /// <param name="id">The resource id.</param>
        /// <returns>The resource, or throws if not found.</returns>
        public IResource GetResource(string id)
        {
            if (TryGetResource(id, out var resource))
            {
                return resource;
            }

            throw new ArgumentException($"Could not find resource '{id}'", paramName: id);
        }

        /// <summary>
        /// Try to get the resource by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="resource">resource that was found or null.</param>
        /// <returns>true if found.</returns>
        public bool TryGetResource(string id, out IResource resource)
        {
            foreach (var resourceProvider in this.resourceProviders)
            {
                if (resourceProvider.TryGetResource(id, out resource))
                {
                    return true;
                }
            }

            resource = null;
            return false;
        }

        /// <summary>
        /// Register a declarative type with the resource loader system.
        /// </summary>
        /// <typeparam name="T">type of object to create.</typeparam>
        /// <param name="kind">the $kind name to map to this type.</param>
        /// <param name="loader">optional custom deserializer.</param>
        /// <returns>Resource explorer for fluent style multiple calls.</returns>
        public ResourceExplorer RegisterType<T>(string kind, ICustomDeserializer loader = null)
        {
            return RegisterType(kind, typeof(T), loader);
        }

        /// <summary>
        /// Register a declarative type with the resource loader system.
        /// </summary>
        /// <param name="kind">the $kind name to map to this type.</param>
        /// <param name="type">type of object to create.</param>
        /// <param name="loader">optional custom deserializer.</param>
        /// <returns>Resource explorer for fluent style multiple calls.</returns>
        public ResourceExplorer RegisterType(string kind, Type type, ICustomDeserializer loader = null)
        {
            // Default loader if none specified
            if (loader == null)
            {
                loader = new DefaultLoader();
            }

            lock (kinds)
            {
                kinds[kind] = type;
            }

            lock (names)
            {
                names[type] = kind;
            }

            lock (builders)
            {
                builders[type] = loader;
            }

            return this;
        }

        /// <summary>
        /// Build type for given $kind using the JToken/serializer as the source.
        /// </summary>
        /// <typeparam name="T">type of object to create.</typeparam>
        /// <param name="kind">$kind.</param>
        /// <param name="obj">source object.</param>
        /// <param name="serializer">serializer to user.</param>
        /// <returns>instantiated object of type(T).</returns>
        public T BuildType<T>(string kind, JToken obj, JsonSerializer serializer)
            where T : class
        {
            ICustomDeserializer builder;
            var type = GetTypeForKind(kind);

            if (type == null)
            {
                throw new ArgumentException($"Type {kind} not registered in factory.");
            }

            var found = builders.TryGetValue(type, out builder);

            if (!found)
            {
                throw new ArgumentException($"Type {kind} not registered in factory.");
            }

            var built = builder.Load(obj, serializer, type);

            var result = built as T;

            if (result == null)
            {
                throw new Exception($"Factory registration for name {kind} resulted in type {built.GetType()}, but expected assignable to {typeof(T)}");
            }

            return result;
        }

        /// <summary>
        /// Get the type for $kind.
        /// </summary>
        /// <param name="kind">$kind.</param>
        /// <returns>type of object.</returns>
        public Type GetTypeForKind(string kind)
        {
            Type type;
            return kinds.TryGetValue(kind, out type) ? type : default(Type);
        }

        /// <summary>
        /// Get the $kind for a type.
        /// </summary>
        /// <param name="type">type.</param>
        /// <returns>$kind for the type.</returns>
        public string GetKindForType(Type type)
        {
            string name;
            return names.TryGetValue(type, out name) ? name : default(string);
        }

        /// <summary>
        /// Get the $kind for a type.
        /// </summary>
        /// <typeparam name="T">type.</typeparam>
        /// <returns>$kind for the type.</returns>
        public string GetKindForType<T>()
        {
            string name;
            return names.TryGetValue(typeof(T), out name) ? name : default(string);
        }

        /// <summary>
        /// Dispose of internal resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var resource in this.resourceProviders)
            {
                if (resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// Determine if token is a reference.
        /// </summary>
        /// <param name="token">jtoken.</param>
        /// <returns>true if it is string ref.</returns>
        public bool IsRef(JToken token)
        {
            return !string.IsNullOrEmpty(GetRefTarget(token));
        }

        /// <summary>
        /// Resolves a ref to the actual object.
        /// </summary>
        /// <param name="refToken">reference.</param>
        /// <returns>resolved object the reference refers to.</returns>
        public async Task<JToken> ResolveRefAsync(JToken refToken)
        {
            var refTarget = GetRefTarget(refToken);

            if (string.IsNullOrEmpty(refTarget))
            {
                throw new InvalidOperationException("Failed to resolve reference, $copy property not present");
            }

            var resource = this.GetResource($"{refTarget}.dialog");
            if (resource == null)
            {
                throw new FileNotFoundException($"Failed to find resource named {refTarget}.dialog");
            }

            string text = await resource.ReadTextAsync().ConfigureAwait(false);
            var json = JToken.Parse(text);

            foreach (JProperty prop in refToken.Children<JProperty>())
            {
                if (prop.Name != "$ref")
                {
                    // JToken is an object, so we merge objects
                    if (json[prop.Name] != null && json[prop.Name].Type == JTokenType.Object)
                    {
                        JObject targetProperty = json[prop.Name] as JObject;
                        targetProperty.Merge(prop.Value);
                    }

                    // JToken is an object, so we merge objects
                    else if (json[prop.Name] != null && json[prop.Name].Type == JTokenType.Array)
                    {
                        JArray targetArray = json[prop.Name] as JArray;
                        targetArray.Merge(prop.Value);
                    }

                    // JToken is a value, simply assign
                    else
                    {
                        json[prop.Name] = prop.Value;
                    }
                }
            }

            // if we have a source path for the resource, then make it available to InterfaceConverter
            if (resource is FileResource fileResource)
            {
                DebugSupport.SourceMap.Add(json, new SourceRange() { Path = fileResource.FullName });
            }

            return json;
        }

        private string GetRefTarget(JToken token)
        {
            // If we expect an instance of IMyInterface and we find a string,
            // we assume that it is an implicit reference
            if (token.Type == JTokenType.String)
            {
                return token.Value<string>();
            }

            // Else try to get a reference from the token
            return token?
                .Children<JProperty>()
                .FirstOrDefault(jProperty => jProperty.Name == RefPropertyName)
                ?.Value.ToString();
        }

        private T Load<T>(string json, Stack<string> paths)
        {
            var converters = new List<JsonConverter>();
            foreach (var component in ComponentRegistration.Registrations.Value.OfType<IComponentDeclarativeTypes>())
            {
                var result = component.GetConverters(this, paths);
                if (result.Any())
                {
                    converters.AddRange(result);
                }
            }

            return JsonConvert.DeserializeObject<T>(
                json, new JsonSerializerSettings()
                {
                    SerializationBinder = new UriTypeBinder(this),
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = converters,
                    Error = (sender, args) =>
                    {
                        var ctx = args.ErrorContext;
                    },
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                });
        }

        private void ResourceProvider_Changed(IResource[] resources)
        {
            if (this.Changed != null)
            {
                foreach (var resource in resources)
                {
                    changedResources.Add(resource);
                }

                lock (cancelReloadToken)
                {
                    cancelReloadToken.Cancel();
                    cancelReloadToken = new CancellationTokenSource();
                    Task.Delay(1000, cancelReloadToken.Token)
                        .ContinueWith(t =>
                        {
                            if (t.IsCanceled)
                            {
                                return;
                            }

                            var changed = changedResources.ToArray();
                            changedResources = new ConcurrentBag<IResource>();
                            this.Changed(changed);
                        }).ContinueWith(t => t.Status);
                }
            }
        }
    }
}
