//using System;
//using System.Collections.Generic;
//using System.Runtime.Serialization;
//using Arbiter;
//using CommonFinancial;
//using CommonSupport;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Base class defines how a source (data, order execution etc.) should look like.
//    /// </summary>
//    [Serializable]
//    [ComponentManagement(20, true)]
//    public abstract class SessionSource : PlatformComponent
//    {// Default component level, fairly low to keep it starting early, before user level components that use it.

//        /// <summary>
//        /// Cleared on UnInit, not persisted in serialized state.
//        /// </summary>
//        List<TransportInfo> _sessionsUpdateSubscribers = new List<TransportInfo>();
        
//        /// <summary>
//        /// A list of all subscribers to these source sessions.
//        /// </summary>
//        protected List<TransportInfo> SessionsUpdateSubscribers
//        {
//            get { return _sessionsUpdateSubscribers; }
//        }

//        /// <summary>
//        /// Not persisted, those are subscriptions waiting to be started, since currently they are not active.
//        /// </summary>
//        Dictionary<DataSessionInfo, List<TransportInfo>> _pendingSubscriptions = new Dictionary<DataSessionInfo, List<TransportInfo>>();

//        // <summary>
//        // Persisted in the serialized state.
//        // </summary>
//        Dictionary<string, Dictionary<DataSessionInfo, RuntimeDataSessionInformation>> _sessionsGroups = new Dictionary<string, Dictionary<DataSessionInfo, RuntimeDataSessionInformation>>();

//        /// <summary>
//        /// 
//        /// </summary>
//        protected Dictionary<string, Dictionary<DataSessionInfo, RuntimeDataSessionInformation>> SessionsGroupsUnsafe
//        {
//            get { return _sessionsGroups; }
//        }

//        /// <summary>
//        /// Count of sessions groups.
//        /// </summary>
//        public int GroupsCount
//        {
//            get { lock (this) { return _sessionsGroups.Keys.Count; } }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public string[] Groups
//        {
//            get
//            {
//                lock (this)
//                {
//                    return GeneralHelper.EnumerableToArray<string>(_sessionsGroups.Keys);
//                }
//            }
//        }

//        public delegate void SessionSourceUpdateDelegate(SessionSource manager);

//        /// <summary>
//        /// 
//        /// </summary>
//        [field:NonSerialized]
//        public event SessionSourceUpdateDelegate SessionsUpdateEvent;

//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        public SessionSource(string name, bool singleThreadMode)
//            : base(name, singleThreadMode)
//        {
//        }

//        /// <summary>
//        /// Deserialization constructor.
//        /// </summary>
//        /// <param name="info"></param>
//        /// <param name="context"></param>
//        public SessionSource(SerializationInfo info, StreamingContext context)
//            :base(info, context)
//        {
//        }

//        /// <summary>
//        /// Custom serialization procedure.
//        /// </summary>
//        public override void GetObjectData(SerializationInfo info, StreamingContext context)
//        {
//            base.GetObjectData(info, context);
//        }

//        protected override bool OnInitialize(Platform platform)
//        {
//            if (base.OnInitialize(platform) == false)
//            {
//                return false;
//            }

//            ChangeOperationalState(OperationalStateEnum.Initialized);
//            return true;
//        }

//        void DoUninitialize()
//        {
//            List<DataSessionInfo> sessionInfos = new List<DataSessionInfo>();
//            lock (this)
//            {
//                foreach (string group in _sessionsGroups.Keys)
//                {
//                    sessionInfos.AddRange(_sessionsGroups[group].Keys);
//                }
//            }

//            RemoveSessions(sessionInfos);
//        }

//        protected override bool OnUnInitialize()
//        {
//            DoUninitialize();

//            ChangeOperationalState(OperationalStateEnum.UnInitialized);

//            return base.OnUnInitialize();
//        }

//        /// <summary>
//        /// This needs to be implement by child classes and refresh sessions.
//        /// </summary>
//        public abstract void UpdateSessions();

//        /// <summary>
//        /// Handle requests to change the operational state.
//        /// </summary>
//        /// <param name="newState"></param>
//        /// <returns></returns>
//        protected override bool OnChangeOperationalStateRequest(OperationalStateEnum newState)
//        {
//            base.OnChangeOperationalStateRequest(newState);

//            if (OperationalState == OperationalStateEnum.Initialized
//                && newState == OperationalStateEnum.Operational)
//            {// Initialized -> Operational.

//                UpdateSessions();

//                ChangeOperationalState(OperationalStateEnum.Operational);

//                return true;
//            }

//            if (OperationalState == OperationalStateEnum.Operational && 
//                (newState == OperationalStateEnum.NotOperational 
//                || newState == OperationalStateEnum.UnInitialized
//                || newState == OperationalStateEnum.Disposed))
//            {// Operational -> Not Operational.

//                DoUninitialize();

//                ChangeOperationalState(OperationalStateEnum.NotOperational);

//                return true;
//            }

//            if ((OperationalState == OperationalStateEnum.NotOperational || 
//                OperationalState == OperationalStateEnum.Unknown)
//                && newState == OperationalStateEnum.Operational)
//            {// Not Operational. -> Operational.

//                UpdateSessions();

//                ChangeOperationalState(OperationalStateEnum.Operational);

//                return true;
//            }

//            return false;
//        }

//        /// <summary>
//        /// Add source session.
//        /// </summary>
//        /// <param name="sessionInfo"></param>
//        protected void AddSessions(IEnumerable<RuntimeDataSessionInformation> sessions)
//        {
//            //TracerHelper.Trace("Adding " + sessions.Length.ToString() + " sessions.");

//            lock (this)
//            {
//                List<RuntimeDataSessionInformation> addedSessions = new List<RuntimeDataSessionInformation>();
//                foreach(RuntimeDataSessionInformation session in sessions)
//                {
//                    DataSessionInfo info = session.Info;
//                    if (_sessionsGroups.ContainsKey(info.Symbol.Group) == false)
//                    {// New group.
//                        _sessionsGroups.Add(info.Symbol.Group, new Dictionary<DataSessionInfo, RuntimeDataSessionInformation>());
//                    }
//                    else if (_sessionsGroups[info.Symbol.Group].ContainsKey(info))
//                    {// Session already added to its group.
//                        SystemMonitor.OperationWarning("Session already added.");
//                        continue;
//                    }
                    
//                    // New entry.
//                    _sessionsGroups[info.Symbol.Group].Add(info, session);
//                    addedSessions.Add(session);
//                }

//                // Update those that follow sessions in general.
//                SendRespondingToMany(_sessionsUpdateSubscribers, new SessionsUpdatesMessage(
//                    SessionsUpdatesMessage.UpdateTypeEnum.Added, addedSessions));

//                // Check for all pending subscriptions, if any of them have been satisfied by the current sessions.
//                foreach(DataSessionInfo pendingInfo in GeneralHelper.EnumerableToArray<DataSessionInfo>(_pendingSubscriptions.Keys))
//                {
//                    foreach (RuntimeDataSessionInformation session in addedSessions)
//                    {
//                        if (session.Info.CheckLooseMatch(pendingInfo))
//                        {// Found a map, look no longer.
                            
//                            foreach (TransportInfo transportInfo in _pendingSubscriptions[pendingInfo])
//                            {// Notify each waiting on this session assignment.
//                                HandleSessionSubscription(pendingInfo, session, transportInfo);
//                            }

//                            // This has been maped and is no longer pending.
//                            _pendingSubscriptions.Remove(pendingInfo);
//                            break;
//                        }
//                    }
//                }
//            } // lock (this)

//            if (SessionsUpdateEvent != null)
//            {
//                SessionsUpdateEvent(this);
//            }
//        }

//        /// <summary>
//        /// Remove existing session.
//        /// </summary>
//        /// <param name="session"></param>
//        protected void RemoveSession(DataSessionInfo session)
//        {
//            RemoveSessions(new DataSessionInfo[] { session });
//        }

//        protected void RemoveSessions(IEnumerable<RuntimeDataSessionInformation> sessions)
//        {
//            List<DataSessionInfo> infos = new List<DataSessionInfo>();
//            foreach (RuntimeDataSessionInformation session in sessions)
//            {
//                infos.Add(session.Info);
//            }

//            RemoveSessions(infos);
//        }

//            /// <summary>
//        /// Remove sessions with multiple input for optimization purposes on large session sets.
//        /// </summary>
//        /// <param name="sessions"></param>
//        protected void RemoveSessions(IEnumerable<DataSessionInfo> sessions)
//        {
//            TracerHelper.Trace("Removing sessions.");
//            List<RuntimeDataSessionInformation> removedSessions = new List<RuntimeDataSessionInformation>();
//            lock (this)
//            {
//                foreach (DataSessionInfo sessionInfo in sessions)
//                {
//                    if (IsSessionAdded(sessionInfo) == false)
//                    {// Already removed.
//                        continue;
//                    }

//                    // Update all subscribers to this particular session session is no londer available.
//                    SubscriptionToSessionTerminatedMessage message = new SubscriptionToSessionTerminatedMessage(sessionInfo);
//                    foreach (TransportInfo info in GetSessionSubscribers(sessionInfo))
//                    {// Wait (3 secs) for the responces to make sure all is well before uninitializing.
//                        // This is required since final uninit may be invoked in mixed order, so make sure everybody goes down properly.
//                        this.SendAndReceiveResponding<ResponceMessage>(info, message, TimeSpan.FromSeconds(3));
//                    }

//                    removedSessions.Add(_sessionsGroups[sessionInfo.Symbol.Group][sessionInfo]);
//                    _sessionsGroups[sessionInfo.Symbol.Group].Remove(sessionInfo);
//                }
//            }

//            DataSessionInfo[] sessionArray = GeneralHelper.EnumerableToArray<DataSessionInfo>(sessions);

//            // Send all at once to handle cases with very many sessions, update those that follow sessions in general.
//            SendRespondingToMany(_sessionsUpdateSubscribers, new SessionsUpdatesMessage(
//                SessionsUpdatesMessage.UpdateTypeEnum.Removed, removedSessions));

//            if (SessionsUpdateEvent != null)
//            {
//                SessionsUpdateEvent(this);
//            }

//            TracerHelper.TraceExit();
//        }

//        #region Messages From Source

//        /// <summary>
//        /// The remote session master is sending a sessions update notification.
//        /// </summary>
//        /// <param name="message"></param>
//        [MessageReceiver]
//        protected void Receive(SessionsUpdatesMessage message)
//        {// A new session was added or removed in the server.

//            switch (message.UpdateType)
//            {
//                case SessionsUpdatesMessage.UpdateTypeEnum.Added:
//                case SessionsUpdatesMessage.UpdateTypeEnum.Requested:
//                {
//                    AddSessions(message.Sessions);
//                }
//                 break;
//                case SessionsUpdatesMessage.UpdateTypeEnum.Removed:
//                {
//                    RemoveSessions(message.Sessions);
//                }
//                 break;
//                case SessionsUpdatesMessage.UpdateTypeEnum.Suspended:
//                 SystemMonitor.NotImplementedWarning("Sessions notification handling not implemented.");
//                 break;
//                case SessionsUpdatesMessage.UpdateTypeEnum.Resumed:
//                 SystemMonitor.NotImplementedWarning("Sessions notification handling not implemented.");
//                 break;
//                default:
//                 SystemMonitor.Error("Unknown session updates mode.");
//                 break;
//            }

//        }

//        #endregion


//        #region Messages From Users

//        [MessageReceiver]
//        protected SessionsUpdatesMessage Receive(GetSessionsUpdatesMessage message)
//        {
//            if (OperationalState != OperationalStateEnum.Operational)
//            {
//                SessionsUpdatesMessage responce = new SessionsUpdatesMessage(
//                    SessionsUpdatesMessage.UpdateTypeEnum.Requested);
//                return responce;
//            }

//            List<RuntimeDataSessionInformation> sessionsRequested = new List<RuntimeDataSessionInformation>();
//            foreach (string group in _sessionsGroups.Keys)
//            {
//                foreach (RuntimeDataSessionInformation information in _sessionsGroups[group].Values)
//                {
//                    if (message.IsSessionRequested(information.Info))
//                    {
//                        sessionsRequested.Add(information);
//                    }
//                }
//            }

//            lock (this)
//            {
//                SessionsUpdatesMessage responce = new SessionsUpdatesMessage(
//                    SessionsUpdatesMessage.UpdateTypeEnum.Requested, sessionsRequested);
//                return responce;
//            }
//        }

//        [MessageReceiver]
//        protected ResponceMessage Receive(SubscribeToSessionsUpdatesMessage message)
//        {
//            lock (this)
//            {
//                if (_sessionsUpdateSubscribers.Contains(message.TransportInfo) == false)
//                {
//                    _sessionsUpdateSubscribers.Add(message.TransportInfo);
//                }
//            }

//            return new ResponceMessage(true);
//        }

//        [MessageReceiver]
//        protected ResponceMessage Receive(UnSubscribeToSessionsUpdatesMessage message)
//        {
//            if (message.RequestConfirmation)
//            {
//                lock (this)
//                {
//                    return new ResponceMessage(_sessionsUpdateSubscribers.Remove(message.TransportInfo));
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        void HandleSessionSubscription(DataSessionInfo requestSessionInfo, RuntimeDataSessionInformation assignedSession, TransportInfo transportInfo)
//        {
//            lock (this)
//            {
//                List<TransportInfo> transport = GetSessionSubscribers(assignedSession);
//                if (transport.Contains(transportInfo) == false)
//                {
//                    transport.Add(transportInfo);
//                }
//            }

//            SendResponding(transportInfo, new SubscriptionToSessionStartedMessage(requestSessionInfo, assignedSession));
//        }

//        /// <summary>
//        /// Receive a request to subscribe for a session.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <returns></returns>
//        [MessageReceiver]
//        protected ResponceMessage Receive(SubscribeToSessionMessage message)
//        {
//            if (IsSessionAdded(message.SessionInfo) == false)
//            {
//                RuntimeDataSessionInformation mappedInfo = TryMapSessionInfo(message.SessionInfo);

//                if (mappedInfo != null)
//                {// Session found, remapped and added.
//                    HandleSessionSubscription(message.SessionInfo, mappedInfo, message.TransportInfo);
//                }
//                else
//                {
//                    if (message.SubscribeToNonExisting == false)
//                    {// Not mapped, future subscription not allowed, return false;
//                        SystemMonitor.OperationError("A request to a session info received, and session not present in this source.");
//                        return new ResponceMessage(false);
//                    }
//                    else
//                    {// Add to pending subscriptions.
//                        lock (this)
//                        {
//                            if (_pendingSubscriptions.ContainsKey(message.SessionInfo) == false)
//                            {
//                                _pendingSubscriptions.Add(message.SessionInfo, new List<TransportInfo>());
//                            }

//                            if (_pendingSubscriptions[message.SessionInfo].Contains(message.TransportInfo) == false)
//                            {
//                                _pendingSubscriptions[message.SessionInfo].Add(message.TransportInfo);
//                            }
//                        }
//                    }
//                }
//            }
//            else
//            {// Session found and subscribed.
//                HandleSessionSubscription(message.SessionInfo, _sessionsGroups[message.SessionInfo.Symbol.Group][message.SessionInfo], message.TransportInfo);
//            }

//            return new ResponceMessage(true);
//        }

//        /// <summary>
//        /// Receive a request to unsubscribe for a session.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <returns></returns>
//        [MessageReceiver]
//        protected ResponceMessage Receive(UnSubscribeToSessionMessage message)
//        {
//            bool result = false;
//            lock (this)
//            {
//                if (IsSessionAdded(message.SessionInfo))
//                {
//                    foreach (TransportInfo info in GetSessionSubscribers(message.SessionInfo))
//                    {
//                        if (info.CheckOriginalSender(message.TransportInfo))
//                        {
//                            List<TransportInfo> subscribers = GetSessionSubscribers(message.SessionInfo);
//                            result = subscribers.Remove(info);
//                            //result = _sessionsGroups[message.SessionInfo.Symbol.Group][message.SessionInfo].remo
//                            break;
//                        }
//                    }
//                }
//                else if (_pendingSubscriptions.ContainsKey(message.SessionInfo))
//                {
//                    _pendingSubscriptions[message.SessionInfo].Remove(message.TransportInfo);
//                }
//            }

//            if (message.RequestConfirmation)
//            {
//                return new ResponceMessage(result);
//            }
//            else
//            {
//                return null;
//            }
//        }

//        #endregion

//        public int GetGroupSessionsCount(string groupName)
//        {
//            lock (this)
//            {
//                if (_sessionsGroups.ContainsKey(groupName))
//                {
//                    return _sessionsGroups[groupName].Keys.Count;
//                }
//            }

//            SystemMonitor.Warning("Get sessions of a group not present.");
//            return 0;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public List<RuntimeDataSessionInformation> GetAllSessions(string sessionsGroup)
//        {
//            List<RuntimeDataSessionInformation> result = new List<RuntimeDataSessionInformation>();
//            lock (this)
//            {
//                foreach (string group in _sessionsGroups.Keys)
//                {
//                    if (string.IsNullOrEmpty(sessionsGroup) || group == sessionsGroup)
//                    {
//                        result.AddRange(_sessionsGroups[group].Values);
//                    }
//                }
//            }
//            return result;
//        }

//        public IEnumerable<DataSessionInfo> GetGroupSessions(string groupName)
//        {
//            lock (this)
//            {
//                if (_sessionsGroups.ContainsKey(groupName))
//                {
//                    return _sessionsGroups[groupName].Keys;
//                }
//            }
//            SystemMonitor.Warning("Get sessions of a group not present.");
//            return new DataSessionInfo[] { };
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public RuntimeDataSessionInformation TryMapSessionInfo(DataSessionInfo inputSessionInfo)
//        {
//            lock (this)
//            {
//                if (_sessionsGroups.ContainsKey(inputSessionInfo.Symbol.Group))
//                {
//                    foreach (RuntimeDataSessionInformation session in _sessionsGroups[inputSessionInfo.Symbol.Group].Values)
//                    {
//                        if (DataSessionInfo.CheckSessionsLooseMatch(session.Info, inputSessionInfo))
//                        {
//                            return session;
//                        }
//                    }
//                }
//            }

//            return null;
//        }

//        public bool IsSessionAdded(DataSessionInfo session)
//        {
//            lock (this)
//            {
//                return (_sessionsGroups.ContainsKey(session.Symbol.Group)
//                    && _sessionsGroups[session.Symbol.Group].ContainsKey(session));
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public List<TransportInfo> GetSessionSubscribers(RuntimeDataSessionInformation sessionInformation)
//        {
//            if (sessionInformation.Tag == null)
//            {
//                sessionInformation.Tag = new List<TransportInfo>();
//            }

//            return (List<TransportInfo>)sessionInformation.Tag;
//        }

//        public List<TransportInfo> GetSessionSubscribers(DataSessionInfo session)
//        {
//            lock (this)
//            {
//                if (_sessionsGroups.ContainsKey(session.Symbol.Group) == false
//                || _sessionsGroups[session.Symbol.Group].ContainsKey(session) == false)
//                {
//                    return null;
//                }
            
//                RuntimeDataSessionInformation sessionInformation = _sessionsGroups[session.Symbol.Group][session];
//                return GetSessionSubscribers(sessionInformation);
//            }
//        }
//    }
//}

