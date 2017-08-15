using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Microsoft.Bot.Builder
{

    public class StoreItem : FlexObject
    {
        /// <summary>
        /// eTag for concurrency
        /// </summary>
        public string eTag { get; set; }
    }

    public class StoreItems : FlexObject
    {
    }

    public class StoreItems<StoreItemT> : StoreItems
        where StoreItemT : StoreItem
    {
    }

    public interface IStorage
    {
        /// <summary>
        /// Read StoreItems from storage
        /// </summary>
        /// <param name="keys">keys of the storeItems to read</param>
        /// <returns>StoreItem dictionary</returns>
        Task<StoreItems> Read(params string[] keys);

        /// <summary>
        /// Write StoreItems to storage
        /// </summary>
        /// <param name="changes"></param>
        Task Write(StoreItems changes);

        /// <summary>
        /// Delete StoreItems from storage
        /// </summary>
        /// <param name="keys">keys of the storeItems to delete</param>
        Task Delete(params string[] keys);
    }

    public static class StorageExtensions
    {

        /// <summary>
        /// Storage extension to Read as strong typed StoreItem objects
        /// </summary>
        /// <typeparam name="StoreItemT"></typeparam>
        /// <param name="storage"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static async Task<StoreItems<StoreItemT>> Read<StoreItemT>(this IStorage storage, params string[] keys)
            where StoreItemT : StoreItem
        {
            var storeItems = await storage.Read(keys).ConfigureAwait(false);
            var newResults = new StoreItems<StoreItemT>();
            foreach (var kv in storeItems)
                newResults[kv.Key] = kv.Value as StoreItemT;
            return newResults;
        }
    }
}
