// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    public sealed class ResolutionParser : IResolutionParser
    {
        bool IResolutionParser.TryParse(IDictionary<string, object> properties, out Resolution resolution)
        {
            if (properties != null)
            {
                if (properties.TryGetValue("resolution_type", out var value) && value is string)
                {
                    switch (value as string)
                    {
                        case "builtin.datetime.date":
                            if (properties.TryGetValue("date", out value) && value is string)
                            {
                                if (BuiltIn.DateTime.DateTimeResolution.TryParse(value as string, out var dateTime))
                                {
                                    resolution = dateTime;
                                    return true;
                                }
                            }

                            break;
                        case "builtin.datetime.time":
                        case "builtin.datetime.set":
                            if (properties.TryGetValue("time", out value) && value is string)
                            {
                                if (BuiltIn.DateTime.DateTimeResolution.TryParse(value as string, out var dateTime))
                                {
                                    resolution = dateTime;
                                    return true;
                                }
                            }

                            break;
                    }
                }
            }

            resolution = null;
            return false;
        }
    }
}
