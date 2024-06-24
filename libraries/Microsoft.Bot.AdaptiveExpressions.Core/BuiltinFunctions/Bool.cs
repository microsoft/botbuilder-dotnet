// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Bot.AdaptiveExpressions.Core.Memory;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Return the Boolean version of a value.
    /// </summary>
    internal class Bool : ComparisonEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bool"/> class.
        /// </summary>
        public Bool()
            : base(
                  ExpressionType.Bool,
                  Function,
                  FunctionUtils.ValidateUnary)
        {
        }

        private static bool Function(IReadOnlyList<object> args, IMemory state)
        {
            var arg = args[0];
            if (arg is int @int)
            {
                arg = @int != 0;
            }

            if (arg is string @string)
            {
                try
                {
                    arg = Convert.ToBoolean(@string, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    // Any string other than 'true' or 'false' will throw an exception. We'll ignore them.
                }
            }

            return FunctionUtils.IsLogicTrue(arg);
        }
    }
}
