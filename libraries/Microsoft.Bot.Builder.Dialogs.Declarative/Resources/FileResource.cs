using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Class which represents a file as a resource.
    /// </summary>
    public class FileResource : IResource
    {
        private string path;
        private Task<byte[]> contentTask;
        private Task<string> textTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileResource"/> class.
        /// </summary>
        /// <param name="path">path to file.</param>
        public FileResource(string path)
        {
            this.path = path;
            this.Id = Path.GetFileName(path);
        }

        /// <summary>
        /// Gets resource Id for the resource.
        /// </summary>
        /// <value>
        /// ResourceId.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Gets the resource path.
        /// </summary>
        /// <value>
        /// The full path to the resource on disk.
        /// </value>
        public string FullName
        {
            get { return this.path; }
        }

        /// <summary>
        /// Open a stream to the resource.
        /// </summary>
        /// <returns>Stream for accesssing the content of the resource.</returns>
        public async Task<Stream> OpenStreamAsync()
        {
            if (contentTask == null)
            {
                this.contentTask = Task.Run(async () =>
                {
                    Trace.TraceInformation($"Loading {this.Id}");
                    var fileInfo = new FileInfo(this.path);
                    Stream stream = null;
                    try
                    {
                        stream = File.OpenRead(this.path);
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
        /// Get resource as a text.
        /// </summary>
        /// <returns>A <see cref="Task"/> with the string.</returns>
        public Task<string> ReadTextAsync()
        {
            if (this.textTask == null)
            {
                this.textTask = Task.Run(async () =>
                {
                    var stream = await OpenStreamAsync().ConfigureAwait(false);
                    TextReader textReader = new StreamReader(stream);
                    return await textReader.ReadToEndAsync().ConfigureAwait(false);
                });
            }

            return this.textTask;
        }

        public override string ToString()
        {
            return this.Id;
        }
    }
}
