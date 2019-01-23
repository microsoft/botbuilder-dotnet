//using System;
//using System.Collections.Generic;

//namespace Microsoft.Bot.Builder.Dialogs.Composition
//{
//    /// <summary>
//    /// Exception which captures that a value is ambigious and contains information necessary to disambiguate
//    /// </summary>
//    public class AmbigiousException<ValueT> : Exception
//    {
//        public AmbigiousException(ISlot slot, string message = null, Exception inner = null) : base(message, inner)
//        {
//            this.Slot = slot;
//        }

//        /// <summary>
//        /// The slot with invalid value
//        /// </summary>
//        public ISlot Slot { get; set; }

//        /// <summary>
//        /// Alternatives 
//        /// </summary>
//        public IList<Alternative<ValueT>> Alternatives { get; set; } = new List<Alternative<ValueT>>();
//    }

//    public class Alternative<ValueT>
//    {
//        public Alternative()
//        {
//        }

//        public Alternative(string name, ValueT value)
//        {
//            this.Name = name;
//            this.Value = value;
//        }

//        public string Name { get; set; }

//        public ValueT Value { get; set; } = default(ValueT);
//    }
//}
