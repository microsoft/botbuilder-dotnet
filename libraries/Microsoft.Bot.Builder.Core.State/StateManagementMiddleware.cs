using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.State
{
    public class StateManagementMiddleware : IMiddleware
    {
        public const string DefaultStateStoreName = nameof(StateManagementMiddleware) + ".DefaultStateStore";

        private Dictionary<string, IStateStorageProvider> _configuredStateStorageProviders = new Dictionary<string, IStateStorageProvider>();
        private Dictionary<string, StateManagerConfiguration> _configuredStateManagers = new Dictionary<string, StateManagerConfiguration>();

        private bool _autoLoadEnabled;
        private bool _autoLoadAll;
        private bool _autoSaveEnabled;
        private bool _autoSaveAll;

        public StateManagementMiddleware()
        {
        }

        public StateManagementMiddleware UseStorageProvider(string name, IStateStorageProvider provider)
        {
            _configuredStateStorageProviders[name] = provider ?? throw new ArgumentNullException(nameof(provider));

            return this;
        }

        public StateManagementMiddleware UseState(Type stateManagerType, Action<StateManagerConfigurationBuilder> configure = null)
        {
            if (stateManagerType == null)
            {
                throw new ArgumentNullException(nameof(stateManagerType));
            }

            if (!typeof(IStateManager).IsAssignableFrom(stateManagerType))
            {
                throw new ArgumentException($"Specified type is not a subtype of {typeof(IStateManager).Name}.", nameof(stateManagerType));
            }

            if (stateManagerType == typeof(StateManager))
            {
                throw new ArgumentException($"Cannot register the standard {typeof(StateManager).Name}; only custom state managers are supported.", nameof(stateManagerType));
            }

            return UseState(new StateManagerConfiguration
            {
                StateNamespace = BuildStateManagerNamespaceForType(stateManagerType),
                StateManagerType = stateManagerType,
                StateStoreName = DefaultStateStoreName,
            },
            configure);
        }

        public StateManagementMiddleware UseState(string stateNamespace, Action<StateManagerConfigurationBuilder> configure = null) =>
            UseState(new StateManagerConfiguration
            {
                StateNamespace = stateNamespace,
                StateStoreName = DefaultStateStoreName,
            },
            configure);

        private StateManagementMiddleware UseState(StateManagerConfiguration stateManagerConfiguration, Action<StateManagerConfigurationBuilder> configure = null)
        {
            if (stateManagerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(stateManagerConfiguration));
            }

            configure?.Invoke(new StateManagerConfigurationBuilder(stateManagerConfiguration));

            _configuredStateManagers[stateManagerConfiguration.StateNamespace] = stateManagerConfiguration;

            _autoLoadEnabled = stateManagerConfiguration.AutoLoad;

            return this;
        }

        public StateManagementMiddleware AutoLoadAll()
        {
            _autoLoadEnabled = true;
            _autoLoadAll = true;

            return this;
        }

        public StateManagementMiddleware AutoSaveAll()
        {
            _autoSaveEnabled = true;
            _autoSaveAll = true;

            return this;
        }

        public async Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            var stateManagerResolver = new StateManagerServiceResolver(context, _configuredStateStorageProviders, _configuredStateManagers);
            context.Services.Add<IStateManagerServiceResolver>(stateManagerResolver);

            await AutoLoadIfEnabled();

            await next();

            await AutoSaveIfEnabled();

            Task AutoLoadIfEnabled()
            {
                if (!_autoLoadEnabled)
                {
                    return Task.CompletedTask;
                }

                var loadTasks = new List<Task>();

                foreach (var stateManagerConfiguration in _configuredStateManagers.Values)
                {
                    if (_autoLoadAll || stateManagerConfiguration.AutoLoad)
                    {
                        var stateManager = stateManagerResolver.ResolveStateManager(stateManagerConfiguration);

                        var loadTask = default(Task);

                        if (_autoLoadAll || stateManagerConfiguration.AutoLoadSpecificKeys == null)
                        {
                            loadTask = stateManager.LoadAll();
                        }
                        else
                        {
                            loadTask = stateManager.Load(stateManagerConfiguration.AutoLoadSpecificKeys);
                        }

                        loadTasks.Add(loadTask);
                    }
                }

                return Task.WhenAll(loadTasks);
            }

            Task AutoSaveIfEnabled()
            {
                if (!_autoSaveEnabled)
                {
                    return Task.CompletedTask;
                }

                if (_autoSaveAll)
                {
                    var saveTasks = new List<Task>();

                    foreach (var stateManager in stateManagerResolver.ResolvedStateManagers)
                    {
                        saveTasks.Add(stateManager.SaveChanges());
                    }

                    return Task.WhenAll(saveTasks);
                }
                else
                {
                    throw new NotImplementedException("Partial auto-saving is not implemented yet.");
                }
            }
        }

        internal static string BuildStateManagerNamespaceForType(Type stateManagerType) => $"TYPED:{stateManagerType.FullName}";
    }
}
