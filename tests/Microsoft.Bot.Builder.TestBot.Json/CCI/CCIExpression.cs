using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Flow.Templates.Helpers;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.TestBot.Json.CCI
{
    internal class CCIExpression : IExpressionEval
    {
        private const string DefaultTriggerName = "trigger";
        private string expression;

        public CCIExpression(string expression)
        {
            this.expression = JsonConvert.SerializeObject(new
            {
                expression,
            });
        }

        public Task<object> Evaluate(IDictionary<string, object> vars)
        {
            bool flag = TemplateExpressionsHelper.IsTemplateLanguageExpression(expression);
            vars = vars ?? new Dictionary<string, object>();
            var variables = new InsensitiveDictionary<JToken>(vars.ToDictionary(kv => kv.Key, kv => kv.Value == null ? JValue.CreateNull() : JToken.FromObject(kv.Value)));
            var evaluationHelper = TemplateExpressionsHelper.GetTemplateFunctionEvaluationHelper(variables, DefaultTriggerName, JValue.CreateNull());

            var result = TemplateExpressionsHelper.EvaluateTemplateLanguageExpressionsRecursive(JToken.Parse(expression), evaluationHelper.EvaluationContext);

            return Task.FromResult(result["expression"].ToObject<object>());
        }
    }
}
