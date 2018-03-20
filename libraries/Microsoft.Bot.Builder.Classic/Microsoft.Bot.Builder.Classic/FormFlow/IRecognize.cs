// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    /// <summary>
    /// Enumeration of special kinds of matches.
    /// </summary>
    public enum SpecialValues
    {
        /// <summary>
        /// Match corresponds to a field, not a specific value in the field.
        /// </summary>
        Field
    };

    /// <summary>
    /// Describe a possible match in the user input.
    /// </summary>
    public class TermMatch
    {
        /// <summary>
        /// Construct a match.
        /// </summary>
        /// <param name="start">Start of match in input string.</param>
        /// <param name="length">Length of match in input string.</param>
        /// <param name="confidence">Confidence of match, 0-1.0.</param>
        /// <param name="value">The underlying C# value for the match.</param>
        public TermMatch(int start, int length, double confidence, object value)
        {
            Start = start;
            Length = length;
            Confidence = confidence;
            Value = value;
        }

        /// <summary>
        /// Start of match in input string.
        /// </summary>
        public readonly int Start;

        /// <summary>
        /// End of match in input string.
        /// </summary>
        public int End { get { return Start + Length; } }

        /// <summary>
        /// Length of match in input string.
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Confidence of match, 0-1.0.
        /// </summary>
        public readonly double Confidence;

        /// <summary>
        /// Underlying C# value.
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// Check to see if this covers the same span as match.
        /// </summary>
        /// <param name="match">TermMatch to compare.</param>
        /// <returns>True if both cover the same span.</returns>
        public bool Same(TermMatch match)
        {
            return Start == match.Start && End == match.End;
        }

        /// <summary>
        /// Check to see if this completely covers match.
        /// </summary>
        /// <param name="match">TermMatch to compare.</param>
        /// <returns>True if this covers all of match.</returns>
        public bool Covers(TermMatch match)
        {
            return Start <= match.Start && End >= match.End && (Start != match.Start || End != match.End);
        }

        /// <summary>
        /// Check to see if this overlaps with match in input.
        /// </summary>
        /// <param name="match">TermMatch to compare.</param>
        /// <returns>True if the matches overlap in the input.</returns>
        public bool Overlaps(TermMatch match)
        {
            return (match.Start <= End && Start <= match.Start && End <= match.End) // tmtm
                || (Start <= match.End && match.Start <= Start && match.End <= End) // mtmt
                || (Start <= match.Start && End >= match.End) // tmmt
                || (match.Start <= Start && match.End >= End) // mttm
                ;
        }

        public override string ToString()
        {
            return string.Format("TermMatch({0}, {1}, {2}-{3})", Value, Confidence, Start, Start + Length);
        }

        public override bool Equals(object obj)
        {
            return obj is TermMatch && this == (TermMatch)obj;
        }

        public static bool operator ==(TermMatch m1, TermMatch m2)
        {
            return ReferenceEquals(m1, m2) || (!ReferenceEquals(m1, null) && !ReferenceEquals(m2, null) && m1.Start == m2.Start && m1.Length == m2.Length && m1.Confidence == m2.Confidence && m1.Value == m2.Value);
        }

        public static bool operator !=(TermMatch m1, TermMatch m2)
        {
            return !(m1 == m2);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ Length.GetHashCode() ^ Confidence.GetHashCode() ^ Value.GetHashCode();
        }
    }

    /// <summary>
    /// Interface for recognizers that look for matches in user input.
    /// </summary>
    /// <typeparam name="T">Underlying form state.</typeparam>
    public interface IRecognize<T>
    {
        #region Documentation
        /// <summary>   Return the arguments to pass to the prompt. </summary>
        ///<remarks>For example a numeric recognizer might pass min and max values.</remarks>
        /// <returns>   An array of arguments.</returns>
        #endregion
        object[] PromptArgs();

        /// <summary>
        /// Return all possible values or null if a primitive type.
        /// </summary>
        /// <returns>All possible values.</returns>
        IEnumerable<object> Values();

        /// <summary>
        /// Return all possible value descriptions in order to support enumeration.
        /// </summary>
        /// <returns>All possible value descriptions.</returns>
        IEnumerable<DescribeAttribute> ValueDescriptions();

        /// <summary>
        /// Return the description of a specific value.
        /// </summary>
        /// <param name="value">Value to get description of.</param>
        /// <returns>Description of the value.</returns>
        DescribeAttribute ValueDescription(object value);

        /// <summary>
        /// Return valid inputs to describe a particular value.
        /// </summary>
        /// <param name="value">Value being checked.</param>
        /// <returns>Valid inputs for describing value.</returns>
        IEnumerable<string> ValidInputs(object value);

        /// <summary>
        /// Return the help string describing what are valid inputs to the recognizer.
        /// </summary>
        /// <returns>Help on what the recognizer accepts.</returns>
        string Help(T state, object defaultValue = null);

        /// <summary>
        /// Return the matches found in the input.
        /// </summary>
        /// <param name="input">The input activity being matched.</param>
        /// <param name="defaultValue">The default value or null if none.</param>
        /// <returns>Match records.</returns>
        IEnumerable<TermMatch> Matches(IMessageActivity input, object defaultValue = null);
    }
}
