using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Resources
{
    public class FileResource : IBotResource
    {
        private byte[] bytes;
        private string text;

        public FileResource() { }

        public string Id { get; set; }
        public IBotResourceProvider Source { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string ResourceType { get; set; }

        public Task<byte[]> GetBinaryAsync()
        {
            if (bytes == null)
            {
                this.bytes = File.ReadAllBytes(this.Path);
            }

            return Task.FromResult(this.bytes);
        }

        public Task<string> GetTextAsync()
        {
            if (text == null)
            {
                this.text = File.ReadAllText(this.Path);
            }

            return Task.FromResult(this.text);
        }
    }
}
