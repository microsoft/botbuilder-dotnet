﻿using System;
using System.Collections.Generic;
using System.Text;

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
