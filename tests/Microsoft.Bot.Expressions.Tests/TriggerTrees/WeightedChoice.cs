#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1601 // Partial elements should be documented

namespace Microsoft.Bot.Expressions.TriggerTrees.Tests
{
    public partial class Generator
    {
        public class WeightedChoice<T>
        {
            public double Weight = 0.0;
            public T Choice = default(T);
        }
    }
}
