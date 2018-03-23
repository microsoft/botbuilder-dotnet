using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


//[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Classic.Tests")]
//[assembly: InternalsVisibleTo("Microsoft.Bot.Sample.Tests")]
[assembly: NeutralResourcesLanguage("en")]

#if (!DEBUG)
[assembly: AssemblyKeyFile(@"..\\35MSSharedLib1024.snk")]
[assembly: AssemblyDelaySign(true)]
#endif
