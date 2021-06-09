// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly ImmutableDictionary<string, object> _initialSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsMemoryScope"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> from which to create these settings.</param>
        public SettingsMemoryScope(IConfiguration configuration = null)
            : base(ScopePath.Settings)
        {
            IncludeInSnapshot = false;
            _initialSettings = LoadSettings(configuration);
        }

        /// <summary>
        /// Gets the backing memory for this scope.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> object for this turn.</param>
        /// <returns>Memory for the scope.</returns>
        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (!dc.Context.TurnState.TryGetValue(ScopePath.Settings, out var settings))
            {
                // legacy behavior is to look for IConfiguration on the TurnState - some tests case rely on this
                var configuration = dc.Context.TurnState.Get<IConfiguration>();
                if (configuration != null)
                {
                    settings = LoadSettings(configuration);
                    dc.Context.TurnState[ScopePath.Settings] = settings;
                }
            }

            // settings is immutable and AdaptiveDialog.Tests rely on that, oddly _emptySettings are mutable
            return settings ?? _emptySettings;
        }

        /// <inheritdoc/>
        public async override Task LoadAsync(DialogContext dialogContext, bool force = false, CancellationToken cancellationToken = default)
        {
            if (!_initialSettings.IsEmpty)
            {
                dialogContext.Context.TurnState[ScopePath.Settings] = _initialSettings;
            }

            await base.LoadAsync(dialogContext, force, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Changes the backing object for the memory scope.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> object for this turn.</param>
        /// <param name="memory">Memory object to set for the scope.</param>
        /// <remarks>Method not supported. You cannot set the memory for a readonly memory scope.</remarks>
        public override void SetMemory(DialogContext dc, object memory)
        {
            throw new NotSupportedException("You cannot set the memory for a readonly memory scope");
        }

        /// <summary>
        /// Build a dictionary view of configuration providers.
        /// </summary>
        /// <param name="configuration">IConfiguration that we are running with.</param>
        /// <returns>projected dictionary for settings.</returns>
        protected static ImmutableDictionary<string, object> LoadSettings(IConfiguration configuration)
        {
            var settings = new Dictionary<string, object>();

            if (configuration != null)
            {
                var configurations = configuration.AsEnumerable().Where(u => !_blockingList.Contains(u.Key, StringComparer.OrdinalIgnoreCase)).ToList();

                // load configuration into settings dictionary
                var root = ConvertFlattenSettingToNode(configurations);
                root.Children.ForEach(u => settings.Add(u.Value, ConvertNodeToObject(u)));
            }

            return settings.ToImmutableDictionary();
        }

        /// <summary>
        /// Generate a node tree with the flatten settings.
        /// For example:
        /// {
        ///   "array":["item1", "item2"],
        ///   "object":{"array":["item1"], "2":"numberkey"}
        /// }
        /// 
        /// Would generate a flatten settings like:
        /// array:0 item1
        /// array:1 item2
        /// object:array:0 item1
        /// object:2 numberkey
        /// 
        /// After Converting it from flatten settings into node tree, would get:
        /// 
        ///                         null
        ///                |                     |
        ///              array                object
        ///            |        |            |        |
        ///           0          1        array        2
        ///           |          |         |           |
        ///         item1       item2      0        numberkey
        ///                                |
        ///                              item1
        /// The result is a Tree.
        /// </summary>
        /// <param name="kvs">Configurations with key value pairs.</param>
        /// <returns>The root node of the tree.</returns>
        private static Node ConvertFlattenSettingToNode(List<KeyValuePair<string, string>> kvs)
        {
            var root = new Node(null);
            foreach (var child in kvs)
            {
                var keyChain = child.Key.Split(':');
                var currentNode = root;
                foreach (var item in keyChain)
                {
                    var matchItem = currentNode.Children.FirstOrDefault(u => u?.Value == item);
                    if (matchItem == null)
                    {
                        // Remove all the leaf children
                        currentNode.Children.RemoveAll(u => u.Children.Count == 0);

                        // Append new child into current node
                        var node = new Node(item);
                        currentNode.Children.Add(node);
                        currentNode = node;
                    }
                    else
                    {
                        currentNode = matchItem;
                    }
                }

                currentNode.Children.Add(new Node(child.Value));
            }

            return root;
        }

        private static object ConvertNodeToObject(Node node)
        {
            if (node.Children.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            // If the child is leaf node, return its value directly.
            if (node.Children.Count == 1 && node.Children[0].Children.Count == 0)
            {
                return node.Children[0].Value;
            }

            // check if all the children are number format.
            var pureNumberIndex = true;
            var indexArray = new List<int>();
            foreach (var child in node.Children)
            {
                if (int.TryParse(child.Value, out var number) && number >= 0)
                {
                    indexArray.Add(number);
                }
                else
                {
                    pureNumberIndex = false;
                    break;
                }
            }

            if (pureNumberIndex)
            {
                // all children are int number, treat it as Array
                var listResult = new object[indexArray.Max() + 1];
                for (var i = 0; i < node.Children.Count; i++)
                {
                    listResult[indexArray[i]] = ConvertNodeToObject(node.Children[i]);
                }

                return listResult;
            }

            // Convert all children into dictionary
            var result = new Dictionary<string, object>();
            node.Children.ForEach(u => result.Add(u.Value, ConvertNodeToObject(u)));

            return result;
        }

        /// <summary>
        /// The setting node.
        /// </summary>
        private class Node
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class.
            /// </summary>
            public Node(string value)
            {
                Value = value;
            }

            /// <summary>
            /// Gets or sets value of the node. If the node is not leaf, value presents the current path.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets children of the node.
            /// </summary>
            public List<Node> Children { get; set; } = new List<Node>();
        }
    }
}
