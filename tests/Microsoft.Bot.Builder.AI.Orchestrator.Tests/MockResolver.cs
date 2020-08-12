// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.BotFramework.Orchestrator;

namespace Microsoft.Bot.Builder.AI.Orchestrator.Tests
{
    public class MockResolver : ILabelResolver
    {
        private readonly IReadOnlyList<Result> _score;

        public MockResolver(IReadOnlyList<Result> score)
        {
            this._score = score;
        }

        public bool AddExample(in Example example)
        {
            throw new NotImplementedException();
        }

        public bool AddSnapshot(in IReadOnlyList<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public bool AddSnapshot(in IReadOnlyList<byte> buffer, in string labels_prefix)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<byte> CreateSnapshot(bool include_examples)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string GetConfigJson()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Result> Score(in string text)
        {
            return _score;
        }

        public IReadOnlyList<Result> Score(in string text, in LabelType labelType)
        {
            throw new NotImplementedException();
        }

        public void SetRuntimeParams(in string config_or_path, bool reset_all)
        {
            throw new NotImplementedException();
        }
    }
}
