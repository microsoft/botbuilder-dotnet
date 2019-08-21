using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    /// ChannelData for Activity template of type Template.
    /// </summary>
    public class TemplateOptions
    {
        [JsonProperty(PropertyName = "template")]
        public string TemplateId { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }
    }
}
