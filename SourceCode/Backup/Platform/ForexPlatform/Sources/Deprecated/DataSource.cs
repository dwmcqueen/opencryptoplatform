//using System;
//using System.Runtime.Serialization;
//using CommonFinancial;
//using CommonSupport;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Base abstract class for dataDelivery sources. A dataDelivery source is a component that provides trading information
//    /// dataDelivery to interested parties.
//    /// </summary>
//    [Serializable]
//    public abstract class DataSource : SessionSource
//    {
//        public delegate void SessionValuesUpdateDelegate(DataSource dataSource, Info account, int value);
//        /// <summary>
//        /// Source, sessionInformation, sessionInformation items length
//        /// </summary>
//        public event SessionValuesUpdateDelegate SessionValuesUpdateEvent;

//        /// <summary>
//        /// 
//        /// </summary>
//        public DataSource(string name, bool singleThreadMode)
//            : base(name, singleThreadMode)
//        {
//        }

//        /// <summary>
//        /// Deserialization constructor.
//        /// </summary>
//        public DataSource(SerializationInfo orderInfo, StreamingContext context)
//            : base(orderInfo, context)
//        {
//        }

//        /// <summary>
//        /// Serialization call.
//        /// </summary>
//        /// <param name="orderInfo"></param>
//        /// <param name="context"></param>
//        public override void GetObjectData(SerializationInfo orderInfo, StreamingContext context)
//        {
//            base.GetObjectData(orderInfo, context);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        protected void RaiseSessionValuesUpdateEvent(Info account, int itemsCount)
//        {
//            if (SessionValuesUpdateEvent != null)
//            {
//                SessionValuesUpdateEvent(this, account, itemsCount);
//            }
//        }
//    }
//}
