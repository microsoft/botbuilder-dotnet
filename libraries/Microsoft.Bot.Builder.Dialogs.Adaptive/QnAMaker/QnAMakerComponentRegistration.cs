// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.QnA;
using Microsoft.Bot.Builder.Dialogs.Adaptive.QnA.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Expressions.Properties.Converters;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    public class QnAMakerComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public IEnumerable<DeclarativeType> GetDeclarativeTypes()
        {
            // Dialogs
            yield return new DeclarativeType<QnAMakerDialog2>(QnAMakerDialog2.DeclarativeType);

            // Recognizers
            yield return new DeclarativeType<QnAMakerRecognizer>(QnAMakerRecognizer.DeclarativeType);
        }

        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, Stack<string> paths)
        {
            yield return new ArrayExpressionConverter<Metadata>();
        }
    }
}
