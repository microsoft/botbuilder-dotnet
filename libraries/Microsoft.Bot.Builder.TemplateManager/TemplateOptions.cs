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
        /// <value>
        /// The id of the template.
        /// </value>
        [JsonProperty(PropertyName = "template")]
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the data of the template.
        /// </summary>
        /// <value>
        /// The data of the template.
        /// </value>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }
    }
}
