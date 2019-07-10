using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Utilities.XmlUtility
{
    public static class XmlUtility
    {
        public static XDocument Convert(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (XmlReader xr = XmlReader.Create(stream))
            {
                return XDocument.Load(xr);
            }
        }
    }
}
