using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Plugins
{
    public class FileDependencyInfo
    {
        public string AssemblyPath { get; set; }
        public string SchemaUri { get; set; }
        public string ClassName { get; set; }
        public string CustomLoaderClassName { get; set; }
    }
}
