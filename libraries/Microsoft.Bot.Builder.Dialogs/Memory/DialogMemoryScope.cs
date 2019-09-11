// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// DialogMemoryScope doesn't store data in TurnState but instead maps "dialog" -> dc.ActiveDialog.State
    /// </summary>
    public class DialogMemoryScope : MemoryScope
    {
        public DialogMemoryScope()
            : base(ScopePath.DIALOG)
        {
        }

        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            return dc.ActiveDialog?.State;
        }

        public override void SetMemory(DialogContext dc, object memory)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (memory == null)
            {
                throw new ArgumentNullException(nameof(memory));
            }
            
            dc.ActiveDialog.State = (IDictionary<string, object>)memory;
        }
    }
}
