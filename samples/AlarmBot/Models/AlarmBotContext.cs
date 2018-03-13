using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlarmBot.Models
{
    public class AlarmBotContext : BotContextWrapper
    {
        public AlarmBotContext(IBotContext context) : base(context)
        {
        }

        /// <summary>
        /// Persisted AlarmBot Conversation State 
        /// </summary>
        public ConversationData ConversationState
        {
            get
            {
                return ConversationState<ConversationData>.Get(this);
            }
        }

        /// <summary>
        /// Persisted AlarmBot User State
        /// </summary>
        public UserData UserState
        {
            get
            {
                return UserState<UserData>.Get(this);
            }
        }

        /// <summary>
        /// AlarmBot recognized Intents for the incoming request
        /// </summary>
        public IRecognizedIntents RecognizedIntents { get { return this.Get<IRecognizedIntents>(); } }

        public IList<DateTime> GetDateTimes()
        {
            IList<DateTime> times = new List<DateTime>();
            // Get DateTime model for English
            var model = new DateTimeRecognizer(this.Request.Locale ?? "en-us").GetDateTimeModel();
            var results = model.Parse(this.Request.Text);

            // Check there are valid results
            if (results.Any() && results.First().TypeName.StartsWith("datetimeV2"))
            {
                // The DateTime model can return several resolution types (https://github.com/Microsoft/Recognizers-Text/blob/master/.NET/Microsoft.Recognizers.Text.DateTime/Constants.cs#L7-L14)
                // We only care for those with a date, date and time, or date time period:
                // date, daterange, datetime, datetimerange

                return results.Where(result =>
                {
                    var subType = result.TypeName.Split('.').Last();
                    return (subType.Contains("date") || subType.Contains("time")) && !subType.Contains("range");
                })
                .Select(result =>
                {
                    var resolutionValues = (IList<Dictionary<string, string>>)result.Resolution["values"];
                    return resolutionValues.Select(v => DateTime.Parse(v["value"]));
                }).SelectMany(l => l).ToList();
            }
            return times;
        }

    }
}
