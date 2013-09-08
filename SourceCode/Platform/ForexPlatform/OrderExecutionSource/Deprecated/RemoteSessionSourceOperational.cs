//using System;
//using System.Collections.Generic;
//using System.Runtime.Serialization;
//using System.Threading;
//using Arbiter;
//using CommonFinancial;
//using CommonSupport;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Base class for classes that provide serices from a remote sessioned source (for ex. a DataDelivery or OrderExecutioners)
//    /// </summary>
//    [Serializable]
//    public abstract class RemoteSessionSourceOperational : OperationalTransportClient
//    {
//        protected List<ArbiterClientId?> _forwardTransportation = null;

//        public ArbiterClientId?[] ForwardTransportationArray
//        {
//            get { lock (this) { return _forwardTransportation.ToArray(); } }
//        }

//        protected RuntimeDataSessionInformation _sessionInformation = null;
//        /// <summary>
//        /// It is possible to have the sessionInfo change its ID in the course of operation.
//        /// </summary>
//        public DataSessionInfo SessionInfo
//        {
//            get { lock (this) { return _sessionInformation.Info; } }
//        }

//        public RuntimeDataSessionInformation RuntimeSessionInformation
//        {
//            get { lock (this) { return _sessionInformation; } }
//        }

//        public Symbol Symbol
//        {
//            get { lock (this) { return SessionInfo.Symbol; } }
//        }

//        /// <summary>
//        /// Default constructor.
//        /// </summary>
//        public RemoteSessionSourceOperational(string name, DataSessionInfo sessionInfo, List<ArbiterClientId?> forwardTransportation)
//            : base(name, false)
//        {
//            DefaultTimeOut = TimeSpan.FromSeconds(30);
//            _forwardTransportation = new List<ArbiterClientId?>(forwardTransportation);
//            _sessionInformation = new RuntimeDataSessionInformation(sessionInfo);
//        }

//        /// <summary>
//        /// Deserialization constructor.
//        /// </summary>
//        public RemoteSessionSourceOperational(SerializationInfo info, StreamingContext context)
//            : base(info, context)
//        {
//            _forwardTransportation = (List<ArbiterClientId?>)info.GetValue("forwardTransportation", typeof(List<ArbiterClientId?>));
//            _sessionInformation = (RuntimeDataSessionInformation)info.GetValue("sessionInfo", typeof(RuntimeDataSessionInformation));
//        }

//        /// <summary>
//        /// Serialization routine.
//        /// </summary>
//        public override void GetObjectData(SerializationInfo info, StreamingContext context)
//        {
//            base.GetObjectData(info, context);
//            info.AddValue("forwardTransportation", _forwardTransportation);
//            info.AddValue("sessionInfo", _sessionInformation);
//        }

//        /// <summary>
//        /// The passed in session info must be compatible with the previously used one.
//        /// </summary>
//        protected virtual bool Initialize(DataSessionInfo? sessionInfo)
//        {
//            if (sessionInfo.HasValue && DataSessionInfo.CheckSessionsLooseMatch(sessionInfo.Value, SessionInfo) == false)
//            {
//                SystemMonitor.Error("Mixed constructor and initialization sessions.");
//                return false;
//            }
//            else
//            {
//                sessionInfo = SessionInfo;
//            }

//            // This will invoke immediate State Update, if successfull.
//            ResponceMessage stateUpdateResult = this.SendAndReceiveForwarding<ResponceMessage>(
//                ForwardTransportationArray, new SubscribeToOperationalStateChangesMessage(true));

//            return (stateUpdateResult != null && stateUpdateResult.OperationResult);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        protected void UnInitialize()
//        {
//            if (OperationalState == OperationalStateEnum.Initialized ||
//                OperationalState == OperationalStateEnum.Operational)
//            {
//                lock (this)
//                {
//                    // UnSubscribe to source here.
//                    SendForwarding(_forwardTransportation, new UnSubscribeToSessionMessage(SessionInfo) { RequestConfirmation = false });

//                    // UnSubscribe to status updates too.
//                    this.SendForwarding(_forwardTransportation, new SubscribeToOperationalStateChangesMessage(false) { RequestResponce = false });
//                }
//            }
//        }

//        /// <summary>
//        /// Release all taken resources.
//        /// </summary>
//        public virtual void Dispose()
//        {
//        }

//        /// <summary>
//        /// Source has change state.
//        /// </summary>
//        [MessageReceiver]
//        protected virtual void Receive(OperationalStateChangeMessage message)
//        {
//            if (message.State == OperationalStateEnum.Operational)
//            {// Source is operational, try to subscribe.

//                ResponceMessage result = SendAndReceiveForwarding<ResponceMessage>(ForwardTransportationArray,
//                    new SubscribeToSessionMessage(SessionInfo) { SubscribeToNonExisting = true });

//                if (result == null || result.OperationResult == false)
//                {
//                    if (result != null)
//                    {
//                        SystemMonitor.OperationError("Failed to subscribe provider to session [" + SessionInfo.Name + ", " + result.ExceptionMessage + "]");
//                    }
//                    else
//                    {
//                        SystemMonitor.OperationError("Failed to subscribe provider to session [" + SessionInfo.Name + "].");
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        [MessageReceiver]
//        protected virtual void Receive(SubscriptionToSessionStartedMessage message)
//        {
//            lock (this)
//            {
//                _sessionInformation = message.AssignedInfo;
//            }
//        }

//        /// <summary>
//        /// Subscription has been terminated.
//        /// </summary>
//        [MessageReceiver]
//        protected virtual ResponceMessage Receive(SubscriptionToSessionTerminatedMessage message)
//        {
//            GeneralHelper.FireAndForget(delegate()
//            {// Resubscribe to session, since we lost it.
//                Thread.Sleep(1500);
//                ResponceMessage result = SendAndReceiveForwarding<ResponceMessage>(ForwardTransportationArray,
//                    new SubscribeToSessionMessage(SessionInfo) { SubscribeToNonExisting = true });
//            });

//            return new ResponceMessage(true);
//        }
//    }
//}
