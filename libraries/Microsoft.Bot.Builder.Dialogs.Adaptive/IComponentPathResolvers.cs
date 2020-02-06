using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public interface IComponentPathResolvers
    {
        IEnumerable<IPathResolver> GetPathResolvers(IDictionary<string, object> dependencies);
    }
}
