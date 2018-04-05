using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Core.State
{
    public class StateManagerConfigurationBuilder
    {
        internal StateManagerConfigurationBuilder(StateManagerConfiguration configuration)
        {
            Configuration = configuration;
        }

        private StateManagerConfiguration Configuration { get; }

        public StateManagerConfigurationBuilder UseStorageProvider(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Expected a non-null/empty value.", nameof(name));
            }

            Configuration.StateStoreName = name;

            return this;
        }

        public StateManagerConfigurationBuilder UseFactory(Func<ITurnContext, IStateStorageProvider, IStateManager> stateManagerFactory)
        {
            Configuration.Factory = stateManagerFactory ?? throw new ArgumentNullException(nameof(stateManagerFactory));

            return this;
        }

        public StateManagerConfigurationBuilder AutoLoadAll()
        {
            Configuration.AutoLoad = true;
            Configuration.AutoLoadSpecificKeys = null;

            return this;
        }

        public StateManagerConfigurationBuilder AutoLoad(IEnumerable<string> keys) => AutoLoad(keys.ToArray());

        public StateManagerConfigurationBuilder AutoLoad(params string[] keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Length == 0)
            {
                throw new ArgumentException($"Expected one or more keys to be specified. Consider using {nameof(AutoLoadAll)} if you want all keys to be loaded.", nameof(keys));
            }

            Configuration.AutoLoad = true;
            Configuration.AutoLoadSpecificKeys = keys;

            return this;
        }
    }


}
