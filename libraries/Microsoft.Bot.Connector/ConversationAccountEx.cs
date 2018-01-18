using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Connector
{
    public partial class ConversationAccount : IEquatable<ConversationAccount>
    {
        public bool Equals(ConversationAccount other)
        {
            return other != null
                && this.Id == other.Id
                && this.Name == other.Name
                && this.IsGroup == other.IsGroup;


        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ConversationAccount);
        }

        public override int GetHashCode()
        {
            var code
                = this.Id.GetHashCode()
                ^ this.Name.GetHashCode()
                ^ this.IsGroup.GetHashCode();
            return code;
        }

        /// <summary>
        /// Extension data for overflow of properties
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; set; } = new JObject();
    }
}
