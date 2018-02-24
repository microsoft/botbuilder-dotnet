using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prompts
{
    public static class PromptValidatorEx
    {
        /// <summary>
        /// Signature of a handler that can be passed to a prompt to provide additional validation logic
        /// or to customize the reply sent to the user when their response is invalid.
        /// </summary>
        /// <typeparam name="InT">Type of value that will recognized and passed to the validator as input</typeparam>
        /// <typeparam name="OutT">Type of output that will be returned by the validator. This can be changed from the input type by the validator.</typeparam>
        /// <param name="context">Context for the current turn of conversation.</param>
        /// <param name="toValidate"></param>
        /// <returns></returns>
        public delegate Task<(bool Passed, OutT Value)> PromptValidator<InT, OutT>(IBotContext context, InT toValidate);
    }
}