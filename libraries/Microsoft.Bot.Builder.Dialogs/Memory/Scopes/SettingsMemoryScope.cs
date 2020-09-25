// Copyright (c) Microsoft Corporation. All rights reserved.
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
        private readonly Dictionary<string, object> _emptySettings = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsMemoryScope"/> class.
        /// </summary>
        public SettingsMemoryScope()
            : base(ScopePath.Settings)
        {
            IncludeInSnapshot = false;
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
                var configuration = dc.Context.TurnState.Get<IConfiguration>();
                if (configuration != null)
                {
                    settings = LoadSettings(configuration);
                    dc.Context.TurnState[ScopePath.Settings] = settings;
                }
            }

            return settings ?? _emptySettings;
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
        protected static Dictionary<string, object> LoadSettings(IConfiguration configuration)
        {
            var settings = new Dictionary<string, object>();

            if (configuration != null)
            {
                // load configuration into settings dictionary
                var root = ConvertFlattenSettingToNode(configuration.AsEnumerable().ToList());
                root.Children.ForEach(u => settings.Add(u.Value, ConvertNodeToObject(u)));
            }

            return settings;
        }

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
            // If the children is leaf node, return its value directly.
            if (node.Children.Count == 1 && node.Children[0].Children.Count == 0)
            {
                return node.Children[0].Value;
            }

            if (node.Children.All(u => int.TryParse(u.Value, out var number) && number >= 0))
            {
                // all children are int number, treat it as Array
                var pairs = new List<Tuple<int, object>>();
                node.Children.ForEach(u => pairs.Add(new Tuple<int, object>(int.Parse(u.Value, CultureInfo.InvariantCulture), ConvertNodeToObject(u))));
                var maxIndex = pairs.Select(u => u.Item1).Max();
                var list = new object[maxIndex + 1];
                foreach (var pair in pairs)
                {
                    list[pair.Item1] = pair.Item2;
                }

                return list;
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
