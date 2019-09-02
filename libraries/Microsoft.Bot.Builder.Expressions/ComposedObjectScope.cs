using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Expressions
{
    // Combine serveral IMemoryScope as one memory scope
    // Designed for evaluating "foreach"
    internal class ComposedObjectScope : IMemoryScope
    {
        private Dictionary<string, IMemoryScope> scopeMap;

        public ComposedObjectScope(Dictionary<string, IMemoryScope> scopeMap)
        {
            this.scopeMap = scopeMap;
        }

        public object GetValue(string path)
        {
            var prefix = path.Split('.')[0];
            if (scopeMap.TryGetValue(prefix, out var scope))
            {
                return scope.GetValue(path.Substring(prefix.Length));
            }
            return null;
        }

        public object SetValue(string path, object value)
        {
            throw new NotImplementedException();
        }
    }
}
