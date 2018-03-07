// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class Entity : FlexObject
    {
        public string GroupName { get; set; }
        public double Score { get; set; }

        public T ValueAs<T>()
        {
            string json = JsonConvert.SerializeObject(this["Value"]);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    //public class EntityObject<T> : Entity
    //{        

    //}

    //public class RecognizerNumberOptions
    //{
    //    public double? MinValue { get; set; }
    //    public double? MaxValue { get; set; }
    //    public bool? IntegersOnly { get; set; }
    //}

    //public class RecognizerValuesOptions
    //{

    //    /// <summary>
    //    /// (Optional) if true, then only some of the tokens in a value need to exist to be considered 
    //    /// a match.The default value is "false".
    //    /// </summary>
    //    public bool AllowPartialMatches { get; set; } = false;
    //    /// <summary>
    //    /// (Optional) maximum tokens allowed between two matched tokens in the utterance. So with
    //    /// a max distance of 2 the value "second last" would match the utternace "second from the last"
    //    /// but it wouldn't match "Wait a second. That's not the last one is it?". 
    //    /// The default value is "2". 
    //    /// </summary>
    //    public uint MaxTokenDistance { get; set; } = 2;
    //}

    //public class RecognizerChoicesOptions : RecognizerValuesOptions
    //{
    //    public bool ExcludeValue { get; set; } = true;
    //    public bool ExcludeAction { get; set; } = true;
    //}

    //public class Choice
    //{
    //    /// <summary>
    //    /// Value to return when selected
    //    /// </summary>
    //    public string Value { get; set; }

    //    /// <summary>
    //    /// (Optional) action to use when rendering the choice as a suggested action.
    //    /// </summary>
    //    public CardAction Action { get; set; } = null;

    //    /// <summary>
    //    /// (Optional) list of synonyms to recognize in addition to the value.
    //    /// </summary>
    //    public IList<string> Synonyms { get; set; } = new List<string>();
    //}

    //public class EntityRecognizers
    //{
    //    public IList<EntityObject<string>> RecognizeLocalizedRegExp(BotContext context, string expresionId)
    //    {
    //        string locale = context.Request.Locale ?? "en";
    //        string utterance = (string.IsNullOrWhiteSpace(context.Request.Text)) ? string.Empty : context.Request.Text.Trim();

    //        LocalizedEntities rules = Locales.Find(locale);
    //        if (rules != null)
    //        {

    //        }
    //    }
    //}
}
