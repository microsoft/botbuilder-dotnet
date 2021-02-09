// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Memory;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    internal static class ExpressionExtensions
    {
        internal static int? GetValueOrNull(this IntExpression expression, DialogStateManager state)
        {
            if (expression != null)
            {
                var (value, valueError) = expression.TryGetValue(state);
                if (valueError != null)
                {
                    throw new InvalidOperationException($"Expression evaluation resulted in an error. Expression: {expression.ExpressionText}. Error: {valueError}");
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
                    throw new InvalidOperationException($"Expression evaluation resulted in an error. Expression: {expression.ExpressionText}. Error: {valueError}");
                }

                return value as string;
            }

            return null;
        }
    }
}
