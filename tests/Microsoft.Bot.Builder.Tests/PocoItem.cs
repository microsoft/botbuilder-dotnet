// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    public class PocoItem
    {
        public string Id { get; set; }

        public int Count { get; set; }

        public string[] ExtraBytes { get; set; }
    }
}
