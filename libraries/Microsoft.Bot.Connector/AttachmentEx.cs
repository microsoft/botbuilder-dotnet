namespace Microsoft.Bot.Connector
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public partial class Attachment : IEquatable<Attachment>
    {
        public bool Equals(Attachment other)
        {
            return other != null
                && this.ContentType == other.ContentType
                && this.ContentUrl == other.ContentUrl
                && object.Equals(this.Content, other.Content)
                && object.Equals(this.Name, other.Name)
                && object.Equals(this.ThumbnailUrl, other.ThumbnailUrl);

        }

        public override bool Equals(object other)
        {
            return this.Equals(other as Attachment);
        }

        public override int GetHashCode()
        {
            var code = this.ContentType.GetHashCode()
                ^ this.ContentUrl.GetHashCode()
                ^ (this.Content == null ? 13 : this.Content.GetHashCode())
                ^ (this.Name == null ? 17 : this.Name.GetHashCode())
                ^ (this.ThumbnailUrl == null ? 23 : this.ThumbnailUrl.GetHashCode());

            return code;
        }

        /// <summary>
        /// Extension data for overflow of properties
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; set; } = new JObject();
    }
}
