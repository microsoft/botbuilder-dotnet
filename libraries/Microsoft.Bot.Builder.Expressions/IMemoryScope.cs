namespace Microsoft.Bot.Builder.Expressions
{
    public interface IMemoryScope
    {
        // resolve a value for a given path, it can be a simple indenfiter like "a"
        // or a combined path like "a.b", "a.b[2]", "a.b[2].c"
        // what's inside [] is guranteed to be a int number
        object GetValue(string path);

        // set a value to a given path
        object SetValue(string path, object value);
    }
}
