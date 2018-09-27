using System;
using System.Collections.Generic;
using System.Text;
using DialogFoundation.Backend.LG;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The blueprint for composite response object, which contains a <see cref="IDictionary{string, LGRequest}"/> of template name as the key and a <see cref="LGRequest"/> object as the value,
    /// </summary>
    internal interface ICompositeRequest
    {
        IDictionary<string, LGRequest> Requests { get; set; }
    }
}
