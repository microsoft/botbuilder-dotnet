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

        public (object value, string error) GetValue(string path)
        {
            var prefix = path.Split('.')[0];
            if (scopeMap.TryGetValue(prefix, out var scope))
            {
                return scope.GetValue(path.Substring(prefix.Length+1)); // +1 to swallow the "."
            }
            return (null, $"path not exists at {path}");
        }

        public (object value, string error) SetValue(string path, object value)
        {
            throw new NotImplementedException();
        }
    }
}
