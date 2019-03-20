using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public class Variable : Expression
    {
        public Variable(string path, string name = null)
            : base(ExpressionType.Variable)
        {
            Path = path;
            Name = name;
        }

        public string Path { get; }

        public string Name { get; }

        public override string ToString()
        {
            return Name == null ? $"{{{Path}}}" : $"{Name}={{{Path}}}";
        }
    }
}
