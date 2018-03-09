// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.Echo
{
    public interface IMyService
    {
        Task DoSomethingAsync();
    }

    public sealed class MyService : IMyService
    {
        public Task DoSomethingAsync() => Task.Delay(500);
    }
}