namespace Microsoft.Bot.Builder.Core.State
{
    public interface IStateManagerServiceResolver
    {
        TStateManager ResolveStateManager<TStateManager>() where TStateManager : class, IStateManager;
        IStateManager ResolveStateManager(string stateNamespace);
    }


}
