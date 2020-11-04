using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests
{
    /// <summary>
    /// An alternative to hand writing test assertions for complicated protocols.
    /// https://en.wikipedia.org/wiki/Test_oracle
    /// .
    /// </summary>
    public static class TraceOracle
    {
        private static readonly JToken FailureMarker = new JValue("FAILFAIL");
        private static readonly IEqualityComparer<JToken> Comparer = new JTokenEqualityComparer();

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };

        public static string MakePath(params string[] names)
            => Path.ChangeExtension(Path.Combine(GetProjectPath(), "TraceOracles", Path.Combine(names)), "json");

        public static async Task ValidateAsync(string pathFile, IReadOnlyList<JToken> listNew, ITestOutputHelper output)
        {
            // ensure the trace oracles directory exists
            var pathRoot = Path.GetDirectoryName(pathFile);
            if (!Directory.Exists(pathRoot))
            {
                Directory.CreateDirectory(pathRoot);
            }

            // if the trace oracle exists
            if (File.Exists(pathFile))
            {
                // then load the existing trace
                var jsonOld = await File.ReadAllTextAsync(pathFile).ConfigureAwait(false);
                var listOld = JsonConvert.DeserializeObject<IReadOnlyList<JToken>>(jsonOld);

                // and verify that the previous test has not failed
                Assert.DoesNotContain<JToken>(FailureMarker, listOld, Comparer);

                try
                {
                    // now verify that the trace has not changed
                    Assert.Equal(listOld, listNew, Comparer);
                }
                catch
                {
                    // if it has changed, then add a failure marker for diagnosis
                    var count = Math.Min(listOld.Count, listNew.Count);
                    int index;
                    for (index = 0; index < count; ++index)
                    {
                        if (!Comparer.Equals(listOld[index], listNew[index]))
                        {
                            break;
                        }
                    }

                    var items = new List<JToken>(listNew);
                    items.Insert(index, FailureMarker);

                    // and updated the saved trace oracle for review
                    var jsonNew = JsonConvert.SerializeObject(items, Settings);
                    await File.WriteAllTextAsync(pathFile, jsonNew).ConfigureAwait(false);

                    output.WriteLine(jsonNew);

                    throw;
                }
            }
            else
            {
                // if the trace oracle does not exist, then assume we are either
                // 1. establishing a new trace
                // 2. the developer deleted the older one after code updates with expected changes
                var jsonNew = JsonConvert.SerializeObject(listNew, Settings);
                await File.WriteAllTextAsync(pathFile, jsonNew).ConfigureAwait(false);
            }
        }

        private static string GetProjectPath()
        {
            var parent = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(parent))
            {
                if (Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }
                else
                {
                    parent = Path.GetDirectoryName(parent);
                }
            }

            return parent;
        }
    }
}
