// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Microsoft.Bot.Builder.Dialogs.Adaptive;

namespace Microsoft.Bot.Builder.Dialogs.Functions
{
    /// <summary>
    /// Defines missingProperties(template) expression function.
    /// </summary>
    /// <remarks>
    /// This expression will get all variables the template contains.
    /// </remarks>
    public class MissingPropertiesFunction : ExpressionEvaluator
    {
        /// <summary>
        /// Function identifier name.
        /// </summary>
        public const string Name = "missingProperties";

        private const string GeneratorPath = "dialogclass.generator";

        private static DialogContext dialogContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingPropertiesFunction"/> class.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        public MissingPropertiesFunction(DialogContext context)
            : base(Name, Function, ReturnType.Array, FunctionUtils.ValidateUnaryString)
        {
            dialogContext = context;
        }

        private static (object value, string error) Function(Expression expression, IMemory state, Options options)
        {
            var (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error != null)
            {
                return (null, error);
            }

            var templateBody = args[0]?.ToString();

            if (state.TryGetValue(GeneratorPath, out var lgGenerator))
            {
                var generator = lgGenerator as LanguageGenerator;
                return (generator.MissingProperties(dialogContext, templateBody, state, options), null);
            }

            return (new List<string>(), null);
        }
    }
}
