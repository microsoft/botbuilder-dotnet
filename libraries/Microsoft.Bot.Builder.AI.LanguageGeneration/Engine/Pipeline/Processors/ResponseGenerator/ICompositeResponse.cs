using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The blueprint for composite request object, which contains a <see cref="IDictionary{string, string}"/> of template name as the key and template resolution as the value,
    /// example :
    /// TemplateResolutions
    /// {
    ///     {"SayHelloTemplate", "Hello Jack"},
    ///     {"OfferrHelpTemplate", "How can I help you Today?"}
    /// }
    /// </summary>
    internal interface ICompositeResponse
    {
        IDictionary<string, string> TemplateResolutions { get; set; }
    }
}
