// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1601 // Partial elements should be documented

namespace Microsoft.Bot.AdaptiveExpressions.Core.TriggerTrees.Tests
{
    public partial class Generator
    {
        public class SimpleValues
        {
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

            public int Int { get; set; } = 1;

            public double Double { get; set; } = 2.0;

            public string String { get; set; } = "3";

            public object Object { get; set; } = null;

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
