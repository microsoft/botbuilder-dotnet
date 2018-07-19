using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

////[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Classic.Tests")]
////[assembly: InternalsVisibleTo("Microsoft.Bot.Sample.Tests")]
[assembly: NeutralResourcesLanguage("en")]

[assembly: AssemblyTitle("Microsoft.Bot.Builder.Classic")]
[assembly: AssemblyDescription("Microsoft.Bot.Builder.Classic")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("Microsoft Bot Builder SDK")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

#if !DEBUG
[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile(@"..\..\..\..\..\build\35MSSharedLib1024.snk")]
#endif
