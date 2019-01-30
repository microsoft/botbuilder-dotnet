using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Plugins
{
    public interface IPlugin
    {
        string SchemaUri { get; }
        Type Type { get; }
        ILoader Loader { get; }
        Task Load();
    }
}
