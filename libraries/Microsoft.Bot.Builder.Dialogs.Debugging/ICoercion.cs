// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface ICoercion
    {
        object Coerce(object source, Type target);
    }

    public sealed class Coercion : ICoercion
    {
        public Coercion()
        {
        }

        object ICoercion.Coerce(object source, Type target)
        {
            var token = source as JToken;
            if (token != null)
            {
                return token.ToObject(target);
            }

            return Convert.ChangeType(source, target);
        }
    }
}
