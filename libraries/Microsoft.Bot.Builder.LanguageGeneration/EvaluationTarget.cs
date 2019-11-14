// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    internal class EvaluationTarget
    {
        public EvaluationTarget(string templateName, object scope)
        {
            TemplateName = templateName;
            Scope = scope;
        }

        public Dictionary<string, object> EvaluatedChildren { get; set; } = new Dictionary<string, object>();

        public string TemplateName { get; set; }

        public object Scope { get; set; }

        public string GetId()
        {
            var id = Scope == null ? string.Empty : Scope.GetHashCode().ToString() + ((CustomizedMemory)Scope).Version();
            return TemplateName + id;
        }
    }
}
