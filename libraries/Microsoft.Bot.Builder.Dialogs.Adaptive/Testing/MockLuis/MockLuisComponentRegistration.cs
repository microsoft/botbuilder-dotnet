// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.MockLuis
{
    public class MockLuisComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<TypeRegistration> GetTypes()
        {
            // Recognizers
            yield return new TypeRegistration<MockLuisRecognizer>("Microsoft.LuisRecognizer") { CustomDeserializer = new MockLuisLoader(TypeFactory.Configuration) };
        }

        public override IEnumerable<JsonConverter> GetConverters(ISourceMap sourceMap, IRefResolver refResolver, Stack<string> paths)
        {
            foreach (var converter in new LuisComponentRegistration().GetConverters(sourceMap, refResolver, paths))
            {
                yield return converter;
            }
        }
    }
}
