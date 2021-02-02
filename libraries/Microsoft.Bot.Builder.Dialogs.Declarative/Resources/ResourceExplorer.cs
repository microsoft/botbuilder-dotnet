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
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Observers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Class which gives standard access to content resources.
    /// </summary>
    public class ResourceExplorer : IDisposable
    {
        private const string RefPropertyName = "$copy";

        private readonly ConcurrentDictionary<string, ICustomDeserializer> kindDeserializers = new ConcurrentDictionary<string, ICustomDeserializer>();
        private readonly ConcurrentDictionary<string, Type> kindToType = new ConcurrentDictionary<string, Type>();
        private readonly ConcurrentDictionary<Type, List<string>> typeToKinds = new ConcurrentDictionary<Type, List<string>>();
        private readonly ResourceExplorerOptions options;

        private List<ResourceProvider> resourceProviders = new List<ResourceProvider>();
        private List<IComponentDeclarativeTypes> declarativeTypes;
        private CancellationTokenSource cancelReloadToken = new CancellationTokenSource();
        private ConcurrentBag<Resource> changedResources = new ConcurrentBag<Resource>();
        private bool typesLoaded = false;

        // To detect redundant calls to dispose
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceExplorer"/> class.
        /// </summary>
        public ResourceExplorer()
            : this(new ResourceExplorerOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceExplorer"/> class.
        /// </summary>
        /// <param name="providers">The list of resource providers to initialize the current instance.</param>
        public ResourceExplorer(IEnumerable<ResourceProvider> providers)
            : this(new ResourceExplorerOptions() { Providers = providers })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceExplorer"/> class.
        /// </summary>
        /// <param name="providers">The list of resource providers to initialize the current instance.</param>
        /// <param name="declarativeTypes">A list of declarative types to use. Falls back to <see cref="ComponentRegistration.Components" /> if set to null.</param>
        public ResourceExplorer(IEnumerable<ResourceProvider> providers, IEnumerable<IComponentDeclarativeTypes> declarativeTypes)
            : this(new ResourceExplorerOptions() { Providers = providers, DeclarativeTypes = declarativeTypes })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceExplorer"/> class.
        /// </summary>
        /// <param name="options">Configuration optiosn for <see cref="ResourceExplorer"/>.</param>
        public ResourceExplorer(ResourceExplorerOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            if (options.Providers != null)
            {
                this.resourceProviders = options.Providers.ToList();
            }

            if (options.DeclarativeTypes != null)
            {
                this.declarativeTypes = options.DeclarativeTypes.ToList();
            }
        }

        /// <summary>
        /// Event which fires when a resource is changed.
        /// </summary>
        public event EventHandler<IEnumerable<Resource>> Changed;

        /// <summary>
        /// Gets the resource providers.
        /// </summary>
        /// <value>
        /// The resource providers.
        /// </value>
        public IEnumerable<ResourceProvider> ResourceProviders
        {
            get { return this.resourceProviders; }
        }

        /// <summary>
        /// Gets the resource type id extensions that you want to manage.
        /// </summary>
        /// <value>
        /// The extensions that you want the to manage.
        /// </value>
        public HashSet<string> ResourceTypes { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dialog",
            "lu",
            "lg",
            "qna",
            "schema",
            "json"
        };

        /// <summary>
        /// Add a resource Type to resource list.
        /// </summary>
        /// <param name="type">resource type.</param>
        public void AddResourceType(string type)
        {
            type = type.TrimStart('.');
            if (!ResourceTypes.Contains(type))
            {
                ResourceTypes.Add(type);
                Refresh();
            }
        }

        /// <summary>
        /// Reload any cached data.
        /// </summary>
        public void Refresh()
        {
            foreach (var resourceProvider in resourceProviders)
            {
                resourceProvider.Refresh();
            }
        }

        /// <summary>
        /// Add a resource provider to the resources managed by the resource explorer.
        /// </summary>
        /// <param name="resourceProvider">resource provider.</param>
        /// <returns>resource explorer so that you can fluently call multiple methods on the resource explorer.</returns>
        public ResourceExplorer AddResourceProvider(ResourceProvider resourceProvider)
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
        public T LoadType<T>(Resource resource)
        {
            return LoadTypeAsync<T>(resource).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create Type from resource.
        /// </summary>
        /// <typeparam name="T">type to create.</typeparam>
        /// <param name="resource">resource to bind to.</param>
        /// <param name="cancellationToken">the <see cref="CancellationToken"/> for the task.</param>
        /// <returns>task which will resolve to created type.</returns>
#pragma warning disable CA1801 // Review unused parameters (we can't remove cancellationToken without breaking binary compat)
        public async Task<T> LoadTypeAsync<T>(Resource resource, CancellationToken cancellationToken = default)
#pragma warning restore CA1801 // Review unused parameters
        {
            RegisterComponentTypes();

            string id = resource.Id;

            try
            {
                var sourceContext = new ResourceSourceContext();
                var (jToken, range) = await ReadTokenRangeAsync(resource, sourceContext).ConfigureAwait(false);

                using (new SourceScope(sourceContext, range))
                {
                    var result = Load<T>(jToken, sourceContext);
                    return result;
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception err)
            {
                if (err.InnerException is SyntaxErrorException)
                {
                    throw new SyntaxErrorException(err.InnerException.Message, err)
                    {
                        Source = $"{id}{err.InnerException.Source}"
                    };
                }

                throw new InvalidOperationException($"{id} error: {err.Message}\n{err.InnerException?.Message}", err);
            }
        }

        /// <summary>
        /// Get resources of a given type.
        /// </summary>
        /// <param name="fileExtension">File extension filter.</param>
        /// <returns>The resources.</returns>
        public IEnumerable<Resource> GetResources(string fileExtension)
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
        public Resource GetResource(string id)
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
        public bool TryGetResource(string id, out Resource resource)
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
            RegisterComponentTypes();
            RegisterTypeInternal(kind, type, loader);
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
            ICustomDeserializer kindDeserializer;
            var type = GetTypeForKind(kind);

            if (type == null)
            {
                throw new ArgumentException($"Type {kind} not registered in factory.");
            }

            var found = kindDeserializers.TryGetValue(kind, out kindDeserializer);

            if (!found)
            {
                throw new ArgumentException($"Type {kind} not registered in factory.");
            }

            var built = kindDeserializer.Load(obj, serializer, type);

            var result = built as T;

            if (result == null)
            {
                throw new ArgumentException($"Factory registration for name {kind} resulted in type {built.GetType()}, but expected assignable to {typeof(T)}");
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
            RegisterComponentTypes();
            return kindToType.TryGetValue(kind, out Type type) ? type : default(Type);
        }

        /// <summary>
        /// Get the $kind for a type.
        /// </summary>
        /// <param name="type">type.</param>
        /// <returns>$kind for the type.</returns>
        public List<string> GetKindsForType(Type type)
        {
            RegisterComponentTypes();
            return typeToKinds.TryGetValue(type, out List<string> kinds) ? kinds : null;
        }

        /// <summary>
        /// Get the $kind for a type.
        /// </summary>
        /// <typeparam name="T">type.</typeparam>
        /// <returns>$kind for the type.</returns>
        public List<string> GetKindsForType<T>()
        {
            return GetKindsForType(typeof(T));
        }

        /// <summary>
        /// Dispose of internal resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
        /// <param name="sourceContext">source context to build debugger source map.</param>
        /// <param name="cancellationToken">the <see cref="CancellationToken"/> for the task.</param>
        /// <returns>resolved object the reference refers to.</returns>
#pragma warning disable CA1801 // Review unused parameters (we can't remove cancellationToken without breaking binary compat)
        public async Task<JToken> ResolveRefAsync(JToken refToken, SourceContext sourceContext, CancellationToken cancellationToken = default)
#pragma warning restore CA1801 // Review unused parameters
        {
            var refTarget = GetRefTarget(refToken);

            if (string.IsNullOrEmpty(refTarget))
            {
                throw new InvalidOperationException("Failed to resolve reference, $copy property not present");
            }

            // see if there is a dialog file for this resource.id
            if (!this.TryGetResource($"{refTarget}.dialog", out Resource resource))
            {
                // if not, try loading the resource directly.
                if (!this.TryGetResource(refTarget, out resource))
                {
                    throw new FileNotFoundException($"Failed to find resource named {refTarget}.dialog or {refTarget}.");
                }
            }

            // When we load a reference and then pass the JsonReader to the converters, 
            // the JsonReader needs to be in the same state as when received from the 
            // Json.Net runtime. Otherwise, the source ranges will look different and not look like
            // the same content, introducing potential bugs.
            // We explicitly pass advance: true in this case to mimic the behavior in Json.Net
            // Json.Net: https://github.com/JamesNK/Newtonsoft.Json/blob/9be95e0f6aedf5cdc108b058bf71f0839911e0c9/Src/Newtonsoft.Json/Serialization/JsonSerializerInternalReader.cs#L1791
            var (json, range) = await ReadTokenRangeAsync(resource, sourceContext, advanceJsonReader: true).ConfigureAwait(false);

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

            // if we have a source range for the resource, then make it available to InterfaceConverter
            DebugSupport.SourceMap.Add(json, range);

            return json;
        }

        /// <summary>
        /// Reads a <see cref="JToken"/> and <see cref="SourceRange"/> for a given resource.
        /// </summary>
        /// <param name="resource"><see cref="Resource"/> to read.</param>
        /// <param name="sourceContext"><see cref="SourceContext"/> for the current operation.</param>
        /// <param name="advanceJsonReader">Whether to advance the <see cref="JsonReader"/>.</param>
        /// <returns>The resulting <see cref="JToken"/> and <see cref="SourceRange"/> for the requested resource.</returns>
        internal async Task<(JToken, SourceRange)> ReadTokenRangeAsync(Resource resource, SourceContext sourceContext, bool advanceJsonReader = false)
        {
            var text = await resource.ReadTextAsync().ConfigureAwait(false);
            using (var readerText = new StringReader(text))
            using (var readerJson = new JsonTextReader(readerText))
            {
                if (advanceJsonReader)
                {
                    // Mimic the state in which Json.Net passes the readers. In some scenarios,
                    // Json.Net advances de readers by one.
                    // Example: https://github.com/JamesNK/Newtonsoft.Json/blob/9be95e0f6aedf5cdc108b058bf71f0839911e0c9/Src/Newtonsoft.Json/Serialization/JsonSerializerInternalReader.cs#L1791
                    readerJson.Read(); // Move to first token
                }

                var (token, range) = SourceScope.ReadTokenRange(readerJson, sourceContext);

                AutoAssignId(resource, token, sourceContext, range);
                range.Path = resource.FullName ?? resource.Id;
                return (token, range);
            }
        }

        /// <summary>
        /// Disposes objected used by the class.
        /// </summary>
        /// <param name="disposing">A Boolean that indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false).</param>
        /// <remarks>
        /// The disposing parameter should be false when called from a finalizer, and true when called from the IDisposable.Dispose method.
        /// In other words, it is true when deterministically called and false when non-deterministically called.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed objects owned by the class here.
                foreach (var resource in this.resourceProviders)
                {
                    if (resource is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                cancelReloadToken.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Handler for on changed events.
        /// </summary>
        /// <param name="resources">A collection of resources to pass to the event handler.</param>
        protected virtual void OnChanged(Resource[] resources)
        {
            Changed?.Invoke(this, resources);
        }

        private IEnumerable<IComponentDeclarativeTypes> GetComponentRegistrations()
        {
            return this.declarativeTypes ?? ComponentRegistration.Components.OfType<IComponentDeclarativeTypes>();
        }

        private void RegisterTypeInternal(string kind, Type type, ICustomDeserializer loader = null)
        {
            // Default loader if none specified
            if (loader == null)
            {
                loader = new DefaultLoader();
            }

            kindToType[kind] = type;

            if (!typeToKinds.TryGetValue(type, out List<string> kinds))
            {
                kinds = new List<string>();
            }

            kinds.Add(kind);
            typeToKinds[type] = kinds;
            kindDeserializers[kind] = loader;
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

        /// <summary>
        /// Register all types from components.
        /// </summary>
        private void RegisterComponentTypes()
        {
            lock (this.kindToType)
            {
                if (!this.typesLoaded)
                {
                    // this can be reentrant, and we only want to do once.
                    this.typesLoaded = true;

                    foreach (var component in GetComponentRegistrations())
                    {
                        if (component != null)
                        {
                            // add types
                            foreach (var typeRegistration in component.GetDeclarativeTypes(this))
                            {
                                RegisterTypeInternal(typeRegistration.Kind, typeRegistration.Type, typeRegistration.CustomDeserializer);
                            }
                        }
                    }
                }
            }
        }

        private T Load<T>(JToken token, SourceContext sourceContext)
        {
            var converters = new List<JsonConverter>();

            // Get converters
            foreach (var component in GetComponentRegistrations())
            {
                var result = component.GetConverters(this, sourceContext);
                if (result.Any())
                {
                    converters.AddRange(result);
                }
            }

            // Create a cycle detection observer
            var cycleDetector = new CycleDetectionObserver(options.AllowCycles);

            // Register our cycle detector on the converters that support observer registration
            foreach (var observableConverter in converters.Where(c => c is IObservableJsonConverter))
            {
                (observableConverter as IObservableJsonConverter).RegisterObserver(cycleDetector);
            }

            var serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = converters,
                Error = (sender, args) =>
                {
                    var ctx = args.ErrorContext;
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                }
            });

            // Pass 1 of cycle detection. This pass fills the cycle detector cache excluding cycles.
            var pass1Result = token.ToObject<T>(serializer);

            cycleDetector.CycleDetectionPass = CycleDetectionPasses.PassTwo;

            // Pass 2 of cycle detection. This pass stitches objects from the cache into the places
            // where we found cycles.

            return token.ToObject<T>(serializer);
        }

        private void ResourceProvider_Changed(object sender, IEnumerable<Resource> resources)
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
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler (this code looks problematic but excluding the rule for now, we need to analyze threading and re-entrance in more detail before trying to change it).
                    Task.Delay(1000, cancelReloadToken.Token)
                        .ContinueWith(t =>
                        {
                            if (t.IsCanceled)
                            {
                                return;
                            }

                            var changed = changedResources.ToArray();
                            changedResources = new ConcurrentBag<Resource>();
                            this.OnChanged(changed);
                        }).ContinueWith(t => t.Status);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
                }
            }
        }

        private void AutoAssignId(Resource resource, JToken jToken, SourceContext sourceContext, SourceRange range)
        {
            if (sourceContext is ResourceSourceContext resourceSourceContext)
            {
                if (jToken is JObject jObj && !jObj.ContainsKey("id"))
                {
                    jObj["id"] = resource.Id;
                    range.EndPoint.LineIndex += 1;

                    if (!resourceSourceContext.DefaultIdMap.ContainsKey(jToken))
                    {
                        resourceSourceContext.DefaultIdMap.Add(jToken, resource.Id);
                    }
                }
            }
        }
    }
}
