using System.Collections.Generic;

namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModule
{
    public class SyntaxnetChecker
    {
        private Checker checker;
        private SyntaxnetModule syntaxnetModule;

        public SyntaxnetChecker()
        {
            this.syntaxnetModule = new SyntaxnetModule();
            this.syntaxnetModule.InitSyntaxModule();
            this.checker = new Checker(syntaxnetModule);
        }

        ~SyntaxnetChecker()
        {
            this.syntaxnetModule.Dispose();
        }

        public string CheckText(string text)
        {
            return this.checker.CheckText(text);
        }
    }
}
