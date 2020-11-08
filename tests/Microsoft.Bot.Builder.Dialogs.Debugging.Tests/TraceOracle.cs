using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public static IReadOnlyList<T> TopologicalSort<T>(IEnumerable<T> nodes, Func<T, IReadOnlyList<T>> targetsBySource, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;

            // https://en.wikipedia.org/wiki/Topological_sorting#Depth-first_search
            var sort = new List<T>();
            var done = new HashSet<T>(comparer);

            void Until(T source)
            {
                if (done.Add(source))
                {
                    var targets = targetsBySource(source);

                    // iterate in reverse order to preserve ordering after Reverse below
                    for (var index = targets.Count - 1; index >= 0; --index)
                    {
                        var target = targets[index];
                        Until(target);
                    }

                    sort.Add(source);
                }
            }

            foreach (var node in nodes)
            {
                Until(node);
            }

            sort.Reverse();
            return sort;
        }

        public static JToken Normalize(JToken token)
            => Visit(token, FixLineEnding);

        private static JToken FixLineEnding(JToken token)
            => token is JValue value && value.Value is string text
            ? new JValue(FixLineEnding(text))
            : token;

        // https://stackoverflow.com/questions/55475483/regex-to-find-and-fix-lf-lineendings-to-crlf
        private static string FixLineEnding(string text)
            => Regex.Replace(text, "(?<!\r)\n", "\r\n", RegexOptions.None);

        private static JToken Visit(JToken token, Func<JToken, JToken> visitor)
        {
            if (token is JValue value)
            {
                return visitor(value);
            }
            else if (token is JContainer container)
            {
                var children = container.Children().Select(c => Visit(c, visitor));
                if (token is JProperty property)
                {
                    return visitor(new JProperty(property.Name) { Value = children.Single() });
                }
                else if (token is JArray array)
                {
                    return visitor(new JArray(children));
                }
                else if (token is JObject record)
                {
                    return visitor(new JObject(children));
                }
            }

            throw new NotImplementedException();
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
