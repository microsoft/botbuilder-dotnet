// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// return true if the two items are not equal.
    /// </summary>
    internal class NotEqual : ComparisonEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotEqual"/> class.
        /// </summary>
        public NotEqual()
            : base(
                  ExpressionType.NotEqual,
                  (args) => !FunctionUtils.CommonEquals(args[0], args[1]),
                  FunctionUtils.ValidateBinary)
        {
        }
    }
}
