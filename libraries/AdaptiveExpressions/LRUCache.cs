// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions
{
    /// <summary>
    /// A least-recently-used cache stored like a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to the cached item.</typeparam>
    /// <typeparam name="TValue">The type of the cached item.</typeparam>
    public sealed class LRUCache<TKey, TValue>
    {
        /// <summary>
        /// Default maximum number of elements to cache.
        /// </summary>
        private const int DefaultCapacity = 255;

        private readonly object lockObj = new object();
        private readonly int capacity;
        private readonly Dictionary<TKey, Entry> cacheMap;
        private readonly LinkedList<TKey> cacheList;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRUCache{TKey, TValue}"/> class.
        /// </summary>
        public LRUCache()
            : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRUCache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="capacity">Maximum number of elements to cache.</param>
        public LRUCache(int capacity)
        {
            this.capacity = capacity > 0 ? capacity : DefaultCapacity;
            this.cacheMap = new Dictionary<TKey, Entry>();
            this.cacheList = new LinkedList<TKey>();
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with
        /// the specified key, if the key is found; otherwise, the default value for the 
        /// type of the <paramref name="value" /> parameter.</param>
        /// <returns>true if contains an element with the specified key; otherwise, false.</returns>
        public bool TryGet(TKey key, out TValue value)
        {
            lock (lockObj)
            {
                if (this.cacheMap.TryGetValue(key, out var entry))
                {
                    Touch(entry.Node);
                    value = entry.Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Adds the specified key and value to the cache.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        public void Set(TKey key, TValue value)
        {
            lock (lockObj)
            {
                if (!this.cacheMap.TryGetValue(key, out var entry))
                {
                    LinkedListNode<TKey> node;
                    if (this.cacheMap.Count >= this.capacity)
                    {
                        node = this.cacheList.Last;
                        this.cacheMap.Remove(node.Value);
                        this.cacheList.RemoveLast();
                        node.Value = key;
                    }
                    else
                    {
                        node = new LinkedListNode<TKey>(key);
                    }

                    this.cacheList.AddFirst(node);
                    this.cacheMap.Add(key, new Entry(node, value));
                }
                else
                {
                    entry.Value = value;
                    this.cacheMap[key] = entry;
                    Touch(entry.Node);
                }
            }
        }

        private void Touch(LinkedListNode<TKey> node)
        {
            if (node != this.cacheList.First)
            {
                this.cacheList.Remove(node);
                this.cacheList.AddFirst(node);
            }
        }

        private struct Entry
        {
            public LinkedListNode<TKey> Node;
            public TValue Value;

            public Entry(LinkedListNode<TKey> node, TValue value)
            {
                this.Node = node;
                this.Value = value;
            }
        }
    }
}
