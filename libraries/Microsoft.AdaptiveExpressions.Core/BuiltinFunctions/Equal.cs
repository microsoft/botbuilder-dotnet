﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Microsoft.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Check whether both values, expressions, or objects are equivalent.
    /// Return true if both are equivalent, or return false if they're not equivalent.
    /// </summary>
    internal class Equal : ComparisonEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Equal"/> class.
        /// </summary>
        public Equal()
            : base(
                  ExpressionType.Equal,
                  (args, state) => FunctionUtils.CommonEquals(args[0], args[1], state),
                  FunctionUtils.ValidateBinary)
        {
        }
    }
}
