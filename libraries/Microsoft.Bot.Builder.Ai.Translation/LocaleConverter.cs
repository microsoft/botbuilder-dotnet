// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// DateAndTimeLocaleFormat Class used to store date format and time format
    /// for different locales.
    /// </summary> 
    internal class DateAndTimeLocaleFormat
    {
        public string TimeFormat { get; set; }
        public string DateFormat { get; set; }

    }

    /// <summary>
    /// TextAndDateTime Class used to store  text and date time object 
    /// from Microsoft Recognizer recognition result.
    /// </summary> 
    internal class TextAndDateTime
    {
        public string Text { get; set; }
        public DateTime dateTime { get; set; }
        public string type { get; set; }
        public bool range { get; set; }
        public DateTime endDateTime { get; set; }
    }

    /// <summary>
    /// Locale Converter Class Converts dates and times 
    /// between different locales.
    /// </summary>
    public class LocaleConverter : ILocaleConverter
    {

        private static readonly ConcurrentDictionary<string, DateAndTimeLocaleFormat> _mapLocaleToFunction = new ConcurrentDictionary<string, DateAndTimeLocaleFormat>();
        private static LocaleConverter _localeConverter;

        public static LocaleConverter Converter
        {
            get
            {

                if (_localeConverter == null)
                {
                    _localeConverter = new LocaleConverter();
                }
                return _localeConverter;
            }
        }
        private LocaleConverter()
        {
            InitLocales();
        }

        /// <summary>
        /// Init different locales format,
        /// Supporting English, French, Deutsche and Chinese Locales.
        /// </summary>
        private void InitLocales()
        {
            if (_mapLocaleToFunction.Count > 0)
                return;
            var supportedLocales = new string[]
            {
                "en-us", "en-za", "en-ie", "en-gb", "en-ca", "fr-ca", "zh-cn", "zh-sg", "zh-hk", "zh-mo", "zh-tw",
                "en-au", "fr-be", "fr-ch", "fr-fr", "fr-lu", "fr-mc", "de-at", "de-ch", "de-de", "de-lu", "de-li"
            };
            foreach (string locale in supportedLocales)
            {
                CultureInfo cultureInfo = new CultureInfo(locale);
                var dateTimeInfo = new DateAndTimeLocaleFormat()
                {
                    DateFormat = $"{{0:{cultureInfo.DateTimeFormat.ShortDatePattern}}}",
                    TimeFormat = $"{{0:{cultureInfo.DateTimeFormat.ShortTimePattern}}}"
                };
                _mapLocaleToFunction[locale] = dateTimeInfo;
            }
        }

        /// <summary>
        /// Check if a specific locale is available.
        /// </summary>
        /// <param name="locale">input locale that we need to check if available</param>
        /// <returns>true if the locale is found, otherwise false.</returns>
        public bool IsLocaleAvailable(string locale)
        {
            AssertValidLocale(locale);
            return _mapLocaleToFunction.ContainsKey(locale);
        }

        private static void AssertValidLocale(string locale)
        {
            if (string.IsNullOrWhiteSpace(locale))
                throw new ArgumentNullException(nameof(locale));
        }

        /// <summary>
        /// Extract date and time from a sentence using Microsoft Recognizer
        /// </summary>
        /// <param name="message">input user message</param>
        /// <param name="fromLocale">Source Locale</param>
        /// <returns></returns>
        private List<TextAndDateTime> ExtractDate(string message, string fromLocale)
        {
            List<TextAndDateTime> fndDates = new List<TextAndDateTime>();
            //extract culture name.
            var cultureName = FindCulture(fromLocale);
            var results = DateTimeRecognizer.RecognizeDateTime(message, cultureName);
            //looping on each result and extracting found date objects from input utterance
            foreach (ModelResult result in results)
            {
                var resolutionValues = (IList<Dictionary<string, string>>)result.Resolution["values"];
                string type = result.TypeName.Replace("datetimeV2.", "");
                DateTime moment; ;
                string momentType;
                DateTime momentEnd;
                TextAndDateTime curDateTimeText;
                if (type.Contains("range"))
                {
                    if (type.Contains("date") && type.Contains("time"))
                    {
                        momentType = "datetime";
                    }
                    else if (type.Contains("date"))
                    {
                        momentType = "date";
                    }
                    else
                    {
                        momentType = "time";
                    }
                    moment = DateTime.Parse(resolutionValues.First()["start"]);
                    momentEnd = DateTime.Parse(resolutionValues.First()["end"]);
                    curDateTimeText = new TextAndDateTime
                    {
                        dateTime = moment,
                        Text = result.Text,
                        type = momentType,
                        range = true,
                        endDateTime = momentEnd
                    };

                }
                else
                {
                    if (type.Contains("date") && type.Contains("time"))
                    {
                        momentType = "datetime";
                    }
                    else if (type.Contains("date"))
                    {
                        momentType = "date";
                    }
                    else
                    {
                        momentType = "time";
                    }
                    moment = resolutionValues.Select(v => DateTime.Parse(v["value"])).FirstOrDefault();
                    curDateTimeText = new TextAndDateTime
                    {
                        dateTime = moment,
                        Text = result.Text,
                        type = momentType,
                        range = false,
                    };
                }
                
                
                fndDates.Add(curDateTimeText);
            }
            return fndDates;
        }

        private static string FindCulture(string fromLocale)
        {
            string culture = fromLocale.Split('-')[0];
            if (fromLocale.StartsWith("fr"))
            {
                return Culture.French;
            }
            else if (fromLocale.StartsWith("de"))
            {
                return Culture.German;
            }
            else if (fromLocale.StartsWith("pt"))
            {
                return Culture.Portuguese;
            }
            else if (fromLocale.StartsWith("zh"))
            {
                return Culture.Chinese;
            }
            else if (fromLocale.StartsWith("es"))
            {
                return Culture.Spanish;
            }
            else if (fromLocale.StartsWith("en"))
            {
                return Culture.English;
            }
            else
            {
                throw (new InvalidOperationException($"Unsupported from locale: {fromLocale}"));
            }
        }

        /// <summary>
        /// Convert a message from locale to another locale
        /// </summary>
        /// <param name="message"> input user message</param>
        /// <param name="fromLocale">Source Locale</param>
        /// <param name="toLocale">Target Locale</param>
        /// <returns></returns>
        public string Convert(string message, string fromLocale, string toLocale)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw (new ArgumentException("Empty message"));
            }
            List<TextAndDateTime> dates = ExtractDate(message, fromLocale);
            if (!IsLocaleAvailable(toLocale))
            {
                throw (new InvalidOperationException($"Unsupported from locale: {toLocale}"));
            }
            string processedMessage = message;
            foreach (TextAndDateTime date in dates)
            {
                if (date.range)
                {
                    if (date.type == "time")
                    {
                        var timeRange = $"{String.Format(_mapLocaleToFunction[toLocale].TimeFormat, date.dateTime)} - {String.Format(_mapLocaleToFunction[toLocale].TimeFormat, date.endDateTime)}";
                        processedMessage = Regex.Replace(processedMessage, $"\\b{date.Text}\\b", timeRange, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    }
                    else if (date.type == "date")
                    {
                        var dateRange = $"{String.Format(_mapLocaleToFunction[toLocale].DateFormat, date.dateTime)} - {String.Format(_mapLocaleToFunction[toLocale].DateFormat, date.endDateTime)}";
                        processedMessage = Regex.Replace(processedMessage, $"\\b{date.Text}\\b", dateRange, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        var convertedStartDate = String.Format(_mapLocaleToFunction[toLocale].DateFormat, date.dateTime);
                        var convertedStartTime = String.Format(_mapLocaleToFunction[toLocale].TimeFormat, date.dateTime);

                        var convertedEndDate = String.Format(_mapLocaleToFunction[toLocale].DateFormat, date.endDateTime);
                        var convertedEndTime = String.Format(_mapLocaleToFunction[toLocale].TimeFormat, date.endDateTime);
                        processedMessage = Regex.Replace(processedMessage, $"\\b{date.Text}\\b", $"{convertedStartDate} {convertedStartTime} - {convertedEndDate} {convertedEndTime}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    }
                }
                else
                {
                    if (date.type == "time")
                    {
                        processedMessage = Regex.Replace(processedMessage, $"\\b{date.Text}\\b", String.Format(_mapLocaleToFunction[toLocale].TimeFormat, date.dateTime), RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    }
                    else if (date.type == "date")
                    {
                        processedMessage = Regex.Replace(processedMessage, $"\\b{date.Text}\\b", String.Format(_mapLocaleToFunction[toLocale].DateFormat, date.dateTime), RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        var convertedDate = String.Format(_mapLocaleToFunction[toLocale].DateFormat, date.dateTime);
                        var convertedTime = String.Format(_mapLocaleToFunction[toLocale].TimeFormat, date.dateTime);
                        processedMessage = Regex.Replace(processedMessage, $"\\b{date.Text}\\b", $"{convertedDate} {convertedTime}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    }
                }
            }
            return processedMessage;
        }

        /// <summary>
        /// Get all available locales
        /// </summary>
        /// <returns></returns>
        public string[] GetAvailableLocales()
        {
            return _mapLocaleToFunction.Keys.ToArray();
        }
    }
}