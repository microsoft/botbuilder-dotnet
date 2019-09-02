namespace Microsoft.Bot.Builder.Expressions
{
    // the memory scope manager for the evaluation process an expression
    // which controls the memory scope access and update
    public interface IMemoryScopeManager
    {
        // resolve a value for a given path, the path can be a simple indenfiter like "a"
        // or a combined path like "a.b", "a.b[2]", "a.b[2].c"
        object GetValue(string path);

        // set a value to a given path
        void SetValue(string path);

        // push a new scope when a sub-evaluation
        void PushScope(object scope);

        // pop scope when sub-evaluation finished
        void PopScope();
    }
}
