using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Prompts;
using Microsoft.Recognizers.Text;
using static Microsoft.Bot.Builder.Dialogs.Prompts.PromptCultureModels;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    internal class TestLocale
    {
        public TestLocale(
            IPromptCultureModel cultureModel,
            string expectedPrompt = null,
            string inputThatResultsInOne = null,
            string inputThatResultsInZero = null)
        {
            if (cultureModel.Locale.Length != 5)
            {
                throw new ArgumentException("validLocale must be in format: es-es");
            }

            Culture = cultureModel;
            ValidLocale = cultureModel.Locale.ToString().ToLower();
            ExpectedPrompt = expectedPrompt;
            InputThatResultsInOne = inputThatResultsInOne;
            InputThatResultsInZero = inputThatResultsInZero;

            // es-ES
            CapEnding = GetCapEnding(ValidLocale);

            // es-Es
            TitleEnding = GetTitleEnding(ValidLocale);

            // ES
            CapTwoLetter = GetCapTwoLetter(ValidLocale);

            // es
            LowerTwoLetter = GetLowerTwoLetter(ValidLocale);
        }

        public string ValidLocale { get; }

        public string CapEnding { get; }

        public string TitleEnding { get; }

        public string CapTwoLetter { get; }

        public string LowerTwoLetter { get; }

        public string ExpectedPrompt { get; }

        public string InputThatResultsInOne { get; }

        public string InputThatResultsInZero { get; }

        public string Separator => Culture.Separator;

        public string InlineOr => Culture.InlineOr;

        public string InlineOrMore => Culture.InlineOrMore;

        private IPromptCultureModel Culture { get; }

        private string GetCapEnding(string validLocale)
        {
            return $"{validLocale[0]}{validLocale[1]}-{validLocale[3].ToString().ToUpper()}{validLocale[4].ToString().ToUpper()}";
        }

        private string GetTitleEnding(string validLocale)
        {
            return $"{validLocale[0]}{validLocale[1]}-{validLocale[3].ToString().ToUpper()}{validLocale[4]}";
        }

        private string GetCapTwoLetter(string validLocale)
        {
            return $"{validLocale[0].ToString().ToUpper()}{validLocale[1].ToString().ToUpper()}";
        }

        private string GetLowerTwoLetter(string validLocale)
        {
            return $"{validLocale[0]}{validLocale[1]}";
        }
    }
}
