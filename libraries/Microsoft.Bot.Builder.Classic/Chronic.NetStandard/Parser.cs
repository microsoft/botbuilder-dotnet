using System;
using System.Collections.Generic;
using System.Linq;
using Chronic.Handlers;

namespace Chronic
{
    public class Parser
    {
        public static bool IsDebugMode { get; set; }
        readonly Options _options;
        readonly HandlerRegistry _registry = new MyHandlerRegistry();
        readonly Tokenizer _tokenizer = new Tokenizer();

        public Parser()
            : this(new Options())
        {

        }

        public Parser(Options options)
        {
            _options = options;
            _registry.MergeWith(new EndianSpecificRegistry(options.EndianPrecedence));
        }

        public Span Parse(string phrase)
        {
            return Parse(phrase, _options);
        }

        public Span Parse(string phrase, Options options)
        {
            var taggedTokens = _tokenizer.Tokenize(phrase, options);
            var span = TokensToSpan(taggedTokens, options);
            Logger.Log(() => "=> " + (span != null ? span.ToString() : "<null>"));
            return span;
        }

        public Span TokensToSpan(IList<Token> tokens, Options options)
        {
            var handlersToMatch = _registry
                .GetHandlers(HandlerType.Endian)
                .Concat(_registry.GetHandlers(HandlerType.Date));

            foreach (var handler in handlersToMatch)
            {
                if (handler.Match(tokens, _registry))
                {
                    var targetTokens = tokens
                        .Where(x => x.IsNotTaggedAs<Separator>())
                        .ToList();
                    return ExecuteHandler(handler, targetTokens, options);
                }
            }

            foreach (var handler in _registry.GetHandlers(HandlerType.Anchor))
            {
                if (handler.Match(tokens, _registry))
                {
                    var targetTokens = tokens
                        .Where(x => x.IsNotTaggedAs<Separator>())
                        .ToList();
                    return ExecuteHandler(handler, targetTokens, options);
                }
            }

            foreach (var handler in _registry.GetHandlers(HandlerType.Arrow))
            {
                if (handler.Match(tokens, _registry))
                {
                    var targetTokens = tokens
                        .Where(x =>
                            x.IsNotTaggedAs<SeparatorAt>() &&
                            x.IsNotTaggedAs<SeparatorComma>() &&
                            x.IsNotTaggedAs<SeparatorDate>())
                        .ToList();
                    return ExecuteHandler(handler, targetTokens, options);
                }
            }

            foreach (var handler in _registry.GetHandlers(HandlerType.Narrow))
            {
                if (handler.Match(tokens, _registry))
                {
                    return ExecuteHandler(handler, tokens, options);
                }
            }

            return null;
        }

        static Span ExecuteHandler(ComplexHandler handler, IList<Token> tokens, Options options)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            if (tokens == null) throw new ArgumentNullException("tokens");
            if (options == null) throw new ArgumentNullException("options");

            Logger.Log(handler.ToString);
            if (handler.BaseHandler == null)
            {
                throw new InvalidOperationException(String.Format(
                    "No base handler found on complex handler: {0}.",
                    handler));
            }
            return handler.BaseHandler.Handle(tokens, options);
        }
    }
}
