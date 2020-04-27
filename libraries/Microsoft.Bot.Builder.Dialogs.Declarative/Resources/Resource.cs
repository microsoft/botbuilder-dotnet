// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Interface for access the content of a resource.
    /// </summary>
    public abstract class Resource
    {
        /// <summary>
        /// Gets or sets resource id.
        /// </summary>
        /// <value>
        /// Resource id.
        /// </value>
        public string Id { get; protected set; }

        /// <summary>
        /// Get readonly stream. 
        /// </summary>
        /// <returns>The resource as a stream.</returns>
        public abstract Task<Stream> OpenStreamAsync();

        /// <summary>
        /// Get resource as text async.
        /// </summary>
        /// <returns>The resource as text.</returns>
        public virtual async Task<string> ReadTextAsync()
        {
            using (var stream = await OpenStreamAsync().ConfigureAwait(false))
            {
                TextReader textReader = new StreamReader(stream);
                return await textReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}
