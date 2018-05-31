// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AspNetCore_Multiple_Prompts
{
    /// <summary>
    /// Class for storing conversation state. 
    /// </summary>
    public class MultiplePromptsState : Dictionary<string, object>
    {
        private const string NameKey = "name";
        private const string AgeKey = "age";

        public MultiplePromptsState()
        {
            this[NameKey] = null;
            this[AgeKey] = 0;
        }

        public string Name
        {
            get { return (string)this[NameKey]; }
            set { this[NameKey] = value; }
        }

        public int Age
        {
            get { return (int)this[AgeKey]; }
            set { this[AgeKey] = value; }
        }
    }
}
