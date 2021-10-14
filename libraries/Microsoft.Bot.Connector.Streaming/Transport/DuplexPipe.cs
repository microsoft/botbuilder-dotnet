// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO.Pipelines;

namespace Microsoft.Bot.Connector.Streaming.Transport
{
    internal class DuplexPipe : IDuplexPipe
    {
        public DuplexPipe(PipeReader reader, PipeWriter writer)
        {
            Input = reader;
            Output = writer;
        }

        public PipeReader Input { get; }

        public PipeWriter Output { get; }

        public static DuplexPipePair CreateConnectionPair(PipeOptions inputOptions, PipeOptions outputOptions)
        {
            var input = new Pipe(inputOptions);
            var output = new Pipe(outputOptions);

            var transport = new DuplexPipe(output.Reader, input.Writer);
            var application = new DuplexPipe(input.Reader, output.Writer);

            return new DuplexPipePair(transport, application);
        }

        internal readonly struct DuplexPipePair
        {
            public DuplexPipePair(IDuplexPipe transport, IDuplexPipe application)
            {
                Transport = transport;
                Application = application;
            }

            public IDuplexPipe Transport { get; }

            public IDuplexPipe Application { get; }
        }
    }
}
