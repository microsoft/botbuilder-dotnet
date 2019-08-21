using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// File resource.
    /// </summary>
    public class FileResource : IResource
    {
        private string path;
        private Task<byte[]> contentTask;
        private Task<string> textTask;

        public FileResource(string path)
        {
            this.path = path;
            this.Id = Path.GetFileName(path);
        }

        public string Id { get; }

        public string FullName
        {
            get { return this.path; }
        }

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
