using System;
using System.Runtime.Serialization;
using CommonSupport;
using ForexPlatform;
using Arbiter;
using CommonFinancial;
using System.Collections.Generic;
using System.Threading;
using FXCore;

namespace FXCMAdapter
{
    /// <summary>
    /// Class provides integration to the FXCM Order2Go API.
    /// Based on contributions by "m".
    /// </summary>
    [Serializable]
	[UserFriendlyName("FXCM Integration Adapter")]
    public class FXCMAdapter : IntegrationAdapter, DataSourceStub.IImplementation
    {
        internal ArbiterClientId? DataSourceId
        {
            get 
            {
                if (_dataSourceStub != null)
                {
                    return base._dataSourceStub.SubscriptionClientID;
                }

                return null;
            }
        }

        internal OrderExecutionSourceStub OrderExecutionSourceStub
        {
            get
            {
                return base._orderExecutionStub;
            }
        }

        Dictionary<string, Symbol> _usedSymbols = new Dictionary<string, Symbol>();

		#region Member Variables
		Dictionary<string, Symbol> _subscribedSymbols;

        // This makes sure we only ever start once.
		static volatile FXCMConnectionManager _manager = null;

		TableAut _offersTable;

        volatile string _username = "FXR751707001";
        /// <summary>
        /// 
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        volatile string _password = "410";
        /// <summary>
        /// 
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

		volatile string _serviceUrl = "http://www.fxcorporate.com/Hosts.jsp";

        public string ServiceUrl
        {
			get { return _serviceUrl; }
			set { _serviceUrl = value; }
        }

		volatile string _accountType = "Demo";

		public string AccountType
        {
            get { return _accountType; }
			set { _accountType = value; }
        }
        
        public static List<string> ForexSymbols = new List<string>(new string[] 
								{
									"EUR/USD",
									"USD/JPY",
									"GBP/USD",
									"USD/CHF",
									"EUR/CHF",
									"AUD/USD",
									"USD/CAD",
									"NZD/USD",
									"EUR/GBP",
									"EUR/JPY",
									"GBP/JPY",
									"GBP/CHF",
								});

        static List<string> _nativePeriods = new List<string>(new string[]
                                {
									"t1",
									"m1",
									"H1",
									"D1",
								 });

        volatile int _defaultLotSize = 10000;
        /// <summary>
        /// 
        /// </summary>
        public int DefaultLotSize
        {
            get { return _defaultLotSize; }
            set { _defaultLotSize = value; }
        }

        static TimeSpan[] DefaultAvailablePeriods = new TimeSpan[] {
                                                                TimeSpan.FromDays(31),
                                                                TimeSpan.FromDays(7),
                                                                TimeSpan.FromDays(1),
                                                                TimeSpan.FromHours(1),
                                                                TimeSpan.FromMinutes(1),
																TimeSpan.FromTicks(1),
														};

        #endregion

        #region Construction and Instance Control
        /// <summary>
        /// Constructor.
        /// </summary>
		public FXCMAdapter()
        {
            DataSourceStub dataSourceStub = new DataSourceStub("FXCM Adapter Data", true);
            OrderExecutionSourceStub orderExecutionSourceStub = new OrderExecutionSourceStub("FXCM Adapter Execution", true);

            base.SetInitialParameters(dataSourceStub, orderExecutionSourceStub);
            Construct();
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
		public FXCMAdapter(SerializationInfo info, StreamingContext context)
            : base(info, context)
		{
			_username = info.GetString("username");
			_password = info.GetString("password");
			_serviceUrl = info.GetString("serviceUrl");
			_accountType = info.GetString("accountType");

            Construct();
        }

        bool Construct()
        {
            if (_manager != null)
            {
                return false;
            }

			StatusSynchronizationEnabled = true;
			_subscribedSymbols = new Dictionary<string, Symbol>();

            lock (this)
            {
				_manager = new FXCMConnectionManager(this);

                _dataSourceStub.Initialize(this);
                _orderExecutionStub.Initialize(_manager.Orders);

				foreach (string symbol in ForexSymbols)
				{
					_dataSourceStub.AddSuggestedSymbol(new Symbol("Forex", symbol));
				}
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("username", _username);
			info.AddValue("password", _password);
			info.AddValue("serviceUrl", _serviceUrl);
			info.AddValue("accountType", _accountType);

            base.GetObjectData(info, context);
        }

        #endregion

		void PostQuoteUpdate(string forexPair, DataTick dataTick)
		{
			RuntimeDataSessionInformation sessionInformation = _dataSourceStub.GetSymbolSessionInformation(new Symbol("Forex", forexPair));

			if (sessionInformation != null &&
				_subscribedSymbols.ContainsKey(forexPair))
			{
				CombinedDataSubscriptionInformation info = _dataSourceStub.GetUnsafeSessionSubscriptions(sessionInformation.Info);

				if (info != null && info.GetCombinedDataSubscription().QuoteSubscription)
				{
					_dataSourceStub.UpdateQuote(sessionInformation.Info, new Quote(dataTick.Ask, dataTick.Bid, null, dataTick.DateTime), null);
					foreach (TimeSpan supportedPeriod in DefaultAvailablePeriods)
					{
						_dataSourceStub.UpdateDataHistory(sessionInformation.Info, new DataHistoryUpdate(supportedPeriod, new DataTick[] { dataTick }));
					}
				}
			}
		}

		public void tdSink_ITradeDeskEvents_Event_OnRowChanged(object tableDisp, string rowID)
		{
			if (_manager.LoggedIn)
			{
				try
				{
					FXCore.ITableAut t = (FXCore.ITableAut)tableDisp;

					if ("offers".Equals(t.Type))
					{
						DataTick dataTick = new DataTick();
						TableAut offersTable = (TableAut)_manager.Desk.FindMainTable("offers");

						RowAut instrumentRow = (RowAut)offersTable.FindRow("OfferID", rowID, 0);
						dataTick.Ask = (decimal)((double)instrumentRow.CellValue("Ask"));
						dataTick.Bid = (decimal)((double)instrumentRow.CellValue("Bid"));
                        dataTick.DateTime = (DateTime)instrumentRow.CellValue("Time");

						PostQuoteUpdate((string)instrumentRow.CellValue("Instrument"), dataTick);
                        
                        // TODO: this may be way too often, since it will update on each tick of any symbol...
                        // Also update the accounts informations.
                        foreach (AccountInfo accountInfo in _manager.Orders.GetAvailableAccounts())
                        {
                            _orderExecutionStub.UpdateAccountInfo(accountInfo);
                        }
					}
				}
				catch (System.Exception ex)
				{
					SystemMonitor.Error(ex.ToString());
				}
			}
		}

        #region Implementation

        protected override bool OnStart(out string operationResultMessage)
        {
            bool constructResult = Construct();

            FXCMConnectionManager manager = _manager;
            if (manager == null)
            {
                operationResultMessage = "Manager not created.";
                return false;
            }

            base.StatusSynchronizationSource = manager;

            lock (this)
            {
                if (manager.OperationalState != OperationalStateEnum.Initialized
                    && manager.OperationalState != OperationalStateEnum.Constructed)
                {
                    operationResultMessage = "The MBTrading Adapter can only be started once each application session. Restart the Open Forex Platform to start it again.";
                    return false;
                }

                ChangeOperationalState(OperationalStateEnum.Initializing);
            }

            if (manager.Login(Username, Password, _serviceUrl, _accountType) == false)
            {
                operationResultMessage = "Failed to log in to FXCM.";
                SystemMonitor.OperationError(operationResultMessage);
                return false;
            }
            else
            {
                operationResultMessage = string.Empty;

                StartSources();
                return true;
            }
        }

        protected void DisposeManager()
        {
            if (_manager != null)
            {
                _manager.Logout();
                _manager.Dispose();
                _manager = null;
            }
        }

        protected override bool OnStop(out string operationResultMessage)
        {
            DisposeManager();

            base.StatusSynchronizationSource = null;

            operationResultMessage = string.Empty;

            StatusSynchronizationEnabled = false;

            ChangeOperationalState(OperationalStateEnum.NotOperational);

            return true;
        }

        public override bool ArbiterInitialize(Arbiter.Arbiter arbiter)
        {
            return base.ArbiterInitialize(arbiter);
        }

        public override bool ArbiterUnInitialize()
        {
            DisposeManager();

            return base.ArbiterUnInitialize();
		}

        #endregion

        #region Implementation Members

        public DataHistoryUpdate GetDataHistoryUpdate(DataSessionInfo session, DataHistoryRequest request)
		{
			if (!IsPeriodSupported(request.Period))
			{
				SystemMonitor.OperationWarning("Source queried for historic information of wrong period.");
				return null;
			}

			List<DataBar> bars = GetSymbolData(session.Symbol, request.Period);
			if (bars != null)
			{
				return new DataHistoryUpdate(request.Period, bars);
			}

			return null;
		}

		private List<DataBar> GetSymbolData(Symbol symbol, TimeSpan period)
		{
			DateTime _startDate = DateTime.Today.AddMonths(-6);
			DateTime _endDate = DateTime.Now.AddDays(1);

			List<DataBar> resultingData = null;

			try
			{
				resultingData = GetHistory(symbol.Name, period, _startDate, _endDate);
			}
			catch
			{
				SystemMonitor.OperationError("Failed to retrieve stock quotes data [" + symbol + ", " + "]");
			}

			return resultingData;
		}

        public Quote? GetQuoteUpdate(DataSessionInfo session)
		{
			return new Quote(new decimal((double)GetInstrumentData(session.Symbol.Name, "Ask")),
							new decimal((double)GetInstrumentData(session.Symbol.Name, "Bid")), null,
							(DateTime)GetInstrumentData(session.Symbol.Name, "Time"));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<Symbol, TimeSpan[]> SearchSymbols(string symbolMatch, int resultLimit)
        {
			Dictionary<Symbol, TimeSpan[]> result = new Dictionary<Symbol, TimeSpan[]>();
			foreach (Symbol symbol in _dataSourceStub.SearchSuggestedSymbols(symbolMatch, resultLimit))
			{
				result.Add(symbol, DefaultAvailablePeriods);
			}

			return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public RuntimeDataSessionInformation GetSymbolSessionRuntimeInformation(Symbol inputSymbol)
        {
			RuntimeDataSessionInformation information = _dataSourceStub.GetSymbolSessionInformation(inputSymbol);
			if (information == null)
			{
				return new RuntimeDataSessionInformation(new DataSessionInfo(Guid.NewGuid(), "Historical Data " + inputSymbol.Name, inputSymbol, 1000, (int)GetInstrumentData(inputSymbol.Name, "Digits")), DefaultAvailablePeriods);
			}
			else
			{
				return information;
			}
        }

        /// <summary>
        /// 
        /// </summary>
        public void SessionDataSubscriptionUpdate(DataSessionInfo session, bool subscribe, DataSubscriptionInfo? info)
        {
            SystemMonitor.CheckError(session.IsEmtpy == false, "Method needs valid session info assigned to operate.");

            DataSourceStub dataSourceStub = _dataSourceStub;
            if (dataSourceStub == null)
            {
                return;
            }

            CombinedDataSubscriptionInformation combined = dataSourceStub.GetUnsafeSessionSubscriptions(session);
            if (combined == null)
            {
                SystemMonitor.OperationError("Update on not existing session.");
                return;
            }

            DataSubscriptionInfo combinedInfo = combined.GetCombinedDataSubscription();
			if (combinedInfo.QuoteSubscription)
			{
				if (!_subscribedSymbols.ContainsKey(session.Symbol.Name))
				{
					_subscribedSymbols.Add(session.Symbol.Name, session.Symbol);
				}
			}
			else
			{
				if (_subscribedSymbols.ContainsKey(session.Symbol.Name))
				{
					_subscribedSymbols.Remove(session.Symbol.Name);
				}
			}
        }

        #endregion

		public bool IsPeriodSupported(TimeSpan period)
		{
			bool isSupported = false;

			if (period.Days == 1)
			{
				isSupported = true;
			}
			else if (period.Hours == 1)
			{
				isSupported = true;
			}
			else if (period.Minutes == 1)
			{
				isSupported = true;
			}
			else if (period.Ticks == 1)
			{
				isSupported = true;
			}

			return isSupported;
		}

		public string GetPeriodId(TimeSpan period)
		{
			string sPeriodId = "t1";

			if (period.Days == 1)
			{
				sPeriodId = "D1";
			}
			else if (period.Hours == 1)
			{
				sPeriodId = "H1";
			}
			else if (period.Minutes == 1)
			{
				sPeriodId = "m1";
			}
			else if (period.Ticks == 1)
			{
				sPeriodId = "t1";
			}

			return sPeriodId;
		}

		public List<DataBar> GetHistory(string forexPair, TimeSpan period, DateTime lowerBound, DateTime upperBound)
		{
			bool keepIterating = true;
			FXCore.IMarketRateEnumAut japaneseCandlestick;
			List<DataBar> candlestickHistoryList = new List<DataBar>();

			candlestickHistoryList = new List<DataBar>();
			lock (this)
			{
				while (keepIterating)
				{
					japaneseCandlestick = (FXCore.IMarketRateEnumAut)_manager.Desk.GetPriceHistory(forexPair, GetPeriodId(period), lowerBound, upperBound, -1, false, true);


					foreach (FXCore.IMarketRateAut marketRate in japaneseCandlestick)
					{
						DataBar dataBar = new DataBar(marketRate.StartDate, new decimal(marketRate.AskOpen), new decimal(marketRate.AskHigh), new decimal(marketRate.AskLow), new decimal(marketRate.AskClose), 0);

						candlestickHistoryList.Add(dataBar);

						lowerBound = marketRate.StartDate;
					}

					Thread.Sleep(100);

					int i = japaneseCandlestick.Size;

					keepIterating = i > 1 && upperBound.CompareTo(lowerBound) >= 0;

					lowerBound.AddTicks(1);
				}
			}

			return candlestickHistoryList;
		}

		public object GetInstrumentData(string forexPair, string columnName)
		{
			object columnData = null;

			lock (this)
			{
				_offersTable = (FXCore.TableAut)_manager.Desk.FindMainTable("offers");

				switch (forexPair)
				{
					case "EUR/USD":
						columnData = _offersTable.CellValue(1, columnName);
						break;
					case "USD/JPY":
						columnData = _offersTable.CellValue(2, columnName);
						break;
					case "GBP/USD":
						columnData = _offersTable.CellValue(3, columnName);
						break;
					case "USD/CHF":
						columnData = _offersTable.CellValue(4, columnName);
						break;
					case "EUR/CHF":
						columnData = _offersTable.CellValue(5, columnName);
						break;
					case "AUD/USD":
						columnData = _offersTable.CellValue(6, columnName);
						break;
					case "USD/CAD":
						columnData = _offersTable.CellValue(7, columnName);
						break;
					case "NZD/USD":
						columnData = _offersTable.CellValue(8, columnName);
						break;
					case "EUR/GBP":
						columnData = _offersTable.CellValue(9, columnName);
						break;
					case "EUR/JPY":
						columnData = _offersTable.CellValue(10, columnName);
						break;
					case "GBP/JPY":
						columnData = _offersTable.CellValue(11, columnName);
						break;
					case "GBP/CHF":
						columnData = _offersTable.CellValue(12, columnName);
						break;
				}
			}

			return columnData;
		}
    }
}
