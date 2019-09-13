#pragma warning disable SA1401 // Fields should be private

namespace Microsoft.Bot.Builder.AI.TriggerTrees.Tests
{
    public partial class Generator
    {
        public class SimpleValues
        {
            public int Int = 1;
            public double Double = 2.0;
            public string String = "3";
            public object Object = null;

            public SimpleValues()
            {
            }

            public SimpleValues(int integer)
            {
                Int = integer;
            }

            public SimpleValues(double number)
            {
                Double = number;
            }

            public SimpleValues(object obj)
            {
                Object = obj;
            }
            
            public static bool Test(SimpleValues obj, int? value) => value.HasValue && obj.Int == value;

            public static bool Test(SimpleValues obj, double? value) => value.HasValue && obj.Double == value;

            public static bool Test(SimpleValues obj, string value) => value != null && obj.String == value;

            public static bool Test(SimpleValues obj, object other) => other != null && obj.Object.Equals(other);

            public bool Test(int? value) => value.HasValue && Int == value;

            public bool Test(double? value) => value.HasValue && Double == value;

            public bool Test(string value) => value != null && String == value;

            public bool Test(SimpleValues value) => Int == value.Int && Double == value.Double && String == value.String && Object.Equals(value.Object);
        }
    }
}
