// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class Slot : IReadWriteSlot
    {
        public Slot(IFrame frame, ISlotDefinition definition)
        {
            this.Frame = frame ?? throw new ArgumentNullException(nameof(frame));
            this.Definition = definition ?? throw new ArgumentNullException(nameof(definition));

            this.Frame.AddSlot(this);
        }

        public ISlotDefinition Definition { get; private set; }

        public IFrame Frame { get; private set; }

        public IReadOnlySlot AsReadOnly()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(TurnContext context)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetAsync(TurnContext context)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasAsync(TurnContext context)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SlotHistoryValue<object>>> HistoryAsync(TurnContext context)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(TurnContext context, object value)
        {
            throw new NotImplementedException();
        }
    }
}
