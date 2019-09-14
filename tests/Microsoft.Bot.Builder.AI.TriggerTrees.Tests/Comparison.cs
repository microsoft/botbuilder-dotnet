#pragma warning disable SA1401 // Fields should be private

namespace Microsoft.Bot.Builder.AI.TriggerTrees.Tests
{
    public class Comparison
    {
        public string Type;
        public object Value;

        public Comparison(string type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
