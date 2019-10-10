using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs
{
    public static class Configuration
    {
        /// <summary>
        /// Build a dictionary view of configuration providers.
        /// </summary>
        /// <param name="configuration">IConfiguration that we are running with.</param>
        /// <returns>projected dictionary for settings.</returns>
        public static Dictionary<string, object> LoadSettings(IConfiguration configuration)
        {
            var settings = new Dictionary<string, object>();

            if (configuration != null)
            {
                // load configuration into settings dictionary
                foreach (var child in configuration.AsEnumerable())
                {
                    string[] keys = child.Key.Split(':');
                    IDictionary<string, object> node = settings;
                    for (int i = 0; i < keys.Length - 1; i++)
                    {
                        var key = keys[i];
                        if (!node.ContainsKey(key))
                        {
                            node[key] = new Dictionary<string, object>();
                        }

                        node = (Dictionary<string, object>)node[key];
                    }

                    if (child.Value != null)
                    {
                        node[keys.Last()] = child.Value;
                    }
                }
            }

            return settings;
        }
    }
}
