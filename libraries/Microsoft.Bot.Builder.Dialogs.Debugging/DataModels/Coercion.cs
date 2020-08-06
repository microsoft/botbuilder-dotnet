// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.DataModels
{
    internal sealed class Coercion : ICoercion
    {
        object ICoercion.Coerce(object source, Type target)
        {
            if (source is JToken token)
            {
                return token.ToObject(target);
            }

            return Convert.ChangeType(source, target, CultureInfo.InvariantCulture);
        }
    }
}
