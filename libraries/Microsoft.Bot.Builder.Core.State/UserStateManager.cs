namespace Microsoft.Bot.Builder.Core.State
{
    public interface IUserStateManager : IStateManager
    {
        string UserId { get; }
    }

    public class UserStateManager : StateManager, IUserStateManager
    {
        public static string PreferredStateStoreName = $"{nameof(UserStateManager)}.PreferredStateStore";

        public UserStateManager(string userId, IStateStorageProvider stateStore) : base(BuildStateNamespace(userId), stateStore)
        {
            UserId = userId ?? throw new System.ArgumentNullException(nameof(userId));
        }

        public string UserId { get; }

        public static string BuildStateNamespace(string userId) => $"/users/{userId}";

        public static string BuildStateNamespace(ITurnContext turnContext) => BuildStateNamespace(turnContext.Activity.From.Id);
    }
}
