using System;
using System.Collections.Generic;

namespace Chronic.Handlers
{
    public class EndianSpecificRegistry : HandlerRegistry
    {
        public EndianSpecificRegistry(EndianPrecedence precedence)
        {
            var handlers = new List<ComplexHandler>()
                {
                    Handle
                        .Required<ScalarMonth>()
                        .Required<SeparatorDate>()
                        .Required<ScalarDay>()
                        .Required<SeparatorDate>()
                        .Required<ScalarYear>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<SmSdSyHandler>(),

                    Handle            
                        .Required<ScalarDay>()
                        .Required<SeparatorDate>()
                        .Required<ScalarMonth>()
                        .Required<SeparatorDate>()
                        .Required<ScalarYear>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<SdSmSyHandler>()
                };

            switch (precedence)
            {
                case EndianPrecedence.Little:
                    {
                        handlers.Reverse();
                        Add(HandlerType.Endian, handlers);
                        break;
                    }
                case EndianPrecedence.Middle:
                    Add(HandlerType.Endian, handlers);
                    break;
                default:
                    throw new ArgumentException(String.Format(
                        "Unknown endian value {0}",
                        precedence));
            }
        }

    }
}