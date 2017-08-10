using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{

    public class StoreItem : FlexObject
    {
        /// <summary>
        /// eTag for concurrency
        /// </summary>
        public string eTag { get; set; }
    }

    public interface IStorage
    {
        /// <summary>
        /// Read StoreItems from storage
        /// </summary>
        /// <param name="keys">keys of the storeItems to read</param>
        /// <returns>StoreItem dictionary</returns>
        Task<Dictionary<string, StoreItem>> Read(string[] keys);

        /// <summary>
        /// Write StoreItems to storage
        /// </summary>
        /// <param name="changes"></param>
        Task Write(Dictionary<string, StoreItem> changes);

        /// <summary>
        /// Delete StoreItems from storage
        /// </summary>
        /// <param name="keys">keys of the storeItems to delete</param>
        Task Delete(string[] keys);
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
        public static async Task<Dictionary<string, StoreItemT>> Read<StoreItemT>(this IStorage storage, string[] keys)
            where StoreItemT : StoreItem
        {
            var results = await storage.Read(keys).ConfigureAwait(false);
            var newResults = new Dictionary<string,StoreItemT>();
            foreach (var key in results.Keys)
                newResults[key] = results[key] as StoreItemT;
            return newResults;
        }
    }
}
