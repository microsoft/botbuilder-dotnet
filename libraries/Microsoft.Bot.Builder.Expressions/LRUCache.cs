using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Bot.Builder.Expressions
{
    public class LRUCache<TKey, TValue>
    {
        private const int DefaultCapacity = 255;
        private int capacity;
        private ReaderWriterLockSlim locker;
        private IDictionary<TKey, TValue> dictionary;
        private LinkedList<TKey> linkedList;

        public LRUCache()
            : this(DefaultCapacity)
        {
        }

        public LRUCache(int capacity)
        {
            locker = new ReaderWriterLockSlim();
            this.capacity = capacity > 0 ? capacity : DefaultCapacity;
            dictionary = new Dictionary<TKey, TValue>();
            linkedList = new LinkedList<TKey>();
        }

        public int Count
        {
            get
            {
                locker.EnterReadLock();
                try
                {
                    return dictionary.Count;
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public int Capacity
        {
            get
            {
                locker.EnterReadLock();
                try
                {
                    return capacity;
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }

            set
            {
                locker.EnterUpgradeableReadLock();
                try
                {
                    if (value > 0 && capacity != value)
                    {
                        locker.EnterWriteLock();
                        try
                        {
                            capacity = value;
                            while (linkedList.Count > capacity)
                            {
                                linkedList.RemoveLast();
                            }
                        }
                        finally
                        {
                            locker.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    locker.ExitUpgradeableReadLock();
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                locker.EnterReadLock();
                try
                {
                    return dictionary.Keys;
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                locker.EnterReadLock();
                try
                {
                    return dictionary.Values;
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public void Set(TKey key, TValue value)
        {
            locker.EnterWriteLock();
            try
            {
                dictionary[key] = value;
                linkedList.Remove(key);
                linkedList.AddFirst(key);
                if (linkedList.Count > capacity)
                {
                    dictionary.Remove(linkedList.Last.Value);
                    linkedList.RemoveLast();
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool TryGet(TKey key, out TValue value)
        {
            locker.EnterUpgradeableReadLock();
            try
            {
                var getSuccess = dictionary.TryGetValue(key, out value);
                if (getSuccess)
                {
                    locker.EnterWriteLock();
                    try
                    {
                        linkedList.Remove(key);
                        linkedList.AddFirst(key);
                    }
                    finally
                    {
                        locker.ExitWriteLock();
                    }
                }
                return getSuccess;
            }
            catch
            {
                throw;
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            locker.EnterReadLock();
            try
            {
                return dictionary.ContainsKey(key);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }
    }
}
