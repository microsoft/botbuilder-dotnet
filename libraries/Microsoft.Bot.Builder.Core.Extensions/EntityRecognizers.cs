// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class Entity
    {
        public string GroupName { get; set; }
        public double Score { get; set; }
        public object Value { get; set; }
    }    
}
