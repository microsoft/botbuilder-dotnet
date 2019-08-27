using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Expressions
{
    public sealed class LRUCache<TKey, TValue>
    {
        private const int DefaultCapacity = 255;

        private readonly object lockObj = new object();
        private readonly int capacity;
        private readonly Dictionary<TKey, Entry> cacheMap;
        private readonly LinkedList<TKey> cacheList;

        public LRUCache()
            : this(DefaultCapacity)
        {
        }

        public LRUCache(int capacity)
        {
            this.capacity = capacity > 0 ? capacity : DefaultCapacity;
            this.cacheMap = new Dictionary<TKey, Entry>();
            this.cacheList = new LinkedList<TKey>();
        }

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
