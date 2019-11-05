using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Memory
{
    internal class SimpleObjectMemory : IMemory
    {
        private object memory = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleObjectMemory"/> class.
        /// This wraps a simple object as IMemory.
        /// </summary>
        /// <param name="memory">the object to wrap.</param>
        public SimpleObjectMemory(object memory)
        {
            this.memory = memory;
        }

        public (object value, string error) GetValue(string path)
        {
            var parts = path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            object value = null;
            var curScope = memory;

            foreach (string part in parts)
            {
                string error = null;
                if (int.TryParse(part, out var idx))
                {
                    (value, error) = BuiltInFunctions.AccessIndex(curScope, idx);
                }
                else
                {
                    (value, error) = BuiltInFunctions.AccessProperty(curScope, part);
                }

                if (error != null)
                {
                    return (null, error);
                }

                curScope = value;
            }

            return (value, null);
        }

        public (object value, string error) SetValue(string path, object value)
        {
            throw new NotImplementedException();
        }
    }
}
