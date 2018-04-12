using System;

namespace Microsoft.Bot.Builder.Core.State
{
    internal class StateManagerConfiguration
    {
        public string StateNamespace { get; set; }
        public Type StateManagerType { get; set; }
        public string StateStoreName { get; set; }
        public bool AutoLoad { get; set; }
        public string[] AutoLoadSpecificKeys { get; set; }
        public Func<ITurnContext, IStateStorageProvider, IStateManager> Factory { get; set; }
    }
}
