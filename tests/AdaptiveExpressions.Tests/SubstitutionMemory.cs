using System;
using System.Collections.Generic;
using System.Text;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.Tests
{
    internal class SubstitutionMemory : IMemory
    {
        public void SetValue(string path, object value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string path, out object value, bool allowSubstitution)
        {
            if (allowSubstitution)
            {
                value = $"{path} is undefined";
            }
            else
            {
                value = null; 
            }

            return true;
        }

        public string Version()
        {
            return "0";
        }
    }
}
