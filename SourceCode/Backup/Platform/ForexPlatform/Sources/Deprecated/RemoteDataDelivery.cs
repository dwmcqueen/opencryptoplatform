//using System;
//using System.Collections.Generic;
//using System.Text;
//using Arbiter;
//using CommonFinancial;
//using CommonSupport;
//using System.Runtime.Serialization;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Implement dataDelivery delivery from a remote dataDelivery source (over the Arbiter messaging).
//    /// Class is not thread safe ?!.
//    /// Provides access to remote dataDelivery sources. Allows an expert sessionInformation to work with the dataDelivery
//    /// provided by the given remote source. A remote source, is any source that is accessed trough
//    /// the Arbiter communication mechanism (be it local or trough network).
//    /// </summary>
//    /// </summary>
//    [Serializable]
//    public class RemoteDataDelivery : RemoteSessionSourceOperational, ISourceDataDelivery
//    {
//        volatile int _defaultDataUpdateBarsCount = 5;
//        /// <summary>
//        /// 
//        /// </summary>
//        public int DefaultDataUpdateBarsCount
//        {
//            get { return _defaultDataUpdateBarsCount; }
//            set { _defaultDataUpdateBarsCount = value; }
//        }

//        volatile int _defaultDataBarsHistoryRetrieved = 500;
//        public int DefaultDataBarsHistoryRetrieved
//        {
//            get { return _defaultDataBarsHistoryRetrieved; }
//            set { _defaultDataBarsHistoryRetrieved = value; }
//        }

//        DataSubscriptionInfo _subscriptionInfo = new DataSubscriptionInfo(false, false, new TimeSpan[] { });

//        [field: NonSerialized]
//        public event QuoteUpdateDelegate QuoteUpdateEvent;

//        [field: NonSerialized]
//        public event DataHistoryUpdateDelegate DataHistoryUpdateEvent;

//        [field: NonSerialized]
//        public event DataDeliveryUpdateDelegate RuntimeSessionsUpdateEvent;

//        /// <summary>
//        /// 
//        /// </summary>
//        public RemoteDataDelivery(string name, Info account, List<ArbiterClientId?> forwardTransportation)
//            : base(name, account, forwardTransportation)
//        {
//        }

//        /// <summary>
//        /// Deserialization constructor.
//        /// </summary>
//        public RemoteDataDelivery(SerializationInfo orderInfo, StreamingContext context)
//            : base(orderInfo, context)
//        {
//        }

//        /// <summary>
//        /// Serialization routine.
//        /// </summary>
//        public override void GetObjectData(SerializationInfo orderInfo, StreamingContext context)
//        {
//            base.GetObjectData(orderInfo, context);
//        }

//        /// <summary>
//        /// Latest sessionInformation orderInfo assigned for usage.
//        /// </summary>
//        /// <param name="?"></param>
//        /// <returns></returns>
//        public bool LoadFromFile()
//        {
//            //if (base.LoadFromFile(account) == false)
//            //{
//            //    return false;
//            //}

//            //if (account.HasValue == false)
//            //{// Reuse the existing sessionInformation orderInfo.
//            //    if (base.Info.IsEmtpy)
//            //    {
//            //        return false;
//            //    }

//            //    account = base.Info;
//            //}

//            this.Name = "SessionDataProvider [" + Info.Name + "]";

//            ChangeOperationalState(OperationalStateEnum.Initialized);

//            //RequestQuoteUpdate(account.Value);

//            return true;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public new void UnInitialize()
//        {
//            ChangeOperationalState(OperationalStateEnum.UnInitialized);

//            base.UnInitialize();
//        }

//        /// <summary>
//        /// This is separate since its not an operation but a frequent update initiated by the server side.
//        /// </summary>
//        /// <param name="requestMessage"></param>
//        [MessageReceiver]
//        protected virtual void Receive(DataHistoryUpdateMessage requestMessage)
//        {
//            if (DataHistoryUpdateEvent != null)
//            {
//                DataHistoryUpdateEvent(this, requestMessage.Info, requestMessage.Update);
//            }
//        }

//        [MessageReceiver]
//        protected virtual void Receive(QuoteUpdateMessage requestMessage)
//        {
//            if (QuoteUpdateEvent != null && requestMessage.Quote.HasValue)
//            {
//                QuoteUpdateEvent(this, requestMessage.Info, requestMessage.Quote);
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="requestMessage"></param>
//        [MessageReceiver]
//        protected override void Receive(SubscriptionToSessionStartedMessage requestMessage)
//        {
//            base.Receive(requestMessage);
//            ChangeOperationalState(OperationalStateEnum.Operational);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        [MessageReceiver]
//        protected override ResultTransportMessage Receive(SubscriptionToSessionTerminatedMessage requestMessage)
//        {
//            ChangeOperationalState(OperationalStateEnum.NotOperational);
//            return base.Receive(requestMessage);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="requestMessage"></param>
//        [MessageReceiver]
//        protected override void Receive(SubscriptionToSessionUpdatedMessage requestMessage)
//        {
//            base.Receive(requestMessage);

//            SystemMonitor.NotImplementedCritical();

//            //bool periodsEqual = _sessionInformation.EqualAvailableDataBarPeriods(requestMessage.SessionInformation);

//            //// TODO: check here if all the subscribed periods are still available.

//            //_sessionInformation = requestMessage.SessionInformation;
            
//            //if (periodsEqual == false)
//            //{
//            //    //if (AvailableDataBarPeriodsUpdateEvent != null)
//            //    //{
//            //    //    AvailableDataBarPeriodsUpdateEvent(this);
//            //    //}
//            //}
//        }
        

//        #region ISourceDataDelivery Members

//        public bool RequestQuoteUpdate(Info account)
//        {
//            RequestQuoteUpdateMessage requestMessage = new RequestQuoteUpdateMessage(Info);

//            OperationResultMessage resultMessage =
//                SendAndReceiveForwarding<OperationResultMessage>(ForwardTransportationArray, requestMessage);

//            if (resultMessage != null && resultMessage.OperationResult)
//            {
//                Receive((QuoteUpdateMessage)resultMessage);
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public bool RequestDataHistoryUpdate(Info account, DataHistoryRequest request)
//        {
//            TracerHelper.Trace(this.Name);

//            if (this.OperationalState != OperationalStateEnum.Operational
//                && this.OperationalState != OperationalStateEnum.Initialized)
//            {
//                return false;
//            }

//            request.StartIndex = -1;

//            if (request.MaxValuesRetrieved.HasValue == false)
//            {
//                request.MaxValuesRetrieved = DefaultDataBarsHistoryRetrieved;
//            }

//            RequestDataHistoryMessage requestMessage = new RequestDataHistoryMessage(Info, request);

//            DataSessionResponceMessage resultMessage =
//                SendAndReceiveForwarding<DataSessionResponceMessage>(ForwardTransportationArray, requestMessage);

//            if (resultMessage != null && resultMessage.OperationResult && resultMessage is DataHistoryUpdateMessage)
//            {
//                DataHistoryUpdateMessage dataHistoryResultMessage = (DataHistoryUpdateMessage)resultMessage;
//                Receive((DataHistoryUpdateMessage)resultMessage);
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        #endregion


//        #region ISourceDataDelivery Members


//        public bool SubsribeToData(Info sessionInformation, DataSubscriptionInfo subscription)
//        {
//            lock (this)
//            {
//                DataSubscriptionRequestMessage request = new DataSubscriptionRequestMessage(sessionInformation, subscription);
//                DataSubscriptionResponceMessage responce = this.SendAndReceiveForwarding<DataSubscriptionResponceMessage>(
//                    ForwardTransportationArray, request);

//                return (responce != null && responce.OperationResult);
//            }
//        }

//        //public bool UnSubsribeToDataBarUpdates(TimeSpan period)
//        //{
//        //    lock (this)
//        //    {
//        //        if (_subscribedDataBarPeriods.Contains(period) == false)
//        //        {
//        //            SystemMonitor.OperationError("Period not subscribed [" + period.ToString() + " ].");
//        //            return false;
//        //        }

//        //        _subscribedDataBarPeriods.Remove(period);
//        //    }

//        //    if (SubscribedDataBarPeriodsUpdateEvent != null)
//        //    {
//        //        SubscribedDataBarPeriodsUpdateEvent(this);
//        //    }
//        //    return true;
//        //}

//        #endregion

//        #region ISourceDataDelivery Members

//        public RuntimeSessionInformation GetSessionRuntimeInformation(Info sessionInformation)
//        {
//            throw new NotImplementedException();
//        }

//        #endregion

//        #region ISourceDataDelivery Members

//        public List<BaseCurrency> SearchSymbols(string symbolMatchPattern)
//        {
//            throw new NotImplementedException();
//        }

//        public Info? GetSymbolDataSessionInfo(BaseCurrency baseCurrency)
//        {
//            throw new NotImplementedException();
//        }

//        #endregion
//    }

//}
