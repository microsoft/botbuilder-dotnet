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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    /// <summary>
    /// The root of the exception hierarchy related to <see cref="Internals.IDialogStack"/> .
    /// </summary>
    [Serializable]
    public abstract class DialogStackException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the DialogStackException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DialogStackException(string message, Exception inner)
            : base(message, inner)
        {
        }
        protected DialogStackException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// The exception representing no resume handler specified for the dialog stack.
    /// </summary>
    [Serializable]
    public sealed class NoResumeHandlerException : DialogStackException
    {
        /// <summary>
        /// Initializes a new instance of the NoResumeHandlerException class.
        /// </summary>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public NoResumeHandlerException(Exception inner)
            : base("IDialog method execution finished with no resume handler specified through IDialogStack.", inner)
        {
        }
        private NoResumeHandlerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// The exception representing multiple resume handlers specified for the dialog stack.
    /// </summary>
    [Serializable]
    public sealed class MultipleResumeHandlerException : DialogStackException
    {
        /// <summary>
        /// Initializes a new instance of the MultipleResumeHandlerException class.
        /// </summary>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public MultipleResumeHandlerException(Exception inner)
            : base("IDialog method execution finished with multiple resume handlers specified through IDialogStack.", inner)
        {
        }
        private MultipleResumeHandlerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// The root of the exception hierarchy related to prompts.
    /// </summary>
    [Serializable]
    public abstract class PromptException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the PromptException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public PromptException(string message)
            : base(message)
        {
        }
        protected PromptException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// The exception representing too many attempts by the user to answer the question asked by the prompt.
    /// </summary>
    [Serializable]
    public sealed class TooManyAttemptsException : PromptException
    {
        /// <summary>
        /// Initializes a new instance of the TooManyAttemptsException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TooManyAttemptsException(string message)
            : base(message)
        {
        }
        private TooManyAttemptsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
