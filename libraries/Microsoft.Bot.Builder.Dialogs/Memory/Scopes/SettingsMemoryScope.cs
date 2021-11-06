﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// SettingsMemoryscope maps "settings" -> IConfiguration.
    /// </summary>
    public class SettingsMemoryScope : MemoryScope
    {
        private static readonly List<string> _blockingList = new List<string>
        {
            "MicrosoftAppPassword",
            "cosmosDb:authKey",
            "blobStorage:connectionString",
            "BlobsStorage:connectionString",
            "CosmosDbPartitionedStorage:authKey",
            "applicationInsights:connectionString",
            "applicationInsights:InstrumentationKey",
            "runtimeSettings:telemetry:options:connectionString",
            "runtimeSettings:telemetry:options:instrumentationKey",
            "runtimeSettings:features:blobTranscript:connectionString"
        };

        private readonly Dictionary<string, object> _emptySettings = new Dictionary<string, object>();

        public SettingsMemoryScope()
            : base(ScopePath.Settings)
        {
            IncludeInSnapshot = false;
        }

        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (!dc.Context.TurnState.TryGetValue(ScopePath.Settings, out var settings))
            {
                var configuration = dc.Context.TurnState.Get<IConfiguration>();
                if (configuration != null)
                {
                    settings = LoadSettings(configuration);
                    dc.Context.TurnState[ScopePath.Settings] = settings;
                }
            }

            return settings ?? _emptySettings;
        }

        public override void SetMemory(DialogContext dc, object memory)
        {
            throw new NotSupportedException("You cannot set the memory for a readonly memory scope");
        }

        /// <summary>
        /// Build a dictionary view of configuration providers.
        /// </summary>
        /// <param name="configuration">IConfiguration that we are running with.</param>
        /// <returns>projected dictionary for settings.</returns>
        protected static Dictionary<string, object> LoadSettings(IConfiguration configuration)
        {
            var settings = new Dictionary<string, object>();

            if (configuration != null)
            {
                var configurations = configuration.AsEnumerable().Where(u => !_blockingList.Contains(u.Key, StringComparer.OrdinalIgnoreCase)).ToList();

                // load configuration into settings dictionary
                foreach (var child in configurations)
                {
                    var keys = child.Key.Split(':');

                    // initialize all parts of path up to the key/value we are attempting to set
                    dynamic parent = null;
                    var parentKey = string.Empty;
                    dynamic node = settings;
                    for (var i = 0; i < keys.Length - 1; i++)
                    {
                        var key = keys[i];
                        if (!node.ContainsKey(key))
                        {
                            node[key] = new Dictionary<string, object>();
                        }

                        parent = node;
                        parentKey = key;
                        node = node[key];
                    }

                    if (child.Value != null)
                    {
                        var lastKey = keys.Last();
                        var hasIndex = int.TryParse(lastKey, out var index);
                        if (hasIndex)
                        {
                            // if node is dictionary, and has zero elements then convert to array
                            var dict = node as IDictionary<string, object>;
                            if (dict != null && dict.Count == 0)
                            {
                                // replace parent with array
                                parent[parentKey] = new object[index + 1];
                                node = parent[parentKey];
                            }

                            // if it's still a dictionary that's because we have a non-number value in it
                            if (dict != null && dict.Count > 0)
                            {
                                // store using key as string
                                node[lastKey] = child.Value;
                            }
                            else
                            {
                                // it's an array
                                var arr = (object[])node;
                                if (arr.Length < index + 1)
                                {
                                    // auto-resize to be bigger if we need to 
                                    // NOTE: keys seem to be reverse indexed, which means we should have already "right-sized" the array
                                    Array.Resize(ref arr, index + 1);
                                    parent[parentKey] = arr;
                                    node = arr;
                                }

                                // store using keyValue as an index
                                arr[index] = child.Value;
                            }
                        }
                        else
                        {
                            if (node is object[] arr)
                            {
                                // we got a non-number key but we have building an array, convert the array to dictionary
                                // NOTE: keys is sorting reverse which means we should we always start out with a dictionary for nonnumberic keys
                                var dict = new Dictionary<string, object>();
                                for (var i = 0; i < arr.Length; i++)
                                {
                                    if (arr[i] != null)
                                    {
                                        dict[i.ToString(CultureInfo.InvariantCulture)] = arr[i];
                                    }
                                }

                                parent[parentKey] = dict;
                                node = dict;
                            }

                            node[lastKey] = child.Value;
                        }
                    }
                }
            }

            return settings;
        }
    }
}
