using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using Arbiter;
using CommonSupport;

namespace ForexPlatform
{
    /// <summary>
    /// Base class for integration adapters, allowing dataDelivery to be imported
    /// and order execution to be exported from the platform.
    /// </summary>
    [Serializable]
    public abstract class IntegrationAdapter : OperationalTransportClient, IIntegrationAdapter
    {
        /// <summary>
        /// What is the advised precision for account values calculations (profit, margin etc.)
        /// </summary>
        public const int AdvisedAccountDecimalsPrecision = 2;

        volatile bool _isStarted = false;
        /// <summary>
        /// The adapter manager controls starting and stopping of adapters.
        /// </summary>
        public bool IsStarted
        {
            get { return _isStarted; }
        }

        protected volatile DataSourceStub _dataSourceStub = null;
        protected volatile OrderExecutionSourceStub _orderExecutionStub = null;

        public event IntegrationAdapterUpdateDelegate PersistenceDataUpdateEvent;

        /// <summary>
        /// Constructor.
        /// </summary>
        public IntegrationAdapter()
            : base("Integration Adapter", false)
        {
            this.Name = UserFriendlyNameAttribute.GetTypeAttributeName(this.GetType());

            base.DefaultTimeOut = TimeSpan.FromSeconds(20);
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        public IntegrationAdapter(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _dataSourceStub = (DataSourceStub)info.GetValue("dataSourceStub", typeof(DataSourceStub));
            _orderExecutionStub = (OrderExecutionSourceStub)info.GetValue("orderSourceStub", typeof(OrderExecutionSourceStub));
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SetInitialParameters(DataSourceStub dataSourceStub, OrderExecutionSourceStub orderExecutionStub)
        {
            if (_dataSourceStub != null || _orderExecutionStub != null)
            {
                return false;
            }

            _dataSourceStub = dataSourceStub;
            _orderExecutionStub = orderExecutionStub;

            return true;
        }

        public override bool ArbiterInitialize(Arbiter.Arbiter arbiter)
        {
            bool result = base.ArbiterInitialize(arbiter);
            
            // Make sure to add sources as soon as possible, since there might be some requests coming in for them.
            InitializeSources();

            return result;
        }

        public override bool ArbiterUnInitialize()
        {
            InitializeSources();

            return base.ArbiterUnInitialize();
        }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            lock (this)
            {
                info.AddValue("dataSourceStub", _dataSourceStub);
                info.AddValue("orderSourceStub", _orderExecutionStub);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void RaisePersistenceDataUpdateEvent()
        {
            if (PersistenceDataUpdateEvent != null)
            {
                PersistenceDataUpdateEvent(this);
            }
        }

        protected bool InitializeSources()
        {
            if (Arbiter != null && _dataSourceStub != null)
            {
                Arbiter.AddClient(_dataSourceStub);
            }

            if (Arbiter != null && _orderExecutionStub != null)
            {
                Arbiter.AddClient(_orderExecutionStub);
            }

            SystemMonitor.CheckError(Arbiter != null, "Arbiter must be assigned to start sources.");

            return true;
        }

        /// <summary>
        /// Helper, removes sources.
        /// </summary>
        /// <returns></returns>
        protected bool UnInitializeSources()
        {
            if (Arbiter != null && _dataSourceStub != null)
            {
                Arbiter.RemoveClient(_dataSourceStub);
            }

            if (Arbiter != null && _orderExecutionStub != null)
            {
                Arbiter.RemoveClient(_orderExecutionStub);
            }

            SystemMonitor.CheckWarning(Arbiter != null, "Arbiter not assigned.");

            RaisePersistenceDataUpdateEvent();
            return true;
        }

        /// <summary>
        /// Helper, start sources.
        /// </summary>
        protected void StartSources()
        {
            if (_dataSourceStub != null)
            {
                _dataSourceStub.Start();
            }

            if (_orderExecutionStub != null)
            {
                _orderExecutionStub.Start();
            }

        }

        /// <summary>
        /// Helper, stop dataDelivery and order sources.
        /// </summary>
        protected void StopSources()
        {
            if (_dataSourceStub != null)
            {
                _dataSourceStub.Stop();
            }

            if (_orderExecutionStub != null)
            {
                _orderExecutionStub.Stop();
            }
        }


        /// <summary>
        /// Manager requires adapter to start.
        /// </summary>
        public bool Start(out string operationResultMessage)
        {
            if (IsStarted)
            {
                operationResultMessage = "Adapter already started.";
                return false;
            }

            _isStarted = true;
            if (OnStart(out operationResultMessage))
            {
                return true;
            }
            else
            {
                StopSources();
                //UnInitializeSources();
            }

            _isStarted = false;
            return false;
        }

        /// <summary>
        /// Called when the adapter is called to start.
        /// Try not to put blocking calls here, since this may be executed on the UI thread.
        /// </summary>
        protected abstract bool OnStart(out string operationResultMessage);

        /// <summary>
        /// Manager requested adapter to stop.
        /// </summary>
        public bool Stop(out string operationResultMessage)
        {
            StopSources();

            if (_isStarted)
            {
                _isStarted = false;
                bool result = OnStop(out operationResultMessage);
                //UnInitializeSources();
                return result;
            }
            else
            {
                operationResultMessage = "Adapter not started.";
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract bool OnStop(out string operationResultMessage);

        //#region Arbiter Messages Sent To Subscribers/SourceContainer

        //protected virtual void SendUpdateResponce(OperationResponceMessage requestMessage, bool toDataSource, bool toOrderSource)
        //{
        //    // Make sure initialization has already passed successfully.
        //    if (_initializationEvent.WaitOne(DefaultTimeOut, true) == false)
        //    {// Time out.
        //        SystemMonitor.OperationError("Time out occured.", SystemMonitor.TracerItem.PriorityEnum.Medium);
        //        return;
        //    }

        //    ArbiterClientId? dataSourceId = _dataSourceId;
        //    ArbiterClientId? orderExecutionSourceId = _orderExecutionSourceId;

        //    if (toDataSource && dataSourceId.HasValue)
        //    {
        //        if (ParentOperator.Platform != null &&
        //            ParentOperator.Platform.GetComponentOperationalState(dataSourceId.Value.Id) != OperationalStateEnum.Operational)
        //        {
        //            return;
        //        }

        //        if (requestMessage.OperationId == -1)
        //        {// Send as a general update for everyone.
        //            this.SendAddressed(dataSourceId.Value, requestMessage);
        //        }
        //        else
        //        {// Send as a respond to whoever requested it.
        //            SystemMonitor.NotImplementedCritical();
        //            //Receive(requestMessage);
        //        }
        //    }

        //    if (toOrderSource && orderExecutionSourceId.HasValue)
        //    {
        //        if (ParentOperator.Platform != null &&
        //            ParentOperator.Platform.GetComponentOperationalState(orderExecutionSourceId.Value.Id) != OperationalStateEnum.Operational)
        //        {
        //            return;
        //        }

        //        if (requestMessage.OperationId == -1)
        //        {// Send as a general update for everyone.
        //            this.SendAddressed(orderExecutionSourceId.Value, requestMessage);
        //        }
        //        else
        //        {// Send as a respond to whoever requested it.
        //            SystemMonitor.NotImplementedCritical();
        //            //Receive(requestMessage);
        //        }
        //    }
        //}

        public void Dispose()
        {
        }
    }
}
