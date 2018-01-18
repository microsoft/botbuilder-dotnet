using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Connector
{
    public partial class ChannelAccount : IEquatable<ChannelAccount>
    {
        public bool Equals(ChannelAccount other)
        {
            return other != null
                && this.Id == other.Id
                && this.Name == other.Name;

        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ChannelAccount);
        }

        public override int GetHashCode()
        {
            var code
                = this.Id.GetHashCode()
                ^ this.Name.GetHashCode();
            return code;
        }

        /// <summary>
        /// Extension data for overflow of properties
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; set; } = new JObject();
    }
}
