﻿// Licensed under the MIT License.
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
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Observers;
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

        private readonly ConcurrentDictionary<Type, ICustomDeserializer> kindDeserializers = new ConcurrentDictionary<Type, ICustomDeserializer>();
        private readonly ConcurrentDictionary<string, Type> kindToType = new ConcurrentDictionary<string, Type>();
        private readonly ConcurrentDictionary<Type, List<string>> typeToKinds = new ConcurrentDictionary<Type, List<string>>();
        private List<IResourceProvider> resourceProviders = new List<IResourceProvider>();
        private CancellationTokenSource cancelReloadToken = new CancellationTokenSource();
        private ConcurrentBag<IResource> changedResources = new ConcurrentBag<IResource>();
        private bool loaded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceExplorer"/> class.
        /// </summary>
        public ResourceExplorer()
        {
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
            RegisterComponentTypes();

            string id = resource.Id;
            if (resource is FileResource fileResource)
            {
                id = fileResource.FullName;
            }

            try
            {
                var context = new Stack<SourceRange>();
                var (json, range) = await resource.ReadTokenRangeAsync(context);
                using (new SourceContext(context, range))
                {
                    var result = Load<T>(json, context);
                    if (result is Dialog dlg)
                    {
                        // dialog id's are resource ids
                        dlg.Id = resource.Id;
                    }

                    return result;
                }
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

            var found = kindDeserializers.TryGetValue(type, out kindDeserializer);

            if (!found)
            {
                throw new ArgumentException($"Type {kind} not registered in factory.");
            }

            var built = kindDeserializer.Load(obj, serializer, type);

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
        /// <param name="context">source range context stack to build debugger source map.</param>
        /// <returns>resolved object the reference refers to.</returns>
        public async Task<JToken> ResolveRefAsync(JToken refToken, Stack<SourceRange> context)
        {
            var refTarget = GetRefTarget(refToken);

            if (string.IsNullOrEmpty(refTarget))
            {
                throw new InvalidOperationException("Failed to resolve reference, $copy property not present");
            }

            // see if there is a dialog file for this resource.id
            if (!this.TryGetResource($"{refTarget}.dialog", out IResource resource))
            {
                // if not, try loading the resource directly.
                if (!this.TryGetResource(refTarget, out resource))
                {
                    throw new FileNotFoundException($"Failed to find resource named {refTarget}.dialog or {refTarget}.");
                }
            }

            var (json, range) = await resource.ReadTokenRangeAsync(context);

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
            kindDeserializers[type] = loader;
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
                if (!this.loaded)
                {
                    // this can be reentrant, and we only want to do once.
                    this.loaded = true;

                    foreach (var component in ComponentRegistration.Registrations.Value.OfType<IComponentDeclarativeTypes>())
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

        private T Load<T>(JToken token, Stack<SourceRange> context)
        {
            var converters = new List<JsonConverter>();
            
            foreach (var component in ComponentRegistration.Registrations.Value.OfType<IComponentDeclarativeTypes>())
            {
                var result = component.GetConverters(this, context);
                if (result.Any())
                {
                    converters.AddRange(result);
                }
            }

            // Create a cycle detection observer
            var cycleDetector = new CycleDetectionObserver();

            // Register our cycle detector on the converters that support observer registration
            foreach (var observableConverter in converters.Where(c => c is IObservableConverter))
            {
                (observableConverter as IObservableConverter).RegisterObserver(cycleDetector);
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
