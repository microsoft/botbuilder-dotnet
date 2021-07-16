// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Class which represents a file as a resource.
    /// </summary>
    public class FileResource : Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileResource"/> class.
        /// </summary>
        /// <param name="path">path to file.</param>
        public FileResource(string path)
        {
            this.FullName = path;
            this.Id = Path.GetFileName(path);
        }

        /// <summary>
        /// Open a stream to the resource.
        /// </summary>
        /// <returns>Stream for accesssing the content of the resource.</returns>
        public override async Task<Stream> OpenStreamAsync()
        {
            return await Task.FromResult(new FileStream(this.FullName, FileMode.Open)).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return this.Id;
        }
    }
}
