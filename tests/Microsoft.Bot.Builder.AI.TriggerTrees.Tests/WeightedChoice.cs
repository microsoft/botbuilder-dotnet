#pragma warning disable SA1401 // Fields should be private

namespace Microsoft.Bot.Builder.AI.TriggerTrees.Tests
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
