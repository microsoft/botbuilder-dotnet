// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class LocalizedEntities
    {
        public string BooleanChoices { get; set; }
        public Regex NumberExpression { get; set; }
        public string NumberTerms { get; set; }
        public string NumberOrdinals { get; set; }
        public string NumberReverseOrdinals { get; set; }
    }

    public class Locales
    {
        private Dictionary<string, LocalizedEntities> _locales= new Dictionary<string, LocalizedEntities>();

        private static readonly Locales _instance = new Locales();

        private Locales()
        {
            _locales.Add("en", En());
        }

        private LocalizedEntities FindInternal(string locale, string defaultLocale)
        {
            int pos = locale.IndexOf('-');
            string parentLocale = pos > 0 ? locale.Substring(0, pos) : locale;

            if (_locales.ContainsKey(locale))
                return _locales[locale];
            else if (_locales.ContainsKey(parentLocale))
                return _locales[parentLocale];
            else if (_locales.ContainsKey(defaultLocale))
                return _locales[defaultLocale];
            else
                throw new InvalidOperationException($"Local {locale} is not supported.");
        }

        public static LocalizedEntities Find(string locale, string defaultLocale = "en")
        {
            if (string.IsNullOrWhiteSpace(locale))
                locale = defaultLocale;

            return _instance.FindInternal(locale, defaultLocale);
        }

        private static LocalizedEntities En()
        {
            LocalizedEntities entity = new LocalizedEntities
            {
                BooleanChoices = "true=y,yes,yep,sure,ok,\uD83D\uDC4D,\uD83D\uDC4C|false=n,no,nope,\uD83D\uDC4E,\u270B,\uD83D\uDD90",
                NumberExpression = new Regex(@"/[+-]?(?:\d+\.?\d*|\d*\.?\d+)/ig"),
                NumberTerms = "0=zero|1=one|2=two|3=three|4=four|5=five|6=six|7=seven|8=eight|9=nine|10=ten|11=eleven|12=twelve|13=thirteen|14=fourteen|15=fifteen|16=sixteen|17=seventeen|18=eighteen|19=nineteen|20=twenty",
                NumberOrdinals = "1=1st,first|2=2nd,second|3=3rd,third|4=4th,fourth|5=5th,fifth|6=6th,sixth|7=7th,seventh|8=8th,eighth|9=9th,ninth|10=10th,tenth",
                NumberReverseOrdinals = "-1=last|-2=next to last,second to last,second from last|-3=third to last,third from last|-4=fourth to last,fourth from last|-5=fifth to last,fifth from last"
            };

            return entity;
        }
    }
}
