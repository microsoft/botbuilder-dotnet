// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;

namespace Microsoft.Bot.Builder.MockLuis
{
    public class MockLuisComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<TypeRegistration> GetTypes()
        {
            // Recognizers
            yield return new TypeRegistration<MockLuisRecognizer>("Microsoft.LuisRecognizer") { CustomDeserializer = new MockLuisLoader(TypeFactory.Configuration) };
        }
    }
}
