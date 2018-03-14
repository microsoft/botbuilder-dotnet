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

using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;

namespace Microsoft.Bot.Builder.Classic.FormFlow
{
    [Serializable]
    internal class FormState
    {
        // Last sent prompt which is used when feedback is supplied
        public FormPrompt LastPrompt;

        // Used when navigating to reflect choices for next
        public NextStep Next;

        // Currently executing step
        public int Step;

        // History of executed steps
        public Stack<int> History;

        // Current phase of each step
        public StepPhase[] Phases;

        // Internal state of a step
        public object StepState;

        // Field number and input
        public List<Tuple<int, string>> FieldInputs;

        // True when we have started processing FieldInputs
        public bool ProcessInputs;

        public FormState(int steps)
        {
            Phases = new StepPhase[steps];
            Reset();
        }

        public void Reset()
        {
            LastPrompt = new FormPrompt();
            Next = null;
            Step = 0;
            History = new Stack<int>();
            Phases = new StepPhase[Phases.Length];
            StepState = null;
            FieldInputs = null;
            ProcessInputs = false;
        }

        public StepPhase Phase()
        {
            return Phases[Step];
        }

        public StepPhase Phase(int step)
        {
            return Phases[step];
        }

        public void SetPhase(StepPhase phase)
        {
            Phases[Step] = phase;
        }

        public void SetPhase(int step, StepPhase phase)
        {
            Phases[step] = phase;
        }
    }
}
