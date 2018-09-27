using System;
using System.Collections.Generic;
using System.Text;
using DialogFoundation.Backend.LG;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The concrete class for composite response object, which contains a <see cref="IDictionary{string, LGRequest}"/> of template name as the key and a <see cref="LGRequest"/> object as the value,
    /// </summary>
    internal class CompositeRequest : ICompositeRequest
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CompositeRequest()
        {
            Requests = new Dictionary<string, LGRequest>();
        }
        public IDictionary<string, LGRequest> Requests { get; set; }
    }
}
