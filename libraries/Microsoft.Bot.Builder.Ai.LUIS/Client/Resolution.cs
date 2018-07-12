// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Text;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    public abstract class Resolution
    {
        public override string ToString()
        {
            var builder = new StringBuilder();
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                if (value != null)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(",");
                    }

                    builder.Append(property.Name);
                    builder.Append("=");
                    builder.Append(value);
                }
            }

            return builder.ToString();
        }
    }
}
