// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Language policy with fallback for each language as most specific to default en-us -> en -> default.
    /// </summary>
    public class LanguagePolicy : Dictionary<string, string[]>
    {
        // Keep this method for JSON deserialization 
        public LanguagePolicy()
            : base(DefaultPolicy(), StringComparer.OrdinalIgnoreCase) 
        { 
        }

        public LanguagePolicy(params string[] defaultLanguage)
            : base(DefaultPolicy(defaultLanguage), StringComparer.OrdinalIgnoreCase)
        {
        }

        // walk through all of the cultures and create a dictionary map with most specific to least specific
        // Example output "en-us" will generate fallback rule like this:
        //   "en-us" -> "en" -> "" 
        //   "en" -> ""
        // So that when we get a locale such as en-gb, we can try to resolve to "en-gb" then "en" then ""
        // See commented section for full sample of output of this function
        private static IDictionary<string, string[]> DefaultPolicy(string[] defaultLanguages = null)
        {
            if (defaultLanguages == null)
            {
                defaultLanguages = new string[] { string.Empty };
            }

            var cultureCodes = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.IetfLanguageTag.ToLower()).ToList();
            var policy = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var language in cultureCodes.Distinct())
            {
                var lang = language.ToLower();
                var fallback = new List<string>();
                while (!string.IsNullOrEmpty(lang))
                {
                    fallback.Add(lang);

                    var i = lang.LastIndexOf("-");
                    if (i > 0)
                    {
                        lang = lang.Substring(0, i);
                    }
                    else
                    {
                        break;
                    }
                }

                if (language == string.Empty)
                {
                    // here we set the default
                    fallback.AddRange(defaultLanguages);
                }

                policy.Add(language, fallback.ToArray());
            }

            return policy;
        }
    }
}

/* Example output:
 {
  "": [
    ""
  ],
  "aa": [
    "aa",
    ""
  ],
  "aa-dj": [
    "aa-dj",
    "aa",
    ""
  ],
  "aa-er": [
    "aa-er",
    "aa",
    ""
  ],
  "aa-et": [
    "aa-et",
    "aa",
    ""
  ],
  "af": [
    "af",
    ""
  ],
  "af-na": [
    "af-na",
    "af",
    ""
  ],
  "af-za": [
    "af-za",
    "af",
    ""
  ],
  "agq": [
    "agq",
    ""
  ],
  "agq-cm": [
    "agq-cm",
    "agq",
    ""
  ],
  "ak": [
    "ak",
    ""
  ],
  "ak-gh": [
    "ak-gh",
    "ak",
    ""
  ],
  "am": [
    "am",
    ""
  ],
  "am-et": [
    "am-et",
    "am",
    ""
  ],
  "ar": [
    "ar",
    ""
  ],
  "ar-001": [
    "ar-001",
    "ar",
    ""
  ],
  "ar-ae": [
    "ar-ae",
    "ar",
    ""
  ],
  "ar-bh": [
    "ar-bh",
    "ar",
    ""
  ],
  "ar-dj": [
    "ar-dj",
    "ar",
    ""
  ],
  "ar-dz": [
    "ar-dz",
    "ar",
    ""
  ],
  "ar-eg": [
    "ar-eg",
    "ar",
    ""
  ],
  "ar-er": [
    "ar-er",
    "ar",
    ""
  ],
  "ar-il": [
    "ar-il",
    "ar",
    ""
  ],
  "ar-iq": [
    "ar-iq",
    "ar",
    ""
  ],
  "ar-jo": [
    "ar-jo",
    "ar",
    ""
  ],
  "ar-km": [
    "ar-km",
    "ar",
    ""
  ],
  "ar-kw": [
    "ar-kw",
    "ar",
    ""
  ],
  "ar-lb": [
    "ar-lb",
    "ar",
    ""
  ],
  "ar-ly": [
    "ar-ly",
    "ar",
    ""
  ],
  "ar-ma": [
    "ar-ma",
    "ar",
    ""
  ],
  "ar-mr": [
    "ar-mr",
    "ar",
    ""
  ],
  "ar-om": [
    "ar-om",
    "ar",
    ""
  ],
  "ar-ps": [
    "ar-ps",
    "ar",
    ""
  ],
  "ar-qa": [
    "ar-qa",
    "ar",
    ""
  ],
  "ar-sa": [
    "ar-sa",
    "ar",
    ""
  ],
  "ar-sd": [
    "ar-sd",
    "ar",
    ""
  ],
  "ar-so": [
    "ar-so",
    "ar",
    ""
  ],
  "ar-ss": [
    "ar-ss",
    "ar",
    ""
  ],
  "ar-sy": [
    "ar-sy",
    "ar",
    ""
  ],
  "ar-td": [
    "ar-td",
    "ar",
    ""
  ],
  "ar-tn": [
    "ar-tn",
    "ar",
    ""
  ],
  "ar-ye": [
    "ar-ye",
    "ar",
    ""
  ],
  "arn": [
    "arn",
    ""
  ],
  "arn-cl": [
    "arn-cl",
    "arn",
    ""
  ],
  "as": [
    "as",
    ""
  ],
  "as-in": [
    "as-in",
    "as",
    ""
  ],
  "asa": [
    "asa",
    ""
  ],
  "asa-tz": [
    "asa-tz",
    "asa",
    ""
  ],
  "ast": [
    "ast",
    ""
  ],
  "ast-es": [
    "ast-es",
    "ast",
    ""
  ],
  "az": [
    "az",
    ""
  ],
  "az-cyrl": [
    "az-cyrl",
    "az",
    ""
  ],
  "az-cyrl-az": [
    "az-cyrl-az",
    "az-cyrl",
    "az",
    ""
  ],
  "az-latn": [
    "az-latn",
    "az",
    ""
  ],
  "az-latn-az": [
    "az-latn-az",
    "az-latn",
    "az",
    ""
  ],
  "ba": [
    "ba",
    ""
  ],
  "ba-ru": [
    "ba-ru",
    "ba",
    ""
  ],
  "bas": [
    "bas",
    ""
  ],
  "bas-cm": [
    "bas-cm",
    "bas",
    ""
  ],
  "be": [
    "be",
    ""
  ],
  "be-by": [
    "be-by",
    "be",
    ""
  ],
  "bem": [
    "bem",
    ""
  ],
  "bem-zm": [
    "bem-zm",
    "bem",
    ""
  ],
  "bez": [
    "bez",
    ""
  ],
  "bez-tz": [
    "bez-tz",
    "bez",
    ""
  ],
  "bg": [
    "bg",
    ""
  ],
  "bg-bg": [
    "bg-bg",
    "bg",
    ""
  ],
  "bin": [
    "bin",
    ""
  ],
  "bin-ng": [
    "bin-ng",
    "bin",
    ""
  ],
  "bm": [
    "bm",
    ""
  ],
  "bm-latn": [
    "bm-latn",
    "bm",
    ""
  ],
  "bm-latn-ml": [
    "bm-latn-ml",
    "bm-latn",
    "bm",
    ""
  ],
  "bn": [
    "bn",
    ""
  ],
  "bn-bd": [
    "bn-bd",
    "bn",
    ""
  ],
  "bn-in": [
    "bn-in",
    "bn",
    ""
  ],
  "bo": [
    "bo",
    ""
  ],
  "bo-cn": [
    "bo-cn",
    "bo",
    ""
  ],
  "bo-in": [
    "bo-in",
    "bo",
    ""
  ],
  "br": [
    "br",
    ""
  ],
  "br-fr": [
    "br-fr",
    "br",
    ""
  ],
  "brx": [
    "brx",
    ""
  ],
  "brx-in": [
    "brx-in",
    "brx",
    ""
  ],
  "bs": [
    "bs",
    ""
  ],
  "bs-cyrl": [
    "bs-cyrl",
    "bs",
    ""
  ],
  "bs-cyrl-ba": [
    "bs-cyrl-ba",
    "bs-cyrl",
    "bs",
    ""
  ],
  "bs-latn": [
    "bs-latn",
    "bs",
    ""
  ],
  "bs-latn-ba": [
    "bs-latn-ba",
    "bs-latn",
    "bs",
    ""
  ],
  "byn": [
    "byn",
    ""
  ],
  "byn-er": [
    "byn-er",
    "byn",
    ""
  ],
  "ca": [
    "ca",
    ""
  ],
  "ca-ad": [
    "ca-ad",
    "ca",
    ""
  ],
  "ca-es": [
    "ca-es",
    "ca",
    ""
  ],
  "ca-es-valencia": [
    "ca-es-valencia",
    "ca-es",
    "ca",
    ""
  ],
  "ca-fr": [
    "ca-fr",
    "ca",
    ""
  ],
  "ca-it": [
    "ca-it",
    "ca",
    ""
  ],
  "ce": [
    "ce",
    ""
  ],
  "ce-ru": [
    "ce-ru",
    "ce",
    ""
  ],
  "cgg": [
    "cgg",
    ""
  ],
  "cgg-ug": [
    "cgg-ug",
    "cgg",
    ""
  ],
  "chr": [
    "chr",
    ""
  ],
  "chr-cher": [
    "chr-cher",
    "chr",
    ""
  ],
  "chr-cher-us": [
    "chr-cher-us",
    "chr-cher",
    "chr",
    ""
  ],
  "co": [
    "co",
    ""
  ],
  "co-fr": [
    "co-fr",
    "co",
    ""
  ],
  "cs": [
    "cs",
    ""
  ],
  "cs-cz": [
    "cs-cz",
    "cs",
    ""
  ],
  "cu": [
    "cu",
    ""
  ],
  "cu-ru": [
    "cu-ru",
    "cu",
    ""
  ],
  "cy": [
    "cy",
    ""
  ],
  "cy-gb": [
    "cy-gb",
    "cy",
    ""
  ],
  "da": [
    "da",
    ""
  ],
  "da-dk": [
    "da-dk",
    "da",
    ""
  ],
  "da-gl": [
    "da-gl",
    "da",
    ""
  ],
  "dav": [
    "dav",
    ""
  ],
  "dav-ke": [
    "dav-ke",
    "dav",
    ""
  ],
  "de": [
    "de",
    ""
  ],
  "de-at": [
    "de-at",
    "de",
    ""
  ],
  "de-be": [
    "de-be",
    "de",
    ""
  ],
  "de-ch": [
    "de-ch",
    "de",
    ""
  ],
  "de-de": [
    "de-de",
    "de",
    ""
  ],
  "de-it": [
    "de-it",
    "de",
    ""
  ],
  "de-li": [
    "de-li",
    "de",
    ""
  ],
  "de-lu": [
    "de-lu",
    "de",
    ""
  ],
  "dje": [
    "dje",
    ""
  ],
  "dje-ne": [
    "dje-ne",
    "dje",
    ""
  ],
  "dsb": [
    "dsb",
    ""
  ],
  "dsb-de": [
    "dsb-de",
    "dsb",
    ""
  ],
  "dua": [
    "dua",
    ""
  ],
  "dua-cm": [
    "dua-cm",
    "dua",
    ""
  ],
  "dv": [
    "dv",
    ""
  ],
  "dv-mv": [
    "dv-mv",
    "dv",
    ""
  ],
  "dyo": [
    "dyo",
    ""
  ],
  "dyo-sn": [
    "dyo-sn",
    "dyo",
    ""
  ],
  "dz": [
    "dz",
    ""
  ],
  "dz-bt": [
    "dz-bt",
    "dz",
    ""
  ],
  "ebu": [
    "ebu",
    ""
  ],
  "ebu-ke": [
    "ebu-ke",
    "ebu",
    ""
  ],
  "ee": [
    "ee",
    ""
  ],
  "ee-gh": [
    "ee-gh",
    "ee",
    ""
  ],
  "ee-tg": [
    "ee-tg",
    "ee",
    ""
  ],
  "el": [
    "el",
    ""
  ],
  "el-cy": [
    "el-cy",
    "el",
    ""
  ],
  "el-gr": [
    "el-gr",
    "el",
    ""
  ],
  "en": [
    "en",
    ""
  ],
  "en-001": [
    "en-001",
    "en",
    ""
  ],
  "en-029": [
    "en-029",
    "en",
    ""
  ],
  "en-150": [
    "en-150",
    "en",
    ""
  ],
  "en-ag": [
    "en-ag",
    "en",
    ""
  ],
  "en-ai": [
    "en-ai",
    "en",
    ""
  ],
  "en-as": [
    "en-as",
    "en",
    ""
  ],
  "en-at": [
    "en-at",
    "en",
    ""
  ],
  "en-au": [
    "en-au",
    "en",
    ""
  ],
  "en-bb": [
    "en-bb",
    "en",
    ""
  ],
  "en-be": [
    "en-be",
    "en",
    ""
  ],
  "en-bi": [
    "en-bi",
    "en",
    ""
  ],
  "en-bm": [
    "en-bm",
    "en",
    ""
  ],
  "en-bs": [
    "en-bs",
    "en",
    ""
  ],
  "en-bw": [
    "en-bw",
    "en",
    ""
  ],
  "en-bz": [
    "en-bz",
    "en",
    ""
  ],
  "en-ca": [
    "en-ca",
    "en",
    ""
  ],
  "en-cc": [
    "en-cc",
    "en",
    ""
  ],
  "en-ch": [
    "en-ch",
    "en",
    ""
  ],
  "en-ck": [
    "en-ck",
    "en",
    ""
  ],
  "en-cm": [
    "en-cm",
    "en",
    ""
  ],
  "en-cx": [
    "en-cx",
    "en",
    ""
  ],
  "en-cy": [
    "en-cy",
    "en",
    ""
  ],
  "en-de": [
    "en-de",
    "en",
    ""
  ],
  "en-dk": [
    "en-dk",
    "en",
    ""
  ],
  "en-dm": [
    "en-dm",
    "en",
    ""
  ],
  "en-er": [
    "en-er",
    "en",
    ""
  ],
  "en-fi": [
    "en-fi",
    "en",
    ""
  ],
  "en-fj": [
    "en-fj",
    "en",
    ""
  ],
  "en-fk": [
    "en-fk",
    "en",
    ""
  ],
  "en-fm": [
    "en-fm",
    "en",
    ""
  ],
  "en-gb": [
    "en-gb",
    "en",
    ""
  ],
  "en-gd": [
    "en-gd",
    "en",
    ""
  ],
  "en-gg": [
    "en-gg",
    "en",
    ""
  ],
  "en-gh": [
    "en-gh",
    "en",
    ""
  ],
  "en-gi": [
    "en-gi",
    "en",
    ""
  ],
  "en-gm": [
    "en-gm",
    "en",
    ""
  ],
  "en-gu": [
    "en-gu",
    "en",
    ""
  ],
  "en-gy": [
    "en-gy",
    "en",
    ""
  ],
  "en-hk": [
    "en-hk",
    "en",
    ""
  ],
  "en-id": [
    "en-id",
    "en",
    ""
  ],
  "en-ie": [
    "en-ie",
    "en",
    ""
  ],
  "en-il": [
    "en-il",
    "en",
    ""
  ],
  "en-im": [
    "en-im",
    "en",
    ""
  ],
  "en-in": [
    "en-in",
    "en",
    ""
  ],
  "en-io": [
    "en-io",
    "en",
    ""
  ],
  "en-je": [
    "en-je",
    "en",
    ""
  ],
  "en-jm": [
    "en-jm",
    "en",
    ""
  ],
  "en-ke": [
    "en-ke",
    "en",
    ""
  ],
  "en-ki": [
    "en-ki",
    "en",
    ""
  ],
  "en-kn": [
    "en-kn",
    "en",
    ""
  ],
  "en-ky": [
    "en-ky",
    "en",
    ""
  ],
  "en-lc": [
    "en-lc",
    "en",
    ""
  ],
  "en-lr": [
    "en-lr",
    "en",
    ""
  ],
  "en-ls": [
    "en-ls",
    "en",
    ""
  ],
  "en-mg": [
    "en-mg",
    "en",
    ""
  ],
  "en-mh": [
    "en-mh",
    "en",
    ""
  ],
  "en-mo": [
    "en-mo",
    "en",
    ""
  ],
  "en-mp": [
    "en-mp",
    "en",
    ""
  ],
  "en-ms": [
    "en-ms",
    "en",
    ""
  ],
  "en-mt": [
    "en-mt",
    "en",
    ""
  ],
  "en-mu": [
    "en-mu",
    "en",
    ""
  ],
  "en-mw": [
    "en-mw",
    "en",
    ""
  ],
  "en-my": [
    "en-my",
    "en",
    ""
  ],
  "en-na": [
    "en-na",
    "en",
    ""
  ],
  "en-nf": [
    "en-nf",
    "en",
    ""
  ],
  "en-ng": [
    "en-ng",
    "en",
    ""
  ],
  "en-nl": [
    "en-nl",
    "en",
    ""
  ],
  "en-nr": [
    "en-nr",
    "en",
    ""
  ],
  "en-nu": [
    "en-nu",
    "en",
    ""
  ],
  "en-nz": [
    "en-nz",
    "en",
    ""
  ],
  "en-pg": [
    "en-pg",
    "en",
    ""
  ],
  "en-ph": [
    "en-ph",
    "en",
    ""
  ],
  "en-pk": [
    "en-pk",
    "en",
    ""
  ],
  "en-pn": [
    "en-pn",
    "en",
    ""
  ],
  "en-pr": [
    "en-pr",
    "en",
    ""
  ],
  "en-pw": [
    "en-pw",
    "en",
    ""
  ],
  "en-rw": [
    "en-rw",
    "en",
    ""
  ],
  "en-sb": [
    "en-sb",
    "en",
    ""
  ],
  "en-sc": [
    "en-sc",
    "en",
    ""
  ],
  "en-sd": [
    "en-sd",
    "en",
    ""
  ],
  "en-se": [
    "en-se",
    "en",
    ""
  ],
  "en-sg": [
    "en-sg",
    "en",
    ""
  ],
  "en-sh": [
    "en-sh",
    "en",
    ""
  ],
  "en-si": [
    "en-si",
    "en",
    ""
  ],
  "en-sl": [
    "en-sl",
    "en",
    ""
  ],
  "en-ss": [
    "en-ss",
    "en",
    ""
  ],
  "en-sx": [
    "en-sx",
    "en",
    ""
  ],
  "en-sz": [
    "en-sz",
    "en",
    ""
  ],
  "en-tc": [
    "en-tc",
    "en",
    ""
  ],
  "en-tk": [
    "en-tk",
    "en",
    ""
  ],
  "en-to": [
    "en-to",
    "en",
    ""
  ],
  "en-tt": [
    "en-tt",
    "en",
    ""
  ],
  "en-tv": [
    "en-tv",
    "en",
    ""
  ],
  "en-tz": [
    "en-tz",
    "en",
    ""
  ],
  "en-ug": [
    "en-ug",
    "en",
    ""
  ],
  "en-um": [
    "en-um",
    "en",
    ""
  ],
  "en-us": [
    "en-us",
    "en",
    ""
  ],
  "en-vc": [
    "en-vc",
    "en",
    ""
  ],
  "en-vg": [
    "en-vg",
    "en",
    ""
  ],
  "en-vi": [
    "en-vi",
    "en",
    ""
  ],
  "en-vu": [
    "en-vu",
    "en",
    ""
  ],
  "en-ws": [
    "en-ws",
    "en",
    ""
  ],
  "en-za": [
    "en-za",
    "en",
    ""
  ],
  "en-zm": [
    "en-zm",
    "en",
    ""
  ],
  "en-zw": [
    "en-zw",
    "en",
    ""
  ],
  "eo": [
    "eo",
    ""
  ],
  "eo-001": [
    "eo-001",
    "eo",
    ""
  ],
  "es": [
    "es",
    ""
  ],
  "es-419": [
    "es-419",
    "es",
    ""
  ],
  "es-ar": [
    "es-ar",
    "es",
    ""
  ],
  "es-bo": [
    "es-bo",
    "es",
    ""
  ],
  "es-br": [
    "es-br",
    "es",
    ""
  ],
  "es-bz": [
    "es-bz",
    "es",
    ""
  ],
  "es-cl": [
    "es-cl",
    "es",
    ""
  ],
  "es-co": [
    "es-co",
    "es",
    ""
  ],
  "es-cr": [
    "es-cr",
    "es",
    ""
  ],
  "es-cu": [
    "es-cu",
    "es",
    ""
  ],
  "es-do": [
    "es-do",
    "es",
    ""
  ],
  "es-ec": [
    "es-ec",
    "es",
    ""
  ],
  "es-es": [
    "es-es",
    "es",
    ""
  ],
  "es-gq": [
    "es-gq",
    "es",
    ""
  ],
  "es-gt": [
    "es-gt",
    "es",
    ""
  ],
  "es-hn": [
    "es-hn",
    "es",
    ""
  ],
  "es-mx": [
    "es-mx",
    "es",
    ""
  ],
  "es-ni": [
    "es-ni",
    "es",
    ""
  ],
  "es-pa": [
    "es-pa",
    "es",
    ""
  ],
  "es-pe": [
    "es-pe",
    "es",
    ""
  ],
  "es-ph": [
    "es-ph",
    "es",
    ""
  ],
  "es-pr": [
    "es-pr",
    "es",
    ""
  ],
  "es-py": [
    "es-py",
    "es",
    ""
  ],
  "es-sv": [
    "es-sv",
    "es",
    ""
  ],
  "es-us": [
    "es-us",
    "es",
    ""
  ],
  "es-uy": [
    "es-uy",
    "es",
    ""
  ],
  "es-ve": [
    "es-ve",
    "es",
    ""
  ],
  "et": [
    "et",
    ""
  ],
  "et-ee": [
    "et-ee",
    "et",
    ""
  ],
  "eu": [
    "eu",
    ""
  ],
  "eu-es": [
    "eu-es",
    "eu",
    ""
  ],
  "ewo": [
    "ewo",
    ""
  ],
  "ewo-cm": [
    "ewo-cm",
    "ewo",
    ""
  ],
  "fa": [
    "fa",
    ""
  ],
  "fa-ir": [
    "fa-ir",
    "fa",
    ""
  ],
  "ff": [
    "ff",
    ""
  ],
  "ff-cm": [
    "ff-cm",
    "ff",
    ""
  ],
  "ff-gn": [
    "ff-gn",
    "ff",
    ""
  ],
  "ff-latn": [
    "ff-latn",
    "ff",
    ""
  ],
  "ff-latn-sn": [
    "ff-latn-sn",
    "ff-latn",
    "ff",
    ""
  ],
  "ff-mr": [
    "ff-mr",
    "ff",
    ""
  ],
  "ff-ng": [
    "ff-ng",
    "ff",
    ""
  ],
  "fi": [
    "fi",
    ""
  ],
  "fi-fi": [
    "fi-fi",
    "fi",
    ""
  ],
  "fil": [
    "fil",
    ""
  ],
  "fil-ph": [
    "fil-ph",
    "fil",
    ""
  ],
  "fo": [
    "fo",
    ""
  ],
  "fo-dk": [
    "fo-dk",
    "fo",
    ""
  ],
  "fo-fo": [
    "fo-fo",
    "fo",
    ""
  ],
  "fr": [
    "fr",
    ""
  ],
  "fr-029": [
    "fr-029",
    "fr",
    ""
  ],
  "fr-be": [
    "fr-be",
    "fr",
    ""
  ],
  "fr-bf": [
    "fr-bf",
    "fr",
    ""
  ],
  "fr-bi": [
    "fr-bi",
    "fr",
    ""
  ],
  "fr-bj": [
    "fr-bj",
    "fr",
    ""
  ],
  "fr-bl": [
    "fr-bl",
    "fr",
    ""
  ],
  "fr-ca": [
    "fr-ca",
    "fr",
    ""
  ],
  "fr-cd": [
    "fr-cd",
    "fr",
    ""
  ],
  "fr-cf": [
    "fr-cf",
    "fr",
    ""
  ],
  "fr-cg": [
    "fr-cg",
    "fr",
    ""
  ],
  "fr-ch": [
    "fr-ch",
    "fr",
    ""
  ],
  "fr-ci": [
    "fr-ci",
    "fr",
    ""
  ],
  "fr-cm": [
    "fr-cm",
    "fr",
    ""
  ],
  "fr-dj": [
    "fr-dj",
    "fr",
    ""
  ],
  "fr-dz": [
    "fr-dz",
    "fr",
    ""
  ],
  "fr-fr": [
    "fr-fr",
    "fr",
    ""
  ],
  "fr-ga": [
    "fr-ga",
    "fr",
    ""
  ],
  "fr-gf": [
    "fr-gf",
    "fr",
    ""
  ],
  "fr-gn": [
    "fr-gn",
    "fr",
    ""
  ],
  "fr-gp": [
    "fr-gp",
    "fr",
    ""
  ],
  "fr-gq": [
    "fr-gq",
    "fr",
    ""
  ],
  "fr-ht": [
    "fr-ht",
    "fr",
    ""
  ],
  "fr-km": [
    "fr-km",
    "fr",
    ""
  ],
  "fr-lu": [
    "fr-lu",
    "fr",
    ""
  ],
  "fr-ma": [
    "fr-ma",
    "fr",
    ""
  ],
  "fr-mc": [
    "fr-mc",
    "fr",
    ""
  ],
  "fr-mf": [
    "fr-mf",
    "fr",
    ""
  ],
  "fr-mg": [
    "fr-mg",
    "fr",
    ""
  ],
  "fr-ml": [
    "fr-ml",
    "fr",
    ""
  ],
  "fr-mq": [
    "fr-mq",
    "fr",
    ""
  ],
  "fr-mr": [
    "fr-mr",
    "fr",
    ""
  ],
  "fr-mu": [
    "fr-mu",
    "fr",
    ""
  ],
  "fr-nc": [
    "fr-nc",
    "fr",
    ""
  ],
  "fr-ne": [
    "fr-ne",
    "fr",
    ""
  ],
  "fr-pf": [
    "fr-pf",
    "fr",
    ""
  ],
  "fr-pm": [
    "fr-pm",
    "fr",
    ""
  ],
  "fr-re": [
    "fr-re",
    "fr",
    ""
  ],
  "fr-rw": [
    "fr-rw",
    "fr",
    ""
  ],
  "fr-sc": [
    "fr-sc",
    "fr",
    ""
  ],
  "fr-sn": [
    "fr-sn",
    "fr",
    ""
  ],
  "fr-sy": [
    "fr-sy",
    "fr",
    ""
  ],
  "fr-td": [
    "fr-td",
    "fr",
    ""
  ],
  "fr-tg": [
    "fr-tg",
    "fr",
    ""
  ],
  "fr-tn": [
    "fr-tn",
    "fr",
    ""
  ],
  "fr-vu": [
    "fr-vu",
    "fr",
    ""
  ],
  "fr-wf": [
    "fr-wf",
    "fr",
    ""
  ],
  "fr-yt": [
    "fr-yt",
    "fr",
    ""
  ],
  "fur": [
    "fur",
    ""
  ],
  "fur-it": [
    "fur-it",
    "fur",
    ""
  ],
  "fy": [
    "fy",
    ""
  ],
  "fy-nl": [
    "fy-nl",
    "fy",
    ""
  ],
  "ga": [
    "ga",
    ""
  ],
  "ga-ie": [
    "ga-ie",
    "ga",
    ""
  ],
  "gd": [
    "gd",
    ""
  ],
  "gd-gb": [
    "gd-gb",
    "gd",
    ""
  ],
  "gl": [
    "gl",
    ""
  ],
  "gl-es": [
    "gl-es",
    "gl",
    ""
  ],
  "gn": [
    "gn",
    ""
  ],
  "gn-py": [
    "gn-py",
    "gn",
    ""
  ],
  "gsw": [
    "gsw",
    ""
  ],
  "gsw-ch": [
    "gsw-ch",
    "gsw",
    ""
  ],
  "gsw-fr": [
    "gsw-fr",
    "gsw",
    ""
  ],
  "gsw-li": [
    "gsw-li",
    "gsw",
    ""
  ],
  "gu": [
    "gu",
    ""
  ],
  "gu-in": [
    "gu-in",
    "gu",
    ""
  ],
  "guz": [
    "guz",
    ""
  ],
  "guz-ke": [
    "guz-ke",
    "guz",
    ""
  ],
  "gv": [
    "gv",
    ""
  ],
  "gv-im": [
    "gv-im",
    "gv",
    ""
  ],
  "ha": [
    "ha",
    ""
  ],
  "ha-latn": [
    "ha-latn",
    "ha",
    ""
  ],
  "ha-latn-gh": [
    "ha-latn-gh",
    "ha-latn",
    "ha",
    ""
  ],
  "ha-latn-ne": [
    "ha-latn-ne",
    "ha-latn",
    "ha",
    ""
  ],
  "ha-latn-ng": [
    "ha-latn-ng",
    "ha-latn",
    "ha",
    ""
  ],
  "haw": [
    "haw",
    ""
  ],
  "haw-us": [
    "haw-us",
    "haw",
    ""
  ],
  "he": [
    "he",
    ""
  ],
  "he-il": [
    "he-il",
    "he",
    ""
  ],
  "hi": [
    "hi",
    ""
  ],
  "hi-in": [
    "hi-in",
    "hi",
    ""
  ],
  "hr": [
    "hr",
    ""
  ],
  "hr-ba": [
    "hr-ba",
    "hr",
    ""
  ],
  "hr-hr": [
    "hr-hr",
    "hr",
    ""
  ],
  "hsb": [
    "hsb",
    ""
  ],
  "hsb-de": [
    "hsb-de",
    "hsb",
    ""
  ],
  "hu": [
    "hu",
    ""
  ],
  "hu-hu": [
    "hu-hu",
    "hu",
    ""
  ],
  "hy": [
    "hy",
    ""
  ],
  "hy-am": [
    "hy-am",
    "hy",
    ""
  ],
  "ia": [
    "ia",
    ""
  ],
  "ia-001": [
    "ia-001",
    "ia",
    ""
  ],
  "ia-fr": [
    "ia-fr",
    "ia",
    ""
  ],
  "ibb": [
    "ibb",
    ""
  ],
  "ibb-ng": [
    "ibb-ng",
    "ibb",
    ""
  ],
  "id": [
    "id",
    ""
  ],
  "id-id": [
    "id-id",
    "id",
    ""
  ],
  "ig": [
    "ig",
    ""
  ],
  "ig-ng": [
    "ig-ng",
    "ig",
    ""
  ],
  "ii": [
    "ii",
    ""
  ],
  "ii-cn": [
    "ii-cn",
    "ii",
    ""
  ],
  "is": [
    "is",
    ""
  ],
  "is-is": [
    "is-is",
    "is",
    ""
  ],
  "it": [
    "it",
    ""
  ],
  "it-ch": [
    "it-ch",
    "it",
    ""
  ],
  "it-it": [
    "it-it",
    "it",
    ""
  ],
  "it-sm": [
    "it-sm",
    "it",
    ""
  ],
  "it-va": [
    "it-va",
    "it",
    ""
  ],
  "iu": [
    "iu",
    ""
  ],
  "iu-cans": [
    "iu-cans",
    "iu",
    ""
  ],
  "iu-cans-ca": [
    "iu-cans-ca",
    "iu-cans",
    "iu",
    ""
  ],
  "iu-latn": [
    "iu-latn",
    "iu",
    ""
  ],
  "iu-latn-ca": [
    "iu-latn-ca",
    "iu-latn",
    "iu",
    ""
  ],
  "ja": [
    "ja",
    ""
  ],
  "ja-jp": [
    "ja-jp",
    "ja",
    ""
  ],
  "jgo": [
    "jgo",
    ""
  ],
  "jgo-cm": [
    "jgo-cm",
    "jgo",
    ""
  ],
  "jmc": [
    "jmc",
    ""
  ],
  "jmc-tz": [
    "jmc-tz",
    "jmc",
    ""
  ],
  "jv": [
    "jv",
    ""
  ],
  "jv-java": [
    "jv-java",
    "jv",
    ""
  ],
  "jv-java-id": [
    "jv-java-id",
    "jv-java",
    "jv",
    ""
  ],
  "jv-latn": [
    "jv-latn",
    "jv",
    ""
  ],
  "jv-latn-id": [
    "jv-latn-id",
    "jv-latn",
    "jv",
    ""
  ],
  "ka": [
    "ka",
    ""
  ],
  "ka-ge": [
    "ka-ge",
    "ka",
    ""
  ],
  "kab": [
    "kab",
    ""
  ],
  "kab-dz": [
    "kab-dz",
    "kab",
    ""
  ],
  "kam": [
    "kam",
    ""
  ],
  "kam-ke": [
    "kam-ke",
    "kam",
    ""
  ],
  "kde": [
    "kde",
    ""
  ],
  "kde-tz": [
    "kde-tz",
    "kde",
    ""
  ],
  "kea": [
    "kea",
    ""
  ],
  "kea-cv": [
    "kea-cv",
    "kea",
    ""
  ],
  "khq": [
    "khq",
    ""
  ],
  "khq-ml": [
    "khq-ml",
    "khq",
    ""
  ],
  "ki": [
    "ki",
    ""
  ],
  "ki-ke": [
    "ki-ke",
    "ki",
    ""
  ],
  "kk": [
    "kk",
    ""
  ],
  "kk-kz": [
    "kk-kz",
    "kk",
    ""
  ],
  "kkj": [
    "kkj",
    ""
  ],
  "kkj-cm": [
    "kkj-cm",
    "kkj",
    ""
  ],
  "kl": [
    "kl",
    ""
  ],
  "kl-gl": [
    "kl-gl",
    "kl",
    ""
  ],
  "kln": [
    "kln",
    ""
  ],
  "kln-ke": [
    "kln-ke",
    "kln",
    ""
  ],
  "km": [
    "km",
    ""
  ],
  "km-kh": [
    "km-kh",
    "km",
    ""
  ],
  "kn": [
    "kn",
    ""
  ],
  "kn-in": [
    "kn-in",
    "kn",
    ""
  ],
  "ko": [
    "ko",
    ""
  ],
  "ko-kp": [
    "ko-kp",
    "ko",
    ""
  ],
  "ko-kr": [
    "ko-kr",
    "ko",
    ""
  ],
  "kok": [
    "kok",
    ""
  ],
  "kok-in": [
    "kok-in",
    "kok",
    ""
  ],
  "kr": [
    "kr",
    ""
  ],
  "kr-ng": [
    "kr-ng",
    "kr",
    ""
  ],
  "ks": [
    "ks",
    ""
  ],
  "ks-arab": [
    "ks-arab",
    "ks",
    ""
  ],
  "ks-arab-in": [
    "ks-arab-in",
    "ks-arab",
    "ks",
    ""
  ],
  "ks-deva": [
    "ks-deva",
    "ks",
    ""
  ],
  "ks-deva-in": [
    "ks-deva-in",
    "ks-deva",
    "ks",
    ""
  ],
  "ksb": [
    "ksb",
    ""
  ],
  "ksb-tz": [
    "ksb-tz",
    "ksb",
    ""
  ],
  "ksf": [
    "ksf",
    ""
  ],
  "ksf-cm": [
    "ksf-cm",
    "ksf",
    ""
  ],
  "ksh": [
    "ksh",
    ""
  ],
  "ksh-de": [
    "ksh-de",
    "ksh",
    ""
  ],
  "ku": [
    "ku",
    ""
  ],
  "ku-arab": [
    "ku-arab",
    "ku",
    ""
  ],
  "ku-arab-iq": [
    "ku-arab-iq",
    "ku-arab",
    "ku",
    ""
  ],
  "ku-arab-ir": [
    "ku-arab-ir",
    "ku-arab",
    "ku",
    ""
  ],
  "kw": [
    "kw",
    ""
  ],
  "kw-gb": [
    "kw-gb",
    "kw",
    ""
  ],
  "ky": [
    "ky",
    ""
  ],
  "ky-kg": [
    "ky-kg",
    "ky",
    ""
  ],
  "la": [
    "la",
    ""
  ],
  "la-001": [
    "la-001",
    "la",
    ""
  ],
  "lag": [
    "lag",
    ""
  ],
  "lag-tz": [
    "lag-tz",
    "lag",
    ""
  ],
  "lb": [
    "lb",
    ""
  ],
  "lb-lu": [
    "lb-lu",
    "lb",
    ""
  ],
  "lg": [
    "lg",
    ""
  ],
  "lg-ug": [
    "lg-ug",
    "lg",
    ""
  ],
  "lkt": [
    "lkt",
    ""
  ],
  "lkt-us": [
    "lkt-us",
    "lkt",
    ""
  ],
  "ln": [
    "ln",
    ""
  ],
  "ln-ao": [
    "ln-ao",
    "ln",
    ""
  ],
  "ln-cd": [
    "ln-cd",
    "ln",
    ""
  ],
  "ln-cf": [
    "ln-cf",
    "ln",
    ""
  ],
  "ln-cg": [
    "ln-cg",
    "ln",
    ""
  ],
  "lo": [
    "lo",
    ""
  ],
  "lo-la": [
    "lo-la",
    "lo",
    ""
  ],
  "lrc": [
    "lrc",
    ""
  ],
  "lrc-iq": [
    "lrc-iq",
    "lrc",
    ""
  ],
  "lrc-ir": [
    "lrc-ir",
    "lrc",
    ""
  ],
  "lt": [
    "lt",
    ""
  ],
  "lt-lt": [
    "lt-lt",
    "lt",
    ""
  ],
  "lu": [
    "lu",
    ""
  ],
  "lu-cd": [
    "lu-cd",
    "lu",
    ""
  ],
  "luo": [
    "luo",
    ""
  ],
  "luo-ke": [
    "luo-ke",
    "luo",
    ""
  ],
  "luy": [
    "luy",
    ""
  ],
  "luy-ke": [
    "luy-ke",
    "luy",
    ""
  ],
  "lv": [
    "lv",
    ""
  ],
  "lv-lv": [
    "lv-lv",
    "lv",
    ""
  ],
  "mas": [
    "mas",
    ""
  ],
  "mas-ke": [
    "mas-ke",
    "mas",
    ""
  ],
  "mas-tz": [
    "mas-tz",
    "mas",
    ""
  ],
  "mer": [
    "mer",
    ""
  ],
  "mer-ke": [
    "mer-ke",
    "mer",
    ""
  ],
  "mfe": [
    "mfe",
    ""
  ],
  "mfe-mu": [
    "mfe-mu",
    "mfe",
    ""
  ],
  "mg": [
    "mg",
    ""
  ],
  "mg-mg": [
    "mg-mg",
    "mg",
    ""
  ],
  "mgh": [
    "mgh",
    ""
  ],
  "mgh-mz": [
    "mgh-mz",
    "mgh",
    ""
  ],
  "mgo": [
    "mgo",
    ""
  ],
  "mgo-cm": [
    "mgo-cm",
    "mgo",
    ""
  ],
  "mi": [
    "mi",
    ""
  ],
  "mi-nz": [
    "mi-nz",
    "mi",
    ""
  ],
  "mk": [
    "mk",
    ""
  ],
  "mk-mk": [
    "mk-mk",
    "mk",
    ""
  ],
  "ml": [
    "ml",
    ""
  ],
  "ml-in": [
    "ml-in",
    "ml",
    ""
  ],
  "mn": [
    "mn",
    ""
  ],
  "mn-cyrl": [
    "mn-cyrl",
    "mn",
    ""
  ],
  "mn-mn": [
    "mn-mn",
    "mn",
    ""
  ],
  "mn-mong": [
    "mn-mong",
    "mn",
    ""
  ],
  "mn-mong-cn": [
    "mn-mong-cn",
    "mn-mong",
    "mn",
    ""
  ],
  "mn-mong-mn": [
    "mn-mong-mn",
    "mn-mong",
    "mn",
    ""
  ],
  "mni": [
    "mni",
    ""
  ],
  "mni-in": [
    "mni-in",
    "mni",
    ""
  ],
  "moh": [
    "moh",
    ""
  ],
  "moh-ca": [
    "moh-ca",
    "moh",
    ""
  ],
  "mr": [
    "mr",
    ""
  ],
  "mr-in": [
    "mr-in",
    "mr",
    ""
  ],
  "ms": [
    "ms",
    ""
  ],
  "ms-bn": [
    "ms-bn",
    "ms",
    ""
  ],
  "ms-my": [
    "ms-my",
    "ms",
    ""
  ],
  "ms-sg": [
    "ms-sg",
    "ms",
    ""
  ],
  "mt": [
    "mt",
    ""
  ],
  "mt-mt": [
    "mt-mt",
    "mt",
    ""
  ],
  "mua": [
    "mua",
    ""
  ],
  "mua-cm": [
    "mua-cm",
    "mua",
    ""
  ],
  "my": [
    "my",
    ""
  ],
  "my-mm": [
    "my-mm",
    "my",
    ""
  ],
  "mzn": [
    "mzn",
    ""
  ],
  "mzn-ir": [
    "mzn-ir",
    "mzn",
    ""
  ],
  "naq": [
    "naq",
    ""
  ],
  "naq-na": [
    "naq-na",
    "naq",
    ""
  ],
  "nb": [
    "nb",
    ""
  ],
  "nb-no": [
    "nb-no",
    "nb",
    ""
  ],
  "nb-sj": [
    "nb-sj",
    "nb",
    ""
  ],
  "nd": [
    "nd",
    ""
  ],
  "nd-zw": [
    "nd-zw",
    "nd",
    ""
  ],
  "nds": [
    "nds",
    ""
  ],
  "nds-de": [
    "nds-de",
    "nds",
    ""
  ],
  "nds-nl": [
    "nds-nl",
    "nds",
    ""
  ],
  "ne": [
    "ne",
    ""
  ],
  "ne-in": [
    "ne-in",
    "ne",
    ""
  ],
  "ne-np": [
    "ne-np",
    "ne",
    ""
  ],
  "nl": [
    "nl",
    ""
  ],
  "nl-aw": [
    "nl-aw",
    "nl",
    ""
  ],
  "nl-be": [
    "nl-be",
    "nl",
    ""
  ],
  "nl-bq": [
    "nl-bq",
    "nl",
    ""
  ],
  "nl-cw": [
    "nl-cw",
    "nl",
    ""
  ],
  "nl-nl": [
    "nl-nl",
    "nl",
    ""
  ],
  "nl-sr": [
    "nl-sr",
    "nl",
    ""
  ],
  "nl-sx": [
    "nl-sx",
    "nl",
    ""
  ],
  "nmg": [
    "nmg",
    ""
  ],
  "nmg-cm": [
    "nmg-cm",
    "nmg",
    ""
  ],
  "nn": [
    "nn",
    ""
  ],
  "nn-no": [
    "nn-no",
    "nn",
    ""
  ],
  "nnh": [
    "nnh",
    ""
  ],
  "nnh-cm": [
    "nnh-cm",
    "nnh",
    ""
  ],
  "no": [
    "no",
    ""
  ],
  "nqo": [
    "nqo",
    ""
  ],
  "nqo-gn": [
    "nqo-gn",
    "nqo",
    ""
  ],
  "nr": [
    "nr",
    ""
  ],
  "nr-za": [
    "nr-za",
    "nr",
    ""
  ],
  "nso": [
    "nso",
    ""
  ],
  "nso-za": [
    "nso-za",
    "nso",
    ""
  ],
  "nus": [
    "nus",
    ""
  ],
  "nus-ss": [
    "nus-ss",
    "nus",
    ""
  ],
  "nyn": [
    "nyn",
    ""
  ],
  "nyn-ug": [
    "nyn-ug",
    "nyn",
    ""
  ],
  "oc": [
    "oc",
    ""
  ],
  "oc-fr": [
    "oc-fr",
    "oc",
    ""
  ],
  "om": [
    "om",
    ""
  ],
  "om-et": [
    "om-et",
    "om",
    ""
  ],
  "om-ke": [
    "om-ke",
    "om",
    ""
  ],
  "or": [
    "or",
    ""
  ],
  "or-in": [
    "or-in",
    "or",
    ""
  ],
  "os": [
    "os",
    ""
  ],
  "os-ge": [
    "os-ge",
    "os",
    ""
  ],
  "os-ru": [
    "os-ru",
    "os",
    ""
  ],
  "pa": [
    "pa",
    ""
  ],
  "pa-arab": [
    "pa-arab",
    "pa",
    ""
  ],
  "pa-arab-pk": [
    "pa-arab-pk",
    "pa-arab",
    "pa",
    ""
  ],
  "pa-guru": [
    "pa-guru",
    "pa",
    ""
  ],
  "pa-in": [
    "pa-in",
    "pa",
    ""
  ],
  "pap": [
    "pap",
    ""
  ],
  "pap-029": [
    "pap-029",
    "pap",
    ""
  ],
  "pl": [
    "pl",
    ""
  ],
  "pl-pl": [
    "pl-pl",
    "pl",
    ""
  ],
  "prg": [
    "prg",
    ""
  ],
  "prg-001": [
    "prg-001",
    "prg",
    ""
  ],
  "prs": [
    "prs",
    ""
  ],
  "prs-af": [
    "prs-af",
    "prs",
    ""
  ],
  "ps": [
    "ps",
    ""
  ],
  "ps-af": [
    "ps-af",
    "ps",
    ""
  ],
  "pt": [
    "pt",
    ""
  ],
  "pt-ao": [
    "pt-ao",
    "pt",
    ""
  ],
  "pt-br": [
    "pt-br",
    "pt",
    ""
  ],
  "pt-ch": [
    "pt-ch",
    "pt",
    ""
  ],
  "pt-cv": [
    "pt-cv",
    "pt",
    ""
  ],
  "pt-gq": [
    "pt-gq",
    "pt",
    ""
  ],
  "pt-gw": [
    "pt-gw",
    "pt",
    ""
  ],
  "pt-lu": [
    "pt-lu",
    "pt",
    ""
  ],
  "pt-mo": [
    "pt-mo",
    "pt",
    ""
  ],
  "pt-mz": [
    "pt-mz",
    "pt",
    ""
  ],
  "pt-pt": [
    "pt-pt",
    "pt",
    ""
  ],
  "pt-st": [
    "pt-st",
    "pt",
    ""
  ],
  "pt-tl": [
    "pt-tl",
    "pt",
    ""
  ],
  "quc": [
    "quc",
    ""
  ],
  "quc-latn": [
    "quc-latn",
    "quc",
    ""
  ],
  "quc-latn-gt": [
    "quc-latn-gt",
    "quc-latn",
    "quc",
    ""
  ],
  "quz": [
    "quz",
    ""
  ],
  "quz-bo": [
    "quz-bo",
    "quz",
    ""
  ],
  "quz-ec": [
    "quz-ec",
    "quz",
    ""
  ],
  "quz-pe": [
    "quz-pe",
    "quz",
    ""
  ],
  "rm": [
    "rm",
    ""
  ],
  "rm-ch": [
    "rm-ch",
    "rm",
    ""
  ],
  "rn": [
    "rn",
    ""
  ],
  "rn-bi": [
    "rn-bi",
    "rn",
    ""
  ],
  "ro": [
    "ro",
    ""
  ],
  "ro-md": [
    "ro-md",
    "ro",
    ""
  ],
  "ro-ro": [
    "ro-ro",
    "ro",
    ""
  ],
  "rof": [
    "rof",
    ""
  ],
  "rof-tz": [
    "rof-tz",
    "rof",
    ""
  ],
  "ru": [
    "ru",
    ""
  ],
  "ru-by": [
    "ru-by",
    "ru",
    ""
  ],
  "ru-kg": [
    "ru-kg",
    "ru",
    ""
  ],
  "ru-kz": [
    "ru-kz",
    "ru",
    ""
  ],
  "ru-md": [
    "ru-md",
    "ru",
    ""
  ],
  "ru-ru": [
    "ru-ru",
    "ru",
    ""
  ],
  "ru-ua": [
    "ru-ua",
    "ru",
    ""
  ],
  "rw": [
    "rw",
    ""
  ],
  "rw-rw": [
    "rw-rw",
    "rw",
    ""
  ],
  "rwk": [
    "rwk",
    ""
  ],
  "rwk-tz": [
    "rwk-tz",
    "rwk",
    ""
  ],
  "sa": [
    "sa",
    ""
  ],
  "sa-in": [
    "sa-in",
    "sa",
    ""
  ],
  "sah": [
    "sah",
    ""
  ],
  "sah-ru": [
    "sah-ru",
    "sah",
    ""
  ],
  "saq": [
    "saq",
    ""
  ],
  "saq-ke": [
    "saq-ke",
    "saq",
    ""
  ],
  "sbp": [
    "sbp",
    ""
  ],
  "sbp-tz": [
    "sbp-tz",
    "sbp",
    ""
  ],
  "sd": [
    "sd",
    ""
  ],
  "sd-arab": [
    "sd-arab",
    "sd",
    ""
  ],
  "sd-arab-pk": [
    "sd-arab-pk",
    "sd-arab",
    "sd",
    ""
  ],
  "sd-deva": [
    "sd-deva",
    "sd",
    ""
  ],
  "sd-deva-in": [
    "sd-deva-in",
    "sd-deva",
    "sd",
    ""
  ],
  "se": [
    "se",
    ""
  ],
  "se-fi": [
    "se-fi",
    "se",
    ""
  ],
  "se-no": [
    "se-no",
    "se",
    ""
  ],
  "se-se": [
    "se-se",
    "se",
    ""
  ],
  "seh": [
    "seh",
    ""
  ],
  "seh-mz": [
    "seh-mz",
    "seh",
    ""
  ],
  "ses": [
    "ses",
    ""
  ],
  "ses-ml": [
    "ses-ml",
    "ses",
    ""
  ],
  "sg": [
    "sg",
    ""
  ],
  "sg-cf": [
    "sg-cf",
    "sg",
    ""
  ],
  "shi": [
    "shi",
    ""
  ],
  "shi-latn": [
    "shi-latn",
    "shi",
    ""
  ],
  "shi-latn-ma": [
    "shi-latn-ma",
    "shi-latn",
    "shi",
    ""
  ],
  "shi-tfng": [
    "shi-tfng",
    "shi",
    ""
  ],
  "shi-tfng-ma": [
    "shi-tfng-ma",
    "shi-tfng",
    "shi",
    ""
  ],
  "si": [
    "si",
    ""
  ],
  "si-lk": [
    "si-lk",
    "si",
    ""
  ],
  "sk": [
    "sk",
    ""
  ],
  "sk-sk": [
    "sk-sk",
    "sk",
    ""
  ],
  "sl": [
    "sl",
    ""
  ],
  "sl-si": [
    "sl-si",
    "sl",
    ""
  ],
  "sma": [
    "sma",
    ""
  ],
  "sma-no": [
    "sma-no",
    "sma",
    ""
  ],
  "sma-se": [
    "sma-se",
    "sma",
    ""
  ],
  "smj": [
    "smj",
    ""
  ],
  "smj-no": [
    "smj-no",
    "smj",
    ""
  ],
  "smj-se": [
    "smj-se",
    "smj",
    ""
  ],
  "smn": [
    "smn",
    ""
  ],
  "smn-fi": [
    "smn-fi",
    "smn",
    ""
  ],
  "sms": [
    "sms",
    ""
  ],
  "sms-fi": [
    "sms-fi",
    "sms",
    ""
  ],
  "sn": [
    "sn",
    ""
  ],
  "sn-latn": [
    "sn-latn",
    "sn",
    ""
  ],
  "sn-latn-zw": [
    "sn-latn-zw",
    "sn-latn",
    "sn",
    ""
  ],
  "so": [
    "so",
    ""
  ],
  "so-dj": [
    "so-dj",
    "so",
    ""
  ],
  "so-et": [
    "so-et",
    "so",
    ""
  ],
  "so-ke": [
    "so-ke",
    "so",
    ""
  ],
  "so-so": [
    "so-so",
    "so",
    ""
  ],
  "sq": [
    "sq",
    ""
  ],
  "sq-al": [
    "sq-al",
    "sq",
    ""
  ],
  "sq-mk": [
    "sq-mk",
    "sq",
    ""
  ],
  "sq-xk": [
    "sq-xk",
    "sq",
    ""
  ],
  "sr": [
    "sr",
    ""
  ],
  "sr-cyrl": [
    "sr-cyrl",
    "sr",
    ""
  ],
  "sr-cyrl-ba": [
    "sr-cyrl-ba",
    "sr-cyrl",
    "sr",
    ""
  ],
  "sr-cyrl-me": [
    "sr-cyrl-me",
    "sr-cyrl",
    "sr",
    ""
  ],
  "sr-cyrl-rs": [
    "sr-cyrl-rs",
    "sr-cyrl",
    "sr",
    ""
  ],
  "sr-cyrl-xk": [
    "sr-cyrl-xk",
    "sr-cyrl",
    "sr",
    ""
  ],
  "sr-latn": [
    "sr-latn",
    "sr",
    ""
  ],
  "sr-latn-ba": [
    "sr-latn-ba",
    "sr-latn",
    "sr",
    ""
  ],
  "sr-latn-me": [
    "sr-latn-me",
    "sr-latn",
    "sr",
    ""
  ],
  "sr-latn-rs": [
    "sr-latn-rs",
    "sr-latn",
    "sr",
    ""
  ],
  "sr-latn-xk": [
    "sr-latn-xk",
    "sr-latn",
    "sr",
    ""
  ],
  "ss": [
    "ss",
    ""
  ],
  "ss-sz": [
    "ss-sz",
    "ss",
    ""
  ],
  "ss-za": [
    "ss-za",
    "ss",
    ""
  ],
  "ssy": [
    "ssy",
    ""
  ],
  "ssy-er": [
    "ssy-er",
    "ssy",
    ""
  ],
  "st": [
    "st",
    ""
  ],
  "st-ls": [
    "st-ls",
    "st",
    ""
  ],
  "st-za": [
    "st-za",
    "st",
    ""
  ],
  "sv": [
    "sv",
    ""
  ],
  "sv-ax": [
    "sv-ax",
    "sv",
    ""
  ],
  "sv-fi": [
    "sv-fi",
    "sv",
    ""
  ],
  "sv-se": [
    "sv-se",
    "sv",
    ""
  ],
  "sw": [
    "sw",
    ""
  ],
  "sw-cd": [
    "sw-cd",
    "sw",
    ""
  ],
  "sw-ke": [
    "sw-ke",
    "sw",
    ""
  ],
  "sw-tz": [
    "sw-tz",
    "sw",
    ""
  ],
  "sw-ug": [
    "sw-ug",
    "sw",
    ""
  ],
  "syr": [
    "syr",
    ""
  ],
  "syr-sy": [
    "syr-sy",
    "syr",
    ""
  ],
  "ta": [
    "ta",
    ""
  ],
  "ta-in": [
    "ta-in",
    "ta",
    ""
  ],
  "ta-lk": [
    "ta-lk",
    "ta",
    ""
  ],
  "ta-my": [
    "ta-my",
    "ta",
    ""
  ],
  "ta-sg": [
    "ta-sg",
    "ta",
    ""
  ],
  "te": [
    "te",
    ""
  ],
  "te-in": [
    "te-in",
    "te",
    ""
  ],
  "teo": [
    "teo",
    ""
  ],
  "teo-ke": [
    "teo-ke",
    "teo",
    ""
  ],
  "teo-ug": [
    "teo-ug",
    "teo",
    ""
  ],
  "tg": [
    "tg",
    ""
  ],
  "tg-cyrl": [
    "tg-cyrl",
    "tg",
    ""
  ],
  "tg-cyrl-tj": [
    "tg-cyrl-tj",
    "tg-cyrl",
    "tg",
    ""
  ],
  "th": [
    "th",
    ""
  ],
  "th-th": [
    "th-th",
    "th",
    ""
  ],
  "ti": [
    "ti",
    ""
  ],
  "ti-er": [
    "ti-er",
    "ti",
    ""
  ],
  "ti-et": [
    "ti-et",
    "ti",
    ""
  ],
  "tig": [
    "tig",
    ""
  ],
  "tig-er": [
    "tig-er",
    "tig",
    ""
  ],
  "tk": [
    "tk",
    ""
  ],
  "tk-tm": [
    "tk-tm",
    "tk",
    ""
  ],
  "tn": [
    "tn",
    ""
  ],
  "tn-bw": [
    "tn-bw",
    "tn",
    ""
  ],
  "tn-za": [
    "tn-za",
    "tn",
    ""
  ],
  "to": [
    "to",
    ""
  ],
  "to-to": [
    "to-to",
    "to",
    ""
  ],
  "tr": [
    "tr",
    ""
  ],
  "tr-cy": [
    "tr-cy",
    "tr",
    ""
  ],
  "tr-tr": [
    "tr-tr",
    "tr",
    ""
  ],
  "ts": [
    "ts",
    ""
  ],
  "ts-za": [
    "ts-za",
    "ts",
    ""
  ],
  "tt": [
    "tt",
    ""
  ],
  "tt-ru": [
    "tt-ru",
    "tt",
    ""
  ],
  "twq": [
    "twq",
    ""
  ],
  "twq-ne": [
    "twq-ne",
    "twq",
    ""
  ],
  "tzm": [
    "tzm",
    ""
  ],
  "tzm-arab": [
    "tzm-arab",
    "tzm",
    ""
  ],
  "tzm-arab-ma": [
    "tzm-arab-ma",
    "tzm-arab",
    "tzm",
    ""
  ],
  "tzm-latn": [
    "tzm-latn",
    "tzm",
    ""
  ],
  "tzm-latn-dz": [
    "tzm-latn-dz",
    "tzm-latn",
    "tzm",
    ""
  ],
  "tzm-latn-ma": [
    "tzm-latn-ma",
    "tzm-latn",
    "tzm",
    ""
  ],
  "tzm-tfng": [
    "tzm-tfng",
    "tzm",
    ""
  ],
  "tzm-tfng-ma": [
    "tzm-tfng-ma",
    "tzm-tfng",
    "tzm",
    ""
  ],
  "ug": [
    "ug",
    ""
  ],
  "ug-cn": [
    "ug-cn",
    "ug",
    ""
  ],
  "uk": [
    "uk",
    ""
  ],
  "uk-ua": [
    "uk-ua",
    "uk",
    ""
  ],
  "ur": [
    "ur",
    ""
  ],
  "ur-in": [
    "ur-in",
    "ur",
    ""
  ],
  "ur-pk": [
    "ur-pk",
    "ur",
    ""
  ],
  "uz": [
    "uz",
    ""
  ],
  "uz-arab": [
    "uz-arab",
    "uz",
    ""
  ],
  "uz-arab-af": [
    "uz-arab-af",
    "uz-arab",
    "uz",
    ""
  ],
  "uz-cyrl": [
    "uz-cyrl",
    "uz",
    ""
  ],
  "uz-cyrl-uz": [
    "uz-cyrl-uz",
    "uz-cyrl",
    "uz",
    ""
  ],
  "uz-latn": [
    "uz-latn",
    "uz",
    ""
  ],
  "uz-latn-uz": [
    "uz-latn-uz",
    "uz-latn",
    "uz",
    ""
  ],
  "vai": [
    "vai",
    ""
  ],
  "vai-latn": [
    "vai-latn",
    "vai",
    ""
  ],
  "vai-latn-lr": [
    "vai-latn-lr",
    "vai-latn",
    "vai",
    ""
  ],
  "vai-vaii": [
    "vai-vaii",
    "vai",
    ""
  ],
  "vai-vaii-lr": [
    "vai-vaii-lr",
    "vai-vaii",
    "vai",
    ""
  ],
  "ve": [
    "ve",
    ""
  ],
  "ve-za": [
    "ve-za",
    "ve",
    ""
  ],
  "vi": [
    "vi",
    ""
  ],
  "vi-vn": [
    "vi-vn",
    "vi",
    ""
  ],
  "vo": [
    "vo",
    ""
  ],
  "vo-001": [
    "vo-001",
    "vo",
    ""
  ],
  "vun": [
    "vun",
    ""
  ],
  "vun-tz": [
    "vun-tz",
    "vun",
    ""
  ],
  "wae": [
    "wae",
    ""
  ],
  "wae-ch": [
    "wae-ch",
    "wae",
    ""
  ],
  "wal": [
    "wal",
    ""
  ],
  "wal-et": [
    "wal-et",
    "wal",
    ""
  ],
  "wo": [
    "wo",
    ""
  ],
  "wo-sn": [
    "wo-sn",
    "wo",
    ""
  ],
  "xh": [
    "xh",
    ""
  ],
  "xh-za": [
    "xh-za",
    "xh",
    ""
  ],
  "xog": [
    "xog",
    ""
  ],
  "xog-ug": [
    "xog-ug",
    "xog",
    ""
  ],
  "yav": [
    "yav",
    ""
  ],
  "yav-cm": [
    "yav-cm",
    "yav",
    ""
  ],
  "yi": [
    "yi",
    ""
  ],
  "yi-001": [
    "yi-001",
    "yi",
    ""
  ],
  "yo": [
    "yo",
    ""
  ],
  "yo-bj": [
    "yo-bj",
    "yo",
    ""
  ],
  "yo-ng": [
    "yo-ng",
    "yo",
    ""
  ],
  "zgh": [
    "zgh",
    ""
  ],
  "zgh-tfng": [
    "zgh-tfng",
    "zgh",
    ""
  ],
  "zgh-tfng-ma": [
    "zgh-tfng-ma",
    "zgh-tfng",
    "zgh",
    ""
  ],
  "zh": [
    "zh",
    ""
  ],
  "zh-cn": [
    "zh-cn",
    "zh",
    ""
  ],
  "zh-hans": [
    "zh-hans",
    "zh",
    ""
  ],
  "zh-hans-hk": [
    "zh-hans-hk",
    "zh-hans",
    "zh",
    ""
  ],
  "zh-hans-mo": [
    "zh-hans-mo",
    "zh-hans",
    "zh",
    ""
  ],
  "zh-hant": [
    "zh-hant",
    "zh",
    ""
  ],
  "zh-hk": [
    "zh-hk",
    "zh",
    ""
  ],
  "zh-mo": [
    "zh-mo",
    "zh",
    ""
  ],
  "zh-sg": [
    "zh-sg",
    "zh",
    ""
  ],
  "zh-tw": [
    "zh-tw",
    "zh",
    ""
  ],
  "zu": [
    "zu",
    ""
  ],
  "zu-za": [
    "zu-za",
    "zu",
    ""
  ]
}


*/
