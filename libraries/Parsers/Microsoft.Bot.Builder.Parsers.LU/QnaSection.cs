// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    /// <summary>
    /// Class for QnA sections.
    /// </summary>
    public class QnaSection : Section
    {
        private const string QNAGENERICSOURCE = "custom editorial";

        /// <summary>
        /// Initializes a new instance of the <see cref="QnaSection"/> class.
        /// </summary>
        /// <param name="parseTree">The qna context from the parse tree.</param>
        public QnaSection(LUFileParser.QnaSectionContext parseTree)
        {
            if (parseTree == null)
            {
                throw new ArgumentNullException(nameof(parseTree));
            }

            SectionType = SectionType.QnaSection;
            Questions = new List<string>() { ExtractQuestion(parseTree) };
            var result = ExtractMoreQuestions(parseTree);
            Questions.AddRange(result.questions);
            Errors = result.errors;
            var result2 = ExtractFilterPairs(parseTree);
            FilterPairs = result2.filterPairs;
            Errors.AddRange(result2.errors);
            Answer = ExtractAnswer(parseTree);
            var result3 = ExtractPrompts(parseTree);
            Prompts = result3.promptDefinitions;
            PromptsText = result3.promptTextList;
            Errors.AddRange(result3.errors);
            QAPairId = ExtractAssignedId(parseTree);
            Source = ExtractSourceInfo(parseTree);
            var startPosition = new Position { Line = parseTree.Start.Line, Character = parseTree.Start.Column };
            var stopPosition = new Position { Line = parseTree.Stop.Line, Character = parseTree.Stop.Column + parseTree.Stop.Text.Length };
            Range = new Range { Start = startPosition, End = stopPosition };
        }

        /// <summary>
        /// Gets the list of questions.
        /// </summary>
        /// <value>
        /// The list of questions. 
        /// </value>
        [JsonProperty("Questions")]
        public List<string> Questions { get; }

        /// <summary>
        /// Gets the list of Filter Pairs.
        /// </summary>
        /// <value>
        /// The list of filter pairs. 
        /// </value>
        [JsonProperty("FilterPairs")]
        public List<QnaTuple> FilterPairs { get; }

        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        /// <value>
        /// The answer to the qna questions. 
        /// </value>
        [JsonProperty("Answer")]
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the pair of question-answer.
        /// </summary>
        /// <value>
        /// The pair of question-answer. 
        /// </value>
        [JsonProperty("QAPairId")]
        public string QAPairId { get; set; }

        /// <summary>
        /// Gets the list of prompts.
        /// </summary>
        /// <value>
        /// The list of prompts. 
        /// </value>
        [JsonProperty("prompts")]
        public List<PromptDefinition> Prompts { get; }

        /// <summary>
        /// Gets the list of prompt's text.
        /// </summary>
        /// <value>
        /// The list of prompt's text. 
        /// </value>
        [JsonProperty("promptsText")]
        public List<string> PromptsText { get; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source. 
        /// </value>
        [JsonProperty("source")]
        public string Source { get; set; }

        private string ExtractQuestion(LUFileParser.QnaSectionContext parseTree)
        {
            return parseTree.qnaDefinition().qnaQuestion().questionText().GetText().Trim();
        }

        private (List<string> questions, List<Error> errors) ExtractMoreQuestions(LUFileParser.QnaSectionContext parseTree)
        {
            var questions = new List<string>();
            var errors = new List<Error>();
            var questionsBody = parseTree.qnaDefinition().moreQuestionsBody();
            foreach (var errorQuestionStr in questionsBody.errorQuestionString())
            {
                if (!string.IsNullOrEmpty(errorQuestionStr.GetText().Trim()))
                {
                    errors.Add(
                        Diagnostic.BuildDiagnostic(
                            message: $"Invalid QnA question line, did you miss '-' at line begin",
                            context: errorQuestionStr));
                }
            }

            foreach (var question in questionsBody.moreQuestion())
            {
                var questionText = question.GetText().Trim();
                questions.Add(questionText.Substring(1).Trim());
            }

            return (questions, errors);
        }

        private (List<QnaTuple> filterPairs, List<Error> errors) ExtractFilterPairs(LUFileParser.QnaSectionContext parseTree)
        {
            var filterPairs = new List<QnaTuple>();
            var errors = new List<Error>();
            var filterSection = parseTree.qnaDefinition().qnaAnswerBody().filterSection();
            if (filterSection != null)
            {
                if (filterSection.errorFilterLine() != null)
                {
                    foreach (var errorFilterLineStr in filterSection.errorFilterLine())
                    {
                        if (!string.IsNullOrEmpty(errorFilterLineStr.GetText().Trim()))
                        {
                            errors.Add(
                                Diagnostic.BuildDiagnostic(
                                    message: $"Invalid QnA filter line, did you miss '-' at line begin",
                                    context: errorFilterLineStr));
                        }
                    }
                }

                foreach (var filterLine in filterSection.filterLine())
                {
                    var filterLineText = filterLine.GetText().Trim();
                    filterLineText = filterLineText.Substring(1).Trim();
                    var filterPair = filterLineText.Split('=');
                    var key = filterPair[0].Trim();
                    var value = filterPair[1].Trim();
                    filterPairs.Add(new QnaTuple { Key = key, Value = value });
                }
            }

            return (filterPairs, errors);
        }

        private string ExtractAnswer(LUFileParser.QnaSectionContext parseTree)
        {
            var multiLineAnswer = parseTree.qnaDefinition().qnaAnswerBody().multiLineAnswer().GetText().Trim();

            // trim first and last line
            // TODO: validate this regex
            var answerRegexp = new Regex(@"^```(markdown)?\r*\n(?<answer>(.|\n|\r\n|\t| )*)\r?\n.*?```$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var answer_match = answerRegexp.Match(multiLineAnswer);
            var answer_group = answer_match.Groups["answer"];
            return answer_group.Success ? answer_group.Value : string.Empty;
        }

        private (List<PromptDefinition> promptDefinitions, List<string> promptTextList, List<Error> errors) ExtractPrompts(LUFileParser.QnaSectionContext parseTree)
        {
            var promptDefinitions = new List<PromptDefinition>();
            var promptTextList = new List<string>();
            var errors = new List<Error>();
            var promptSection = parseTree.qnaDefinition().promptSection();

            if (promptSection == null)
            {
                return (promptDefinitions, promptTextList: null, errors);
            }

            if (promptSection.errorFilterLine() != null)
            {
                foreach (var errorFilterLineStr in promptSection.errorFilterLine())
                {
                    if (!string.IsNullOrEmpty(errorFilterLineStr.GetText().Trim()))
                    {
                        errors.Add(
                            Diagnostic.BuildDiagnostic(
                                message: $"Invalid QnA prompt line, expecting '-' prefix for each line.",
                                context: errorFilterLineStr));
                    }
                }
            }

            foreach (var promptLine in promptSection.filterLine())
            {
                var filterLineText = promptLine.GetText().Trim();
                filterLineText = filterLineText.Substring(1).Trim();
                promptTextList.Add(filterLineText);
                var promptConfigurationRegExp = new Regex(@"^\[(?<displayText>.*?)]\([ ]*\#[ ]*[ ?]*(?<linkedQuestion>.*?)\)[ ]*(?<contextOnly>\`context-only\`)?.*?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var splitLineMatch = promptConfigurationRegExp.Match(filterLineText);
                if (!splitLineMatch.Success)
                {
                    errors.Add(
                        Diagnostic.BuildDiagnostic(
                            message: $"Invalid QnA prompt definition. Unable to parse prompt. Please verify syntax as well as question link.",
                            context: promptLine));
                }

                promptDefinitions.Add(
                    new PromptDefinition()
                    {
                        DisplayText = splitLineMatch.Groups["displayText"].Value,
                        LinkedQuestion = splitLineMatch.Groups["linkedQuestion"].Value,
                        ContextOnly = splitLineMatch.Groups["linkedQuestion"].Value
                    });
            }

            return (promptDefinitions, promptTextList, errors);
        }

        private string ExtractAssignedId(LUFileParser.QnaSectionContext parseTree)
        {
            var idAssignment = parseTree.qnaDefinition().qnaIdMark();
            if (idAssignment != null)
            {
                var idTextRegExp = new Regex(@"^\<a[ ]*id[ ]*=[ ]*[""\'](?<idCaptured>.*?)[""\'][ ]*>[ ]*\<\/a\>$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var idTextMatch = idTextRegExp.Match(idAssignment.GetText().Trim());
                return idTextMatch.Groups["idCaptured"].Success ? idTextMatch.Groups["idCaptured"].Value : null;
            }

            return null;
        }

        private string ExtractSourceInfo(LUFileParser.QnaSectionContext parseTree)
        {
            var srcAssignment = parseTree.qnaDefinition().qnaSourceInfo();
            if (srcAssignment != null)
            {
                var srcRegExp = new Regex(@"^[ ]*\>[ ]*!#[ ]*@qna.pair.source[ ]*=[ ]*(?<sourceInfo>.*?)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var srcMatch = srcRegExp.Match(srcAssignment.GetText().Trim());
                return srcMatch.Groups["sourceInfo"].Success ? srcMatch.Groups["sourceInfo"].Value : QNAGENERICSOURCE;
            }

            return QNAGENERICSOURCE;
        }
    }
}
