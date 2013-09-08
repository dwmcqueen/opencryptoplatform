//using System;
//using System.Collections.Generic;
//using System.Text;
//using Arbiter;
//using CommonSupport;
//using System.Runtime.Serialization;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Remote dataDelivery source class, allows the aquisition of trading dataDelivery from remote locations (like integrations).
//    /// Receives and provides its functionality trough the usage of Arbiter messages.
//    /// </summary>
//    [Serializable]
//    [UserFriendlyName("Remote Data OrderExecutionProvider Source")]
//    public class ProxyDataSource : DataSource
//    {
//        ArbiterClientId? _orderExecutionSourceId;

//        bool _isPersistableToDB = true;
//        public override bool IsPersistableToDB
//        {
//            get
//            {
//                return _isPersistableToDB;
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public ProxyDataSource(string name, ArbiterClientId? sourceSourceId, bool isPersistableToDB)
//            : base(name, false)
//        {
//            this.DefaultTimeOut = TimeSpan.FromSeconds(30);

//            _orderExecutionSourceId = sourceSourceId;
//            _isPersistableToDB = isPersistableToDB;
//        }

//        /// <summary>
//        /// Deserialization constructor.
//        /// </summary>
//        public ProxyDataSource(SerializationInfo orderInfo, StreamingContext context)
//            : base(orderInfo, context)
//        {
//            _orderExecutionSourceId = (ArbiterClientId)orderInfo.GetValue("remoteSourceId", typeof(ArbiterClientId));
//            _isPersistableToDB = orderInfo.GetBoolean("isPersistableToDB");
//        }

//        /// <summary>
//        /// Serialization routine.
//        /// </summary>
//        public override void GetObjectData(SerializationInfo orderInfo, StreamingContext context)
//        {
//            base.GetObjectData(orderInfo, context);

//            SystemMonitor.CheckThrow(IsPersistableToDB, "Object not persistable, not supposed to be serialized.");
//            orderInfo.AddValue("remoteSourceId", _orderExecutionSourceId.Value);
//            orderInfo.AddValue("isPersistableToDB", _isPersistableToDB);
//        }

//        /// <summary>
//        /// Source is being uninitialized.
//        /// </summary>
//        /// <returns></returns>
//        protected override bool OnUnInitialize()
//        {
//            if (Platform.GetComponentOperationalState(_orderExecutionSourceId) == OperationalStateEnum.Operational)
//            {
//                SendAddressed(_orderExecutionSourceId.Value, new ComponentUnInitializedMessage(this.SubscriptionClientID));
//            }

//            return base.OnUnInitialize();
//        }

//        #region Arbiter Messages

//        [MessageReceiver]
//        DataSessionResponceMessage Receive(RequestDataHistoryMessage requestMessage)
//        {
//            if (OperationalState != OperationalStateEnum.Operational)
//            {
//                return new DataSessionResponceMessage(requestMessage.Info, requestMessage.OperationId, false);
//            }

//            return SendAndReceiveAddressed<DataSessionResponceMessage>(_orderExecutionSourceId, requestMessage);
//        }

//        [MessageReceiver]
//        DataSessionResponceMessage Receive(RequestQuoteUpdateMessage requestMessage)
//        {
//            if (OperationalState != OperationalStateEnum.Operational)
//            {
//                return new DataSessionResponceMessage(requestMessage.Info, requestMessage.OperationId, false);
//            }

//            return SendAndReceiveAddressed<DataSessionResponceMessage>(_orderExecutionSourceId, requestMessage);
//        }

//        [MessageReceiver]
//        void Receive(QuoteUpdateMessage requestMessage)
//        {
//            if (OperationalState != OperationalStateEnum.Operational)
//            {
//                SystemMonitor.OperationWarning("Using dataDelivery source not in operational state.");
//                return;
//            }

//            lock (this)
//            {
//                SystemMonitor.NotImplementedCritical();
//                //if (IsSessionAdded(requestMessage) == false)
//                //{
//                //    return;
//                //}
//                //SendRespondingToMany(GetSessionSubscribers(requestMessage.Info), requestMessage);
//            }

//        }

//        [MessageReceiver]
//        void Receive(DataHistoryUpdateMessage requestMessage)
//        {
//            if (OperationalState != OperationalStateEnum.Operational)
//            {
//                SystemMonitor.OperationWarning("Using dataDelivery source not in operational state.");
//                return;
//            }

//            lock (this)
//            {
//                if (IsSessionAdded(requestMessage.Info) == false)
//                {
//                    return;
//                }

//                SendRespondingToMany(GetSessionSubscribers(requestMessage.Info), requestMessage);

//                RaiseSessionValuesUpdateEvent(requestMessage.Info, requestMessage.Update == null ? 0 : requestMessage.Update.DataBarsUnsafe.Count);
//            }
//        }

//        #endregion

//        public override void UpdateSessions()
//        {
//            SendAddressed(_orderExecutionSourceId.Value, new GetSessionsUpdatesMessage() { RequestResponce = false });
//        }
//    }
//}
