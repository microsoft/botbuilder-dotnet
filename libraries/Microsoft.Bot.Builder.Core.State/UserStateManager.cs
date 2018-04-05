namespace Microsoft.Bot.Builder.Core.State
{
    public interface IUserStateManager : IStateManager
    {
        string UserId { get; }
    }

    public class UserStateManager : StateManager, IUserStateManager
    {
        public UserStateManager(string userId, IStateStorageProvider stateStore) : base(BuildStateNamespace(userId), stateStore)
        {
            UserId = userId ?? throw new System.ArgumentNullException(nameof(userId));
        }

        public string UserId { get; }

        public static string BuildStateNamespace(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new System.ArgumentException("Expected non-null/empty value.", nameof(userId));
            }

            return $"/users/{userId}";
        }

        public static string BuildStateNamespace(ITurnContext turnContext)
        {
            if (turnContext == null)
            {
                throw new System.ArgumentNullException(nameof(turnContext));
            }

            return BuildStateNamespace(turnContext.Activity.From.Id);
        }
    }
}
