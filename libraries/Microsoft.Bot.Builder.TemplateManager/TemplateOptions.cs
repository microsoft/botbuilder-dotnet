using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    /// ChannelData for Activity template of type Template.
    /// </summary>
    public class TemplateOptions
    {
        /// <summary>
        /// Gets or sets the id of the template.
        /// </summary>
        [JsonProperty(PropertyName = "template")]
#pragma warning disable SA1609 // Property documentation should have value
        public string TemplateId { get; set; }
#pragma warning restore SA1609 // Property documentation should have value

        /// <summary>
        /// Gets or sets the data of the template.
        /// </summary>
        [JsonProperty(PropertyName = "data")]
#pragma warning disable SA1609 // Property documentation should have value
        public object Data { get; set; }
#pragma warning restore SA1609 // Property documentation should have value
    }
}
