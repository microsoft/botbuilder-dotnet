using System;
using System.Collections.Generic;

namespace Microsoft.Expressions
{

    /// Delegate which evaluates operators operands (aka the paramters) to the result
    /// </summary>
    public delegate object EvaluationDelegate(IReadOnlyList<object> parameters);
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

        private static readonly Dictionary<string, EvaluationDelegate> FunctionMap = new Dictionary<string, EvaluationDelegate>
        {
            {"/", BuildinFunctions.Div},
            {"*", BuildinFunctions.Mul},
            {"+", BuildinFunctions.Add},
            {"-", BuildinFunctions.Sub},
            {"==", BuildinFunctions.Equal},
            {"!=", BuildinFunctions.NotEqual},
            {"max", BuildinFunctions.Max},
            {"min", BuildinFunctions.Min},
            {"<", BuildinFunctions.LessThan},
            {"<=", BuildinFunctions.LessThanOrEqual},
            {">", BuildinFunctions.GreaterThan},
            {">=", BuildinFunctions.GreaterThanOrEqual},
        };


        /// <summary>
        /// Default list of methods.
        /// </summary>
        public static GetMethodDelegate All { get; } = (string name) =>
         {
             if (FunctionMap.ContainsKey(name))
                 return FunctionMap[name];

             throw new Exception($"Operation {name} is invalid.");
         };
    }
}
