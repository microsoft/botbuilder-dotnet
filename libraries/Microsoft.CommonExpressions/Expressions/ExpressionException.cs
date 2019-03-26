// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

﻿using System;

namespace Microsoft.Expressions
{
    public class ExpressionException : Exception
    {
        public ExpressionException(string msg, Expression expression = null)
            : base($"{msg}{(expression == null ? String.Empty : ": " + expression.ToString())}")
        {
        }
    }
}
