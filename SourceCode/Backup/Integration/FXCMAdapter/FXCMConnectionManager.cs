using System;
using CommonSupport;
using FXCore;

namespace FXCMAdapter
{
    /// <summary>
    /// Class constrols integration to the Fxcm Order2Go API.
    /// 
    /// Works in a separate thread, since COM EVENTS coming from the API
    /// require a requestMessage loop and the same thread to access them, it takes
    /// care of all this.
    /// </summary>
    public class FXCMConnectionManager : Operational, IDisposable
    {
        /// <summary>
        /// Try to access this only from the internal thread.
        /// </summary>
		volatile CoreAut _core;
		volatile TradeDeskAut _desk;
		TradeDeskEventsSinkClass _tradeDeskEventsSink;

		public TradeDeskAut Desk
		{
			get { return _desk; }
			set { _desk = value; }
		}

        BackgroundMessageLoopOperator _messageLoopOperator;

		volatile FXCMOrders _orders;
        /// <summary>
        /// 
        /// </summary>
		public FXCMOrders Orders
        {
            get { return _orders; }
        }

		FXCMAdapter _adapter;

		public bool LoggedIn
		{
			get { return _desk.IsLoggedIn();; }
		}

		int _subscriptionResponse;

        /// <summary>
        /// Constructor.
        /// </summary>
		public FXCMConnectionManager(FXCMAdapter adapter)
        {
            ChangeOperationalState(OperationalStateEnum.Constructed);

			_subscriptionResponse = -1;
            _adapter = adapter;
            _messageLoopOperator = new BackgroundMessageLoopOperator(false);
			_orders = new FXCMOrders(_messageLoopOperator);

            _messageLoopOperator.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                if (_orders != null)
                {
                    _orders.UnInitialize();
                    _orders.Dispose();
                    _orders = null;
                }

                if (_adapter != null)
                {
                    _adapter = null;
                }

                if (_core != null)
                {
					_desk = null;
                    _core = null;
                }

                if (_messageLoopOperator != null)
                {
                    _messageLoopOperator.Stop();
                    _messageLoopOperator.Dispose();
                    _messageLoopOperator = null;
                }

                ChangeOperationalState(OperationalStateEnum.Disposed);
            }
            
            //GC.Collect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Login(string username, string password, string serviceUrl, string accountType)
        {
            if (OperationalState != OperationalStateEnum.Initialized
                 && OperationalState != OperationalStateEnum.Initializing
                && OperationalState != OperationalStateEnum.Constructed)
            {
                return false;
            }

            object result = false;

            GeneralHelper.GenericReturnDelegate<bool> del = delegate()
            {
                if (_core == null)
                {
                    _core = new FXCore.CoreAutClass();
                    _desk = (FXCore.TradeDeskAut)_core.CreateTradeDesk("trader");
                }

                lock (this)
                {
                    _orders.Initialize(_adapter, this);
                }

                ChangeOperationalState(OperationalStateEnum.Initializing);

                try
                {
                    _desk.Login(username, password, serviceUrl, accountType);
                    Subscribe();
                    ChangeOperationalState(OperationalStateEnum.Operational);
                }
                catch (Exception exception)
                {
                    SystemMonitor.OperationError("Failed to log in [" + exception.Message + "].");
                    ChangeOperationalState(OperationalStateEnum.NotOperational);
                }

                return _desk.IsLoggedIn();
            };

            _messageLoopOperator.Invoke(del, TimeSpan.FromSeconds(180), out result);

            return (bool)result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Logout()
        {
			if (_core != null && _desk.IsLoggedIn())
			{
				Unsubscribe();

				_desk.Logout();

				_desk = null;
				_core = null;
            }

            ChangeOperationalState(OperationalStateEnum.NotOperational);
            return true;
		}

		public void Subscribe()
		{
			_tradeDeskEventsSink = new TradeDeskEventsSinkClass();
			_tradeDeskEventsSink.ITradeDeskEvents_Event_OnRowChanged += new ITradeDeskEvents_OnRowChangedEventHandler(_adapter.tdSink_ITradeDeskEvents_Event_OnRowChanged);
            _tradeDeskEventsSink.ITradeDeskEvents_Event_OnSessionStatusChanged += new ITradeDeskEvents_OnSessionStatusChangedEventHandler(_tradeDeskEventsSink_ITradeDeskEvents_Event_OnSessionStatusChanged);

			_subscriptionResponse = _desk.Subscribe(_tradeDeskEventsSink);

			SystemMonitor.Report("FXCM Service Subscribed");
		}

        void _tradeDeskEventsSink_ITradeDeskEvents_Event_OnSessionStatusChanged(string sStatus)
        {// TODO: handle changes in status of connection to the server.
            //The session can have one of the following statuses:
            //Disconnected
            // The connection to the trade server is not established. Methods of the TradeDeskAut except CheckVersion or Login must not be used.
            //Connecting
            // The connection to the trade server is being established. Methods of the TradeDeskAut must not be used.
            //Connected
            // The connection to the trade server is established and the tables are loaded. All methods of the TradeDeskAut except Login may be used.
            //Reconnecting
            // The connection to the server has been lost and the is being restored. Methods of the TradeDeskAut or subsequent objects must not be used. As you can see in the diagram below, this status can be reached because of connection problem as well as because of server forced re-login. You can use TradeDeskAut.LastError property to distinguish these variant. In case the reconnect is started because of connection problem the property will consist of an empty string. In case the reconnect is forced by the server, the property will consist of the server message.
            //Disconnecting
            // The connection to the trade server is being terminated and all session-related resources is being freed. Methods of the TradeDeskAut or subsequent objects must not be used.
        }

		public void Unsubscribe()
		{
			_tradeDeskEventsSink.ITradeDeskEvents_Event_OnRowChanged -= new ITradeDeskEvents_OnRowChangedEventHandler(_adapter.tdSink_ITradeDeskEvents_Event_OnRowChanged);
            _tradeDeskEventsSink.ITradeDeskEvents_Event_OnSessionStatusChanged -= new ITradeDeskEvents_OnSessionStatusChangedEventHandler(_tradeDeskEventsSink_ITradeDeskEvents_Event_OnSessionStatusChanged);

			if (_subscriptionResponse != -1)
			{
				_desk.Unsubscribe(_subscriptionResponse);
				_tradeDeskEventsSink = null;
			}
		}
    }
}
