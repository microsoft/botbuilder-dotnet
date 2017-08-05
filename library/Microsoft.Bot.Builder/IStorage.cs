using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// collection of named StoreItem objects
    /// </summary>
    public class StoreItems : Dictionary<string, StoreItems>
    {

    }

    interface IStorage
    {
        /// <summary>
        /// Read StoreItems from storage
        /// </summary>
        /// <param name="keys">keys of the storeItems to read</param>
        /// <returns>StoreItem dictionary</returns>
        Task<StoreItems> Read(string[] keys);

        /// <summary>
        /// Write StoreItems to storage
        /// </summary>
        /// <param name="changes"></param>
        Task Write(StoreItems changes);

        /// <summary>
        /// Delete StoreItems from storage
        /// </summary>
        /// <param name="keys">keys of the storeItems to delete</param>
        Task Delete(string[] keys);
    }
}
