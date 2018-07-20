using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Represents an <see cref="Activity"/> that includes an abstract value of some kind.
    /// </summary>
    public abstract class ActivityWithValue : Activity
    {
        protected ActivityWithValue(string type) : base(type)
        {
        }

        /// <summary>
        /// Gets or sets unique string which identifies the shape of the value
        /// object
        /// </summary>
        [JsonProperty(PropertyName = "valueType")]
        public string ValueType { get; set; }

        /// <summary>
        /// Gets or sets open-ended value
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }
    }
}
