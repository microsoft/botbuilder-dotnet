using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    /// ChannelData for Activity template of type Template
    /// </summary>
    public class TemplateOptions
    {
        public string TemplateId { get; set; }
        public object Data { get; set; }
    }
}
