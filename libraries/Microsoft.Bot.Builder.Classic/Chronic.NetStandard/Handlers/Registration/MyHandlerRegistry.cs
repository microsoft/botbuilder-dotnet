using System.Collections.Generic;
using Chronic.Tags;
using Chronic.Tags.Repeaters;

namespace Chronic.Handlers
{
    public class MyHandlerRegistry : HandlerRegistry
    {
        public MyHandlerRegistry()
        {
            RegisterTimeHandler();
            RegisterDateHandlers();
            RegisterAnchorHandlers();
            RegisterArrowHandlers();
            RegisterNarrowHandlers();
        }

        void RegisterNarrowHandlers()
        {
            var handlers = new List<ComplexHandler>()
                {
                    Handle
                        .Required<Ordinal>()
                        .Required<IRepeater>()
                        .Required<Separator>()
                        .Required<IRepeater>()
                        .Using<ORSRHandler>(),
                    Handle
                        .Required<Ordinal>()
                        .Required<IRepeater>()
                        .Required<Grabber>()
                        .Required<IRepeater>()
                        .Using<ORGRHandler>(),
                    Handle
                        .Required<Grabber>()
                        .Required<IRepeater>()
                        .Required<Grabber>()
                        .Required<IRepeater>()
                        .Using<GRGRHandler>(),
                };
            Add(HandlerType.Narrow, handlers);
        }

        void RegisterArrowHandlers()
        {
            var handlers = new List<ComplexHandler>()
                {
                    Handle
                        .Required<Scalar>()
                        .Required<IRepeater>()
                        .Required<Pointer>()
                        .Using<SRPHandler>(),
                    Handle
                        .Required<Pointer>()
                        .Required<Scalar>()
                        .Required<IRepeater>()
                        .Using<PSRHandler>(),
                    Handle
                        .Required<Scalar>()
                        .Required<IRepeater>()
                        .Required<Pointer>()
                        .Required(HandlerType.Anchor)
                        .Using<SRPAHandler>(),
                };
            Add(HandlerType.Arrow, handlers);
        }

        void RegisterAnchorHandlers()
        {
            // tonight at 7pm
            var handlers = new List<ComplexHandler>()
                {
                    Handle
                        .Optional<Grabber>()
                        .Required<IRepeater>()
                        .Optional<SeparatorAt>()
                        .Optional<IRepeater>()
                        .Optional<IRepeater>()
                        .Using<RHandler>(),
                    Handle
                        .Optional<Grabber>()
                        .Required<IRepeater>()
                        .Required<IRepeater>()
                        .Optional<SeparatorAt>()
                        .Optional<IRepeater>()
                        .Optional<IRepeater>()
                        .Using<RHandler>(),
                    Handle
                        .Required<IRepeater>()
                        .Required<Grabber>()
                        .Required<IRepeater>()
                        .Using<RGRHandler>(),
                };
            Add(HandlerType.Anchor, handlers);
        }

        void RegisterDateHandlers()
        {
            var dateHandlers = new List<ComplexHandler>()
                {
                    Handle
                        .Required<RepeaterDayName>()
                        .Required<RepeaterMonthName>()
                        .Required<ScalarDay>()
                        .Using<RdnRmnSdHandler>(),
                    Handle
                        .Required<RepeaterDayName>()
                        .Required<RepeaterMonthName>()
                        .Required<OrdinalDay>()
                        .Using<RdnRmnOdHandler>(),
                    Handle
                        .Required<RepeaterMonthName>()
                        .Required<ScalarDay>()
                        .Optional<SeparatorComma>()
                        .Required<ScalarYear>()
                        .Using<RmnSdSyHandler>(),
                    Handle
                        .Required<RepeaterMonthName>()
                        .Required<OrdinalDay>()
                        .Optional<SeparatorComma>()
                        .Required<ScalarYear>()
                        .Using<RmnOdSyHandler>(),
                    Handle
                        .Required<RepeaterMonthName>()
                        .Required<ScalarDay>()
                        .Required<ScalarYear>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<RmnSdSyHandler>(),
                    Handle
                        .Required<RepeaterMonthName>()
                        .Required<OrdinalDay>()
                        .Required<ScalarYear>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<RmnOdSyHandler>(),
                    Handle
                        .Required<RepeaterMonthName>()
                        .Required<ScalarDay>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<RmnSdHandler>(),
                    Handle
                        .Required<RepeaterTime>()
                        .Optional<IRepeaterDayPortion>()
                        .Optional<SeparatorOn>()
                        .Required<RepeaterMonthName>()
                        .Required<ScalarDay>()
                        .Using<RmnSdOnHandler>(),
                    Handle
                        .Required<RepeaterMonthName>()
                        .Required<OrdinalDay>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<RmnOdHandler>(),
                    Handle
                        .Required<OrdinalDay>()
                        .Required<RepeaterMonthName>()
                        .Required<ScalarYear>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<OdRmnSyHandler>(),
                    Handle
                        .Required<OrdinalDay>()
                        .Required<RepeaterMonthName>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<OdRmnHandler>(),
                    Handle
                        .Required<ScalarYear>()
                        .Required<RepeaterMonthName>()
                        .Required<OrdinalDay>()
                        .Using<SyRmnOdHandler>(),
                    Handle
                        .Required<RepeaterTime>()
                        .Optional<IRepeaterDayPortion>()
                        .Optional<SeparatorOn>()
                        .Required<RepeaterMonthName>()
                        .Required<OrdinalDay>()
                        .Using<RmnOdOnHandler>(),


                    Handle
                        .Required<RepeaterMonthName>()
                        .Required<ScalarDay>()
                        .Required<ScalarYear>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<RmnSdSyHandler>(),
                    Handle
                        .Required<RepeaterMonthName>()
                        .Required<ScalarYear>()
                        .Using<RmnSyHandler>(),

                    Handle
                        .Required<ScalarDay>()
                        .Required<RepeaterMonthName>()
                        .Required<ScalarYear>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<SdRmnSyHandler>(),
                    Handle
                        .Required<ScalarDay>()
                        .Required<RepeaterMonthName>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<SdRmnHandler>(),
                    Handle

                        .Required<ScalarYear>()
                        .Required<SeparatorDate>()
                        .Required<ScalarMonth>()
                        .Required<SeparatorDate>()
                        .Required<ScalarDay>()
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<SySmSdHandler>(),


                    Handle
                        .Required<ScalarMonth>()
                        .Required<SeparatorDate>()
                        .Required<ScalarYear>()
                        .Using<SmSyHandler>(),

                    Handle
                        .Required<Scalar>()
                        .Required<IRepeater>()
                        .Optional<SeparatorComma>() 
                        .Required<Pointer>()
                        .Required(HandlerType.Anchor)
                        .Required<SeparatorAt>()
                        .Required(HandlerType.Time)
                        .Using<SRPAHandler>(),

                    Handle
                        .Repeat(pattern => pattern                        
                            .Required<Scalar>()
                            .Required<IRepeater>()
                            .Optional<SeparatorComma>() 
                        ).AnyNumberOfTimes()
                        .Required<Pointer>()
                        .Optional(HandlerType.Anchor)
                        .Optional<SeparatorAt>()
                        .Optional(HandlerType.Time)
                        .Using<MultiSRHandler>(),

                    //Handle
                    //    .Required<ScalarMonth>()
                    //    .Required<SeparatorDate>()
                    //    .Required<ScalarDay>()
                    //    .Using<SmSdHandler>(),

                    //Handle
                    //    .Required<ScalarMonth>()    
                    //    .Required<SeparatorDate>()                        
                    //    .Required<ScalarDay>()
                    //    .Required<SeparatorDate>()     
                    //    .Required<ScalarYear>()
                    //    .Optional<SeparatorAt>()
                    //    .Optional(HandlerType.Time)
                    //    .Using<SmSdSyHandler>(),
                    //Handle
                    //    .Required<ScalarDay>()
                    //    .Required<SeparatorDate>()                        
                    //    .Required<ScalarMonth>()    
                    //    .Required<SeparatorDate>()     
                    //    .Required<ScalarYear>()
                    //    .Optional<SeparatorAt>()
                    //    .Optional(HandlerType.Time)
                    //    .Using<SdSmSyHandler>(),




                };

            Add(HandlerType.Date, dateHandlers);
        }

        void RegisterTimeHandler()
        {
            var timeHandlers = new List<ComplexHandler>()
                {
                    Handle
                        .Required<RepeaterTime>()
                        .Optional<IRepeaterDayPortion>()
                        .UsingNothing(),
                };
            Add(HandlerType.Time, timeHandlers);
        }
    }
}