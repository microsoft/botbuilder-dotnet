// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Runtime.Tests
{
    public class JObjectConfigurationProvider : ConfigurationProvider
    {
        private const char Separator = ':';

        private readonly JObject jObject;

        public JObjectConfigurationProvider(JObject jObject)
        {
            this.jObject = jObject ?? throw new ArgumentNullException(nameof(jObject));
        }

        public override void Load()
        {
            this.Data.Clear();
            this.ParseToken(token: this.jObject, pathStack: new Stack<string>(), current: null);
        }

        private void ParseToken(JToken token, Stack<string> pathStack, string current)
        {
            if (current != null)
            {
                pathStack.Push(current);
            }

            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (JProperty property in ((JObject)token).Properties())
                    {
                        this.ParseToken(property, pathStack, property.Name);
                    }

                    break;

                case JTokenType.Property:
                    this.ParseToken(((JProperty)token).Value, pathStack, current: null);
                    break;

                case JTokenType.Array:
                    var array = (JArray)token;
                    for (int i = 0; i < array.Count; i++)
                    {
                        this.ParseToken(array[i], pathStack, current: i.ToString());
                    }

                    break;

                case JTokenType.Boolean:
                case JTokenType.Date:
                case JTokenType.Float:
                case JTokenType.Guid:
                case JTokenType.Integer:
                case JTokenType.String:
                case JTokenType.TimeSpan:
                case JTokenType.Uri:
                    this.Data[string.Join(Separator, pathStack.Reverse())] = token.Value<string>();
                    break;

                case JTokenType.Null:
                case JTokenType.Undefined:
                    this.Data[string.Join(Separator, pathStack.Reverse())] = null;
                    break;
            }

            if (current != null)
            {
                pathStack.Pop();
            }
        }
    }
}
