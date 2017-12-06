using Microsoft.Bot.Builder;
using Microsoft.Recognizers.Text.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlarmBot
{
    public static class DateTimeRecognizerExtensions
    {
        public static IList<DateTime> GetDateTimes(this BotContext context)
        {
            IList<DateTime> times = new List<DateTime>();
            //if (context.TopIntent != null && context.TopIntent.Entities != null && context.TopIntent.Entities.Any())
            //{
            //    entities = context.TopIntent.Entities.Where(entity => entity.GroupName == ;
            //}
            //else
            {
                // Get DateTime model for English
                var model = DateTimeRecognizer.GetInstance().GetDateTimeModel(context.Request.Locale ?? "en-us");
                var results = model.Parse(context.Request.Text);

                // Check there are valid results
                if (results.Any() && results.First().TypeName.StartsWith("datetimeV2"))
                {
                    // The DateTime model can return several resolution types (https://github.com/Microsoft/Recognizers-Text/blob/master/.NET/Microsoft.Recognizers.Text.DateTime/Constants.cs#L7-L14)
                    // We only care for those with a date, date and time, or date time period:
                    // date, daterange, datetime, datetimerange

                    var first = results.First();
                    var resolutionValues = (IList<Dictionary<string, string>>)first.Resolution["values"];

                    var subType = first.TypeName.Split('.').Last();
                    if (subType.Contains("time") && !subType.Contains("range"))
                    {
                        // a date (or date & time) or multiple
                        times = resolutionValues.Select(v => DateTime.Parse(v["value"])).ToList();
                    }
                }
            }
            return times;
        }
    }
}
