//using System;
//using System.Collections.Generic;
//using System.Text;
//using Arbiter;
//using CommonSupport;
//using System.Runtime.Serialization;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Source allows executing orders on remote locations, like integrations for ex.
//    /// The communication to the remote points is done trough Arbiter module messages.
//    /// </summary>
//    [Serializable]
//    [UserFriendlyName("Remote Order Execution Source")]
//    public class RemoteExecutionSource : OrderExecutionSource
//    {
//        ArbiterClientId? _sourceId;

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
//        public RemoteExecutionSource(string name, ArbiterClientId sourceId, bool isPersistableToDB)
//            : base(name, false)
//        {
//            this.DefaultTimeOut = TimeSpan.FromSeconds(45);

//            _sourceId = sourceId;
//            _isPersistableToDB = isPersistableToDB;
//        }

//        /// <summary>
//        /// Deserialization constructor.
//        /// </summary>
//        public RemoteExecutionSource(SerializationInfo info, StreamingContext context)
//            : base(info, context)
//        {
//            _sourceId = (ArbiterClientId)info.GetValue("sourceId", typeof(ArbiterClientId));
//        }

//        /// <summary>
//        /// Serialization routine.
//        /// </summary>
//        public override void GetObjectData(SerializationInfo info, StreamingContext context)
//        {
//            base.GetObjectData(info, context);
//            SystemMonitor.CheckThrow(_isPersistableToDB == true, "Object not persistable, not supposed to be serialized.");

//            info.AddValue("sourceId", _sourceId.Value);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        protected override bool OnUnInitialize()
//        {
//            if (Platform.GetComponentOperationalState(_sourceId) == OperationalStateEnum.Operational)
//            {
//                SendAddressed(_sourceId.Value, new ComponentUnInitializedMessage(this.SubscriptionClientID));
//            }

//            return base.OnUnInitialize();
//        }

//        /// <summary>
//        /// Receive a session operation request message.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <returns></returns>
//        [MessageReceiver]
//        public SessionOperationResponceMessage Receive(SessionOperationMessage message)
//        {
//            if (OperationalState != OperationalStateEnum.Operational)
//            {
//                return new SessionOperationResponceMessage(message.SessionInfo, message.OperationID, false);
//            }

//            List<TransportInfo> infos;
//            lock (this)
//            {
//                if (IsSessionAdded(message.SessionInfo) == false)
//                {// Session not added
//                    return new SessionOperationResponceMessage(message.SessionInfo, message.OperationID, false) { ExceptionMessage = "Session not present in source." };
//                }

//                infos = GetSessionSubscribers(message.SessionInfo);
//            }

//            foreach (TransportInfo info in infos)
//            {
//                if (message.TransportInfo.CheckOriginalSender(info))
//                {// Ok, message comes from a valid subscriber - send onwards.
//                    SessionOperationResponceMessage responceMessage = SendAndReceiveAddressed<SessionOperationResponceMessage>(_sourceId.Value, message);
//                    return responceMessage;
//                }
//            }

//            // Fall trough - failed to find a valid subsriber.
//            SystemMonitor.Error("Failed to verify session operation message subscriber.");

//            // Subscriber not found - deny message execution.
//            return new SessionOperationResponceMessage(message.SessionInfo, message.OperationID, false);
//        }

//        /// <summary>
//        /// Receive account information updated.
//        /// </summary>
//        /// <param name="message"></param>
//        [MessageReceiver]
//        void Receive(AccountInformationUpdateMessage message)
//        {
//            if (OperationalState != OperationalStateEnum.Operational)
//            {
//                SystemMonitor.OperationWarning("Using data source not in operational state.");
//                return;
//            }

//            lock (this)
//            {
//                if (IsSessionAdded(message.SessionInfo) == false)
//                {
//                    return;
//                }

//                SendRespondingToMany(GetSessionSubscribers(message.SessionInfo), message);
//            }

//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public override void UpdateSessions()
//        {
//            SendAddressed(_sourceId.Value, new GetSessionsUpdatesMessage() { RequestResponce = false });
//        }

//    }
//}
