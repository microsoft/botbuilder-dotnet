// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    public class LanguageServicesTests 
    {
        /*protected override async Task<DialogTurnResult> DisplayQnAResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            base.DisplayQnAResultAsync()
            return await Task.FromResult<DialogTurnResult>(null);
        }
*/
        [Fact]
        public void TryingItOut()
        {
            var thisvar = "Hi";
            var var2 = "Hi";

            /*var res = this.DisplayQnAResultAsync(null, CancellationToken.None).GetAwaiter().GetResult();*/
            
            Assert.Equal(thisvar, var2);
        }
    }
}
