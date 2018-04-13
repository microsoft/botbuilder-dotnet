// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                _mapLocaleToFunction[locale] = yearMonthDay;
            }
            foreach (string locale in new string[] { "en-au", "fr-be", "fr-ch", "fr-fr", "fr-lu", "fr-mc", "de-at", "de-ch", "de-de", "de-lu", "de-li" })
            {
                _mapLocaleToFunction[locale] = dayMonthYear;
            }
            _mapLocaleToFunction["en-us"] = monthDayYEar;
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
                {
                    continue;
                }
                var curDateTimeText = new TextAndDateTime
                {
                    dateTime = moment,
                    Text = result.Text
                };
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
                if (date.dateTime.Date == DateTime.Now.Date)
                {
                    processedMessage = processedMessage.Replace(date.Text, String.Format(_mapLocaleToFunction[toLocale].TimeFormat, date.dateTime));
                }
                else
                {
                    processedMessage = processedMessage.Replace(date.Text, String.Format(_mapLocaleToFunction[toLocale].DateFormat, date.dateTime));
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