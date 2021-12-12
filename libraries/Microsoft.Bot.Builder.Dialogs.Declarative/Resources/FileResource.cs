// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
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
        private readonly Lazy<Task<string>> _contentCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileResource"/> class.
        /// </summary>
        /// <param name="path">path to file.</param>
        public FileResource(string path)
        {
            FullName = path;
            Id = Path.GetFileName(path);
            _contentCache = new Lazy<Task<string>>(GetOrLoadTextAsync);
        }

        /// <inheritdoc/>
        public override async Task<string> ReadTextAsync()
        {
            return await _contentCache.Value.ConfigureAwait(false);
        }

        /// <summary>
        /// Open a stream to the resource.
        /// </summary>
        /// <returns>Stream for accesssing the content of the resource.</returns>
        public override async Task<Stream> OpenStreamAsync()
        {
            return await Task.FromResult(new FileStream(FullName, FileMode.Open)).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Id;
        }

        private async Task<string> GetOrLoadTextAsync()
        {
            return await base.ReadTextAsync().ConfigureAwait(false);
        }
    }
}
