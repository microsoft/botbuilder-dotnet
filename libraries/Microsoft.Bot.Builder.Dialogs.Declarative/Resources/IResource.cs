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
        /// Resource name
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Get resource as text async
        /// </summary>
        /// <returns></returns>
        Task<string> ReadTextAsync();

        /// <summary>
        /// Get resource as text
        /// </summary>
        /// <returns></returns>
        string ReadText();

        /// <summary>
        /// Get readonly stream
        /// </summary>
        /// <returns></returns>
        Stream OpenStream();
    }
}
