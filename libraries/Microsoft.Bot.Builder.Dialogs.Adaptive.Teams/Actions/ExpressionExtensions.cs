// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Memory;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    internal static class ExpressionExtensions
    {
//#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
//        internal static T? GetValueOrNull<T>(this ExpressionProperty<T> expression, DialogStateManager state)
//#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
//        {
//            if (expression != null)
//            {
//                var (value, valueError) = expression.TryGetValue(state);
//                if (valueError != null)
//                {
//                    throw new Exception($"Expression evaluation resulted in an error. Expression: {expression.ExpressionText}. Error: {valueError}");
//                }

//                return value;
//            }

//            return default(T);
//        }

        internal static int? GetValueOrNull(this IntExpression expression, DialogStateManager state)
        {
            if (expression != null)
            {
                var (value, valueError) = expression.TryGetValue(state);
                if (valueError != null)
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {expression.ExpressionText}. Error: {valueError}");
                }

                return value;
            }

            return null;
        }

        internal static string GetValueOrNull(this StringExpression expression, DialogStateManager state)
        {
            if (expression != null)
            {
                var (value, valueError) = expression.TryGetValue(state);
                if (valueError != null)
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {expression.ExpressionText}. Error: {valueError}");
                }

                return value as string;
            }

            return null;
        }
    }
}
