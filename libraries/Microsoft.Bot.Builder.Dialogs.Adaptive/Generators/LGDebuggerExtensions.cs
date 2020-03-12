using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Schema;
using LGTemplates = Microsoft.Bot.Builder.LanguageGeneration.Templates;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    public static class LGDebuggerExtensions
    {
        public static void RegisterSourceMap(this LGTemplates templates)
        {
            foreach (var t in templates)
            {
                var key = templates.Id + "#" + t.Name;
                var startLine = t.ParseTree.Start.Line;
                var startChar = t.ParseTree.Start.Column;
                var endLine = t.ParseTree.Stop.Line;
                var endChar = t.ParseTree.Stop.Column;
                var path = templates.Id;

                DebugSupport.SourceMap.Add(key, new SourceRange(path, startLine, startChar, endLine, endChar));
            }
        }
    }
}
