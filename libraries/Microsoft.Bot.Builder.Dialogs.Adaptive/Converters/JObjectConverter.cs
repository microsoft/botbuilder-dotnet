// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    /// <summary>
    /// JsonConverter which handles resolving declarative types in JObjects using the ResourceExplorer.
    /// </summary>
    public class JObjectConverter : JsonConverter
    {
        private readonly ResourceExplorer resourceExplorer;
        private readonly Stack<SourceRange> context;

        /// <summary>
        /// Initializes a new instance of the <see cref="JObjectConverter"/> class.
        /// </summary>
        /// <param name="resourceExplorer">ResourceExplorer to use to resolve references.</param>
        /// <param name="context">source range context stack to build debugger source map.</param>
        public JObjectConverter(ResourceExplorer resourceExplorer, Stack<SourceRange> context)
        {
            this.resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
            => typeof(JObject) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = SourcePoint.ReadObjectWithSourcePoints(reader, JToken.Load, out var start, out var after);

            if (resourceExplorer.IsRef(jsonObject))
            {
                // We can't do this asynchronously as the Json.NET interface is synchronous
                jsonObject = resourceExplorer.ResolveRefAsync(jsonObject, context).GetAwaiter().GetResult();
            }

            return jsonObject as JObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
