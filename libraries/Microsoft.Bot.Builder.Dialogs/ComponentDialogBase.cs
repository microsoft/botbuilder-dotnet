// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class ComponentDialogBase : Dialog
    {
       

        public ComponentDialogBase(string dialogId=null)
            : base(dialogId)
        {
           
        }

        

        
    }
}
