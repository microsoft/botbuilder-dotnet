using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Core.State
{
    internal class StateManagerServiceResolver : IStateManagerServiceResolver
    {
        private readonly ITurnContext _turnContext;
        private readonly Dictionary<string, IStateStorageProvider> _availableStateStores;
        private readonly Dictionary<string, StateManagerConfiguration> _availableStateManagerConfigurations;
        private readonly Dictionary<string, IStateManager> _resolvedStateManagers = new Dictionary<string, IStateManager>();

        public StateManagerServiceResolver(ITurnContext turnContext, Dictionary<string, IStateStorageProvider> availableStateStores, Dictionary<string, StateManagerConfiguration> availableStateManagerConfigurations)
        {
            _turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            _availableStateStores = availableStateStores;
            _availableStateManagerConfigurations = availableStateManagerConfigurations;
        }

        public IReadOnlyCollection<IStateManager> ResolvedStateManagers => _resolvedStateManagers.Values;

        public IStateManager ResolveStateManager(string stateNamespace)
        {
            if (string.IsNullOrEmpty(stateNamespace))
            {
                throw new ArgumentException("Expected a non-null/empy value.", nameof(stateNamespace));
            }

            if (_resolvedStateManagers.TryGetValue(stateNamespace, out var stateManager))
            {
                return stateManager;
            }

            if (!_availableStateManagerConfigurations.TryGetValue(stateNamespace, out var stateManagerConfiguration))
            {
                throw new InvalidOperationException($"No state manager for namespace \"{stateNamespace}\" has been configured with the state management middleware. Please check your middleware configuration.");
            }

            return CreateAndCacheStateManager(stateManagerConfiguration);
        }

        public TStateManager ResolveStateManager<TStateManager>() where TStateManager : class, IStateManager
        {
            var typedStateManagerKey = StateManagementMiddleware.BuildStateManagerNamespaceForType(typeof(TStateManager));

            if (_resolvedStateManagers.TryGetValue(typedStateManagerKey, out var stateManager))
            {
                return stateManager as TStateManager;
            }

            if (!_availableStateManagerConfigurations.TryGetValue(typedStateManagerKey, out var stateManagerConfiguration))
            {
                throw new InvalidOperationException($"No state manager of type {typeof(TStateManager).Name} has been configured with the state management middleware. Please check your middleware configuration.");
            }

            return CreateAndCacheStateManager(stateManagerConfiguration) as TStateManager;
        }

        internal IStateManager ResolveStateManager(StateManagerConfiguration stateManagerConfiguration)
        {
            var stateManagerKey = stateManagerConfiguration.StateNamespace;

            if(_resolvedStateManagers.TryGetValue(stateManagerConfiguration.StateNamespace, out var stateManager))
            {
                return stateManager;
            }

            return CreateAndCacheStateManager(stateManagerConfiguration); ;
        }

        private IStateManager CreateAndCacheStateManager(StateManagerConfiguration stateManagerConfiguration)
        {
            if (stateManagerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(stateManagerConfiguration));
            }

            if (!_availableStateStores.TryGetValue(stateManagerConfiguration.StateStoreName, out var configuredStateStore))
            {
                if (stateManagerConfiguration.StateManagerType != null)
                {
                    throw new InvalidOperationException($"State manager of type \"{stateManagerConfiguration.StateManagerType.FullName}\" was configured to use a state store named \"{stateManagerConfiguration.StateStoreName}\", but no such named state store was configured. Please check your middleware configuration.");
                }

                throw new InvalidOperationException($"State manager for namespace \"{stateManagerConfiguration.StateNamespace}\" was configured to use a state store named \"{stateManagerConfiguration.StateStoreName}\", but no such named state store was configured. Please check your middleware configuration.");
            }

            var stateManager = default(IStateManager);

            if (stateManagerConfiguration.StateManagerType != null)
            {
                if (stateManagerConfiguration.Factory != null)
                {
                    stateManager = stateManagerConfiguration.Factory(_turnContext, configuredStateStore);
                }
                else
                {
                    try
                    {
                        stateManager = (IStateManager)Activator.CreateInstance(stateManagerConfiguration.StateManagerType, configuredStateStore);
                    }
                    catch (Exception createException)
                    {
                        throw new Exception($"Failed to create an instance of a typed state manager: {stateManagerConfiguration.StateManagerType.FullName}. Please check the inner exception for more details.", createException);
                    }
                }
            }
            else
            {
                stateManager = new StateManager(stateManagerConfiguration.StateNamespace, configuredStateStore);
            }

            _resolvedStateManagers.Add(stateManagerConfiguration.StateNamespace, stateManager);

            return stateManager;
        }
    }


}
