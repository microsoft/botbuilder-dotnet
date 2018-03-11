using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Microsoft.Bot.Builder.Ai
{

    //Struct used to store date format and time format for different locales
    internal struct DateAndTimeLocaleFormat
    { 
        public string TimeFormat { get; set; }
        public string DateFormat { get;set; }

    }

    //Struct to store  text and date time object from Microsoft Recognizer recognition result
    internal struct TextAndDateTime
    { 
        public string Text { get; set; }
        public DateTime DateTimeObj { get; set; }
    }

    /// <summary>
    /// Locale Converter Class used to convert between locales
    /// in terms of date and time only
    /// </summary>
    public class LocaleConverter : ILocaleConverter
    { 
        private Dictionary<string, DateAndTimeLocaleFormat> mapLocaleToFunction = new Dictionary<string, DateAndTimeLocaleFormat>();

        public LocaleConverter()
        {
            initLocales();
        }

        //Init different locales format
        //Supporting English ,French deutch and chinese locales
        private void initLocales()
        {
            DateAndTimeLocaleFormat yearMonthDay = new DateAndTimeLocaleFormat
            {
                TimeFormat = "{0:hh:mm tt}",
                DateFormat = "{0:yyyy-MM-dd}"
            };
            DateAndTimeLocaleFormat dayMonthYear = new DateAndTimeLocaleFormat
            {
                TimeFormat = "{0:hh:mm tt}",
                DateFormat = "{0:dd/MM/yyyy}"
            };
            DateAndTimeLocaleFormat monthDayYEar = new DateAndTimeLocaleFormat
            {
                TimeFormat = "{0:hh:mm tt}",
                DateFormat = "{0:MM/dd/yyyy}"
            };
            foreach (string locale in new string[] { "en-za", "en-ie", "en-gb", "en-ca", "fr-ca", "zh-cn", "zh-sg", "zh-hk", "zh-mo", "zh-tw" })
            {
                mapLocaleToFunction[locale] = yearMonthDay;
            }
            foreach (string locale in new string[] { "en-au", "fr-be", "fr-ch", "fr-fr", "fr-lu", "fr-mc", "de-at", "de-ch", "de-de", "de-lu", "de-li" })
            {
                mapLocaleToFunction[locale] = dayMonthYear;
            }
            mapLocaleToFunction["en-us"] = monthDayYEar;
        }

        //check if a specific locale is available
        public bool IsLocaleAvailable(string locale)
        {
            return mapLocaleToFunction.ContainsKey(locale);
        }

        //extract date and time from a sentence using Microsoft Recognizer
        private List<TextAndDateTime> extractDate(string message, string fromLocale)
        {
            List<TextAndDateTime> fndDates = new List<TextAndDateTime>();
            var culture = Culture.English; 
            if (fromLocale.StartsWith("fr"))
            {
                culture = Culture.French;
            }
            else if (fromLocale.StartsWith("de"))
            {
                culture = Culture.German;
            }
            else if (fromLocale.StartsWith("pt"))
            {
                culture = Culture.Portuguese;
            }
            else if (fromLocale.StartsWith("zh"))
            {
                culture = Culture.Chinese;
            }
            else if (fromLocale.StartsWith("es"))
            {
                culture = Culture.Spanish;
            }
            else if(!fromLocale.StartsWith("en"))
            {
                throw (new ArgumentException("Unsupported From Locale"));
            }
            var model = DateTimeRecognizer.GetInstance().GetDateTimeModel(culture);
            var results = model.Parse(message);
            foreach (ModelResult result in results)
            {
                var resolutionValues = (IList<Dictionary<string, string>>)result.Resolution["values"];
                string type = result.TypeName.Split('.').Last();
                DateTime moment = new DateTime();
                if (type.Contains("date") && !type.Contains("range"))
                {
                    moment = resolutionValues.Select(v => DateTime.Parse(v["value"])).FirstOrDefault();

                }
                else if (type.Contains("date") && type.Contains("range"))
                {
                    moment = DateTime.Parse(resolutionValues.First()["start"]);
                }
                else
                    continue;
                var curDateTimeText = new TextAndDateTime
                {
                    DateTimeObj = moment,
                    Text = result.Text
                };
                fndDates.Add(curDateTimeText);
            }
            return fndDates;
        }

        //Convert a message from locale to another locale
        public async Task<string> Convert(string message, string fromLocale, string toLocale)
        {
            List<TextAndDateTime> dates = extractDate(message, fromLocale);
            if (!mapLocaleToFunction.ContainsKey(toLocale))
            {
                throw (new ArgumentException("Unsupported To Locale"));
            }
            if (dates.Count == 0)
                return  message;
            string processedMessage = message;
            foreach (TextAndDateTime date in dates)
            {
                if (date.DateTimeObj.Date == DateTime.Now.Date)
                {
                    processedMessage = processedMessage.Replace(date.Text, String.Format(mapLocaleToFunction[toLocale].TimeFormat, date.DateTimeObj));
                }
                else
                {
                    processedMessage = processedMessage.Replace(date.Text, String.Format(mapLocaleToFunction[toLocale].DateFormat, date.DateTimeObj));
                }
            }
            return  processedMessage;
        }

        //Get all supported Locales
        public string[] GetAvailableLocales()
        {
            return mapLocaleToFunction.Keys.ToArray();
        }
    }
}
