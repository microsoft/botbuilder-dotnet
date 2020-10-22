// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.TestBot.Shared.Services
{
    public class FlightBookingService : IFlightBookingService
    {
        public Task<bool> BookFlight(BookingDetails booking, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(true);
    }
}
