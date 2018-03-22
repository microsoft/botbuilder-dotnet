namespace Microsoft.Bot.Builder.Core.State
{
    public interface IStateStorageEntry
    {
        string Namespace { get; }

        string Key { get;  }

        string ETag { get; }

        T GetValue<T>() where T : class, new();

        void SetValue<T>(T value) where T : class;
    }
}