using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Defines ChoiceSet collection.
    /// </summary>
    [JsonConverter(typeof(ChoiceSetConverter))]
    public class ChoiceSet : List<Choice>, ITemplate<ChoiceSet>
    {
        private string template;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceSet"/> class.
        /// </summary>
        public ChoiceSet()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceSet"/> class.
        /// </summary>
        /// <param name="choices">Choice values.</param>
        public ChoiceSet(IEnumerable<Choice> choices)
            : base(choices)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceSet"/> class.
        /// </summary>
        /// <param name="obj">Choice values.</param>
        public ChoiceSet(object obj)
        {
            if (obj is string template)
            {
                this.template = template;
            }
            else if (obj is IEnumerable<string> strings)
            {
                // support string[] => choice[]
                foreach (var str in strings)
                {
                    this.Add(new Choice(str));
                }
            }
            else if (obj is JArray array)
            {
                // support JArray to => choice
                if (array.HasValues)
                {
                    foreach (var element in array)
                    {
                        if (element is JValue jval)
                        {
                            this.Add(new Choice(element.ToString()));
                        }
                        else if (element is JObject jobj)
                        {
                            this.Add(jobj.ToObject<Choice>());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts a bool into a <see cref="ChoiceSet"/>.
        /// </summary>
        /// <param name="value">Bool expression.</param>
        public static implicit operator ChoiceSet(bool value) => new ChoiceSet(value);

        /// <summary>
        /// Converts a string into a <see cref="ChoiceSet"/>.
        /// </summary>
        /// <param name="value">String expression.</param>
        public static implicit operator ChoiceSet(string value) => new ChoiceSet(value);

        /// <summary>
        /// Converts a <see cref="JToken"/> into a <see cref="ChoiceSet"/>.
        /// </summary>
        /// <param name="value"><see cref="JToken"/> expression.</param>
        public static implicit operator ChoiceSet(JToken value) => new ChoiceSet(value);

        /// <inheritdoc/>
        public async Task<ChoiceSet> BindAsync(DialogContext dialogContext, object data = null, CancellationToken cancellationToken = default)
        {
            if (this.template == null)
            {
                return this;
            }

            var languageGenerator = dialogContext.Services.Get<LanguageGenerator>() ?? throw new MissingMemberException(nameof(LanguageGeneration));
            var lgResult = await languageGenerator.GenerateAsync(dialogContext, this.template, dialogContext.State).ConfigureAwait(false);
            if (lgResult is ChoiceSet cs)
            {
                return cs;
            }
            else if (lgResult is string str)
            {
                try
                {
                    var jObj = (JToken)JsonConvert.DeserializeObject(str);

                    if (jObj is JArray jarr)
                    {
                        return new ChoiceSet(jarr);
                    }

                    return jObj.ToObject<ChoiceSet>();
                }
                catch (JsonReaderException)
                {
                    return new ChoiceSet(str.Split('|').Select(t => t.Trim()));
                }
            }

            return new ChoiceSet(lgResult);
        }
    }
}
