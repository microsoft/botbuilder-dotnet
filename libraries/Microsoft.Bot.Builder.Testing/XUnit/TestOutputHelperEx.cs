// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Microsoft.Bot.Builder.Testing.XUnit
{
    /// <summary>
    /// Extension methods for XUnit <see cref="ITestOutputHelper"/>.
    /// </summary>
    public static class TestOutputHelperEx
    {
        /// <summary>
        /// Writes a given object as formatted json.
        /// </summary>
        /// <param name="outputHelper">the instance of <see cref="ITestOutputHelper"/> being extended.</param>
        /// <param name="object">An object instance to be written to the output as formatted json.</param>
        public static void WriteAsFormattedJson(this ITestOutputHelper outputHelper, object @object)
        {
            var s = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            outputHelper.WriteLine(JsonConvert.SerializeObject(@object, s));
        }
    }
}
