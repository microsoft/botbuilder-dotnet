using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public interface IResource
    {
        /// <summary>
        /// Gets resource name.
        /// </summary>
        /// <value>
        /// Resource name.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Get resource as text async.
        /// </summary>
        /// <returns></returns>
        Task<string> ReadTextAsync();

        /// <summary>
        /// Get readonly stream. 
        /// </summary>
        /// <returns></returns>
        Task<Stream> OpenStreamAsync();
    }
}
