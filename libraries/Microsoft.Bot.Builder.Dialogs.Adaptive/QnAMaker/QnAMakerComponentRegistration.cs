// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.QnA;
using Microsoft.Bot.Builder.Dialogs.Adaptive.QnA.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    public class QnAMakerComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<TypeRegistration> GetTypes()
        {
            // Dialogs
            yield return new TypeRegistration<QnAMakerDialog2>(QnAMakerDialog2.DeclarativeType);

            // Recognizers
            yield return new TypeRegistration<QnAMakerRecognizer>(QnAMakerRecognizer.DeclarativeType);
        }

        public override IEnumerable<JsonConverter> GetConverters(ISourceMap sourceMap, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new ArrayExpressionConverter<Metadata>();

            yield break;
        }
    }
}
