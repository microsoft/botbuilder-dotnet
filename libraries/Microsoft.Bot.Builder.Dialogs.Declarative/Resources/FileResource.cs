﻿// Licensed under the MIT License.
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
        private Task<byte[]> contentTask;

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
            if (contentTask == null)
            {
                this.contentTask = Task.Run(async () =>
                {
                    Trace.TraceInformation($"Loading {this.Id}");
                    var fileInfo = new FileInfo(this.FullName);
                    Stream stream = null;
                    try
                    {
                        stream = File.OpenRead(this.FullName);
                        var buffer = new byte[fileInfo.Length];
                        await stream.ReadAsync(buffer, 0, (int)fileInfo.Length).ConfigureAwait(false);
                        return buffer;
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Close();
                        }
                    }
                });
            }

            var content = await contentTask.ConfigureAwait(false);
            return new MemoryStream(content);
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
