﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class NotEqual : ComparisonEvaluator
    {
        public NotEqual(string alias = null)
            : base(
                  alias ?? ExpressionType.NotEqual,
                  (args) => !Function(args),
                  FunctionUtils.ValidateBinary)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            if (args[0] == null || args[1] == null)
            {
                // null will only equals to null
                return args[0] == null && args[1] == null;
            }

            if (FunctionUtils.TryParseList(args[0], out IList l0) && l0.Count == 0 && (FunctionUtils.TryParseList(args[1], out IList l1) && l1.Count == 0))
            {
                return true;
            }

            if (GetPropertyCount(args[0]) == 0 && GetPropertyCount(args[1]) == 0)
            {
                return true;
            }

            if (args[0].IsNumber() && args[0].IsNumber())
            {
                if (Math.Abs(FunctionUtils.CultureInvariantDoubleConvert(args[0]) - FunctionUtils.CultureInvariantDoubleConvert(args[1])) < 0.00000001)
                {
                    return true;
                }
            }

            try
            {
                return args[0] == args[1] || (args[0] != null && args[0].Equals(args[1]));
            }
            catch
            {
                return false;
            }
        }

        private static int GetPropertyCount(object obj)
        {
            if (obj is IDictionary dictionary)
            {
                return dictionary.Count;
            }
            else if (obj is JObject jobj)
            {
                return jobj.Properties().Count();
            }
            else if (!(obj is JValue) && obj.GetType().IsValueType == false && obj.GetType().FullName != "System.String")
            {
                // exclude constant type.
                return obj.GetType().GetProperties().Length;
            }

            return -1;
        }
    }
}
