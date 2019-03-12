using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers
{
    public interface IDocumentLoader
    {
        JToken Load(string target);
    }
}
