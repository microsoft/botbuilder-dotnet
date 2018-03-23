using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


//[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Classic.Tests")]
//[assembly: InternalsVisibleTo("Microsoft.Bot.Sample.Tests")]
[assembly: NeutralResourcesLanguage("en")]

#if (!DEBUG)
[assembly: AssemblyKeyFileAttribute(@"..\..\..\build\35MSSharedLib1024.snk")]
[assembly: AssemblyDelaySignAttribute(true)]
#endif
