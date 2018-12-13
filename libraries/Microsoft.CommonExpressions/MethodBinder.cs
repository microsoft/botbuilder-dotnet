using System;
using System.Collections.Generic;

namespace Microsoft.Expressions
{
    /// <summary>
    /// Get a method by name.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate EvaluationDelegate GetMethodDelegate(string name);

    /// <summary>
    /// Implementations of <see cref="GetMethodDelegate"/>.
    /// </summary>
    public static class MethodBinder
    {
        /// <summary>
        /// Default list of methods.
        /// </summary>
        public static GetMethodDelegate All { get; } = (string name) =>
         {
             switch (name)
             {
                 case "min":
                     return parameters =>
                         parameters[0] is IComparable c0 && parameters[1] is IComparable c1 ? (c0.CompareTo(c1) < 0 ? c0 : c1) :
                         throw new NotImplementedException();
                 case "max":
                     return parameters =>
                         parameters[0] is IComparable c0 && parameters[1] is IComparable c1 ? (c0.CompareTo(c1) > 0 ? c0 : c1) :
                         throw new NotImplementedException();
             }

             throw new NotImplementedException();
         };
    }
}
