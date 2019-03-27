// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Expressions
{
    public interface IExpressionParser
    {
        Expression Parse(string expression);
    }
}
