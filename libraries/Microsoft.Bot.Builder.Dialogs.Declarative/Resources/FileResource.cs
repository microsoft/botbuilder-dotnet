using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// File resource
    /// </summary>
    public class FileResource : IResource
    {
        private string path;

        public FileResource(string path)
        {
            this.path = path;
            this.Id = Path.GetFileName(path);
        }

        public string Id { get; }

        public string FullName { get { return this.path; } }

        /// <summary>
        /// Open read only stream
        /// </summary>
        /// <returns></returns>
        public Stream OpenStream()
        {
            return File.OpenRead(this.path);
        }

        /// <summary>
        /// Get resource as atext
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadTextAsync()
        {
            using (var stream = File.OpenRead(this.path))
            {
                using (TextReader textReader = new StreamReader(stream))
                {
                    return await textReader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
        }

        public string ReadText()
        {
            return File.ReadAllText(this.path);
        }

        public override string ToString()
        {
            return this.Id;
        }

    }
}
