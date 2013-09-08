using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Arbiter;
using CommonFinancial;
using CommonSupport;
using ForexPlatform;

namespace MBTradingAdapter
{
    /// <summary>
    /// Class provides integration to MBTrading API.
    /// </summary>
    [Serializable]
    [UserFriendlyName("MBTrading Integration Adapter")]
    public class MBTradingAdapter : IntegrationAdapter, DataSourceStub.IImplementation
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

        // This makes sure we only ever start once.
        static volatile MBTradingConnectionManager _manager = null;

        volatile string _username = "DEMOYOVR";
        /// <summary>
        /// 
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        volatile string _password = "1jeep2desk";
        /// <summary>
        /// TODO: move to secure storage module.
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        static string[] EquitiesSymbols = new string[] 
        {
            "MSFT",
            "GOOG",
            "CSCO",
            "COHR",
            "DELL",
            "DRYS",
            "EBAY",
            "GOOG",
            "INTC",
            "MSFT",
            "PDLI",
            "RIMM",
            "YHOO",
            "ESPZF",
            "CPEU",
            "AA",
            "AXP",
            "BAC",
            "C",
            "DAL",
            "DIS",
            "FAS",
            "GE",
            "IBM",
            "LLC",
            "MMM",
            "MO",
            "MRK",
            "T",
            "XLF",
        };
        
        static string[] ForexSymbols = new string[] 
        {
            "AUD/JPY",
            "AUD/NZD",
            "AUD/USD",
            "CAD/JPY",
            "CAD/CHF",
            "CHF/JPY",
            "EUR/AUD",
            "EUR/CAD",
            "EUR/CHF",
            "EUR/GBP",
            "EUR/JPY",
            "EUR/NOK",
            "EUR/SEK",
            "EUR/USD",
            "GBP/AUD",
            "GBP/CAD",
            "GBP/CHF",
            "GBP/JPY",
            "GBP/USD",
            "NZD/JPY",
            "NZD/USD",
            "USD/CAD",
            "USD/CHF",
            "USD/DKK",
            "USD/JPY",
            "USD/NOK",
            "USD/SEK"
        };

        volatile int _defaultLotSize = 10000;
        /// <summary>
        /// 
        /// </summary>
        public int DefaultLotSize
        {
            get { return _defaultLotSize; }
            set { _defaultLotSize = value; }
        }

        static TimeSpan[] DefaultAvailablePeriods = new TimeSpan[] { TimeSpan.FromDays(365),
                                                                TimeSpan.FromDays(31),
                                                                TimeSpan.FromDays(7),
                                                                TimeSpan.FromDays(1),
                                                                TimeSpan.FromHours(12),
                                                                TimeSpan.FromHours(4),
                                                                TimeSpan.FromHours(1),
                                                                TimeSpan.FromMinutes(30),
                                                                TimeSpan.FromMinutes(15),
                                                                TimeSpan.FromMinutes(5),
                                                                TimeSpan.FromMinutes(1) };

        #endregion

        #region Construction and Instance Control
        /// <summary>
        /// Constructor.
        /// </summary>
        public MBTradingAdapter()
        {
            DataSourceStub dataSourceStub = new DataSourceStub("MBTrading Adapter Data", true);
            OrderExecutionSourceStub orderExecutionSourceStub = new OrderExecutionSourceStub("MBTrading Adapter Execution", false);

            base.SetInitialParameters(dataSourceStub, orderExecutionSourceStub);

            Construct();
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        public MBTradingAdapter(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _username = info.GetString("username");
            _password = info.GetString("password");

            Construct();
        }

        bool Construct()
        {
            if (_manager != null)
            {
                return false;
            }

            StatusSynchronizationEnabled = true;

            lock (this)
            {
                _manager = new MBTradingConnectionManager(this);
                if (_manager.Quotes != null)
                {
                    _manager.Quotes.QuoteUpdateEvent += new MBTradingQuote.QuoteUpdateDelegate(Quotes_QuoteUpdateEvent);
                }

                _dataSourceStub.Initialize(this);
                _orderExecutionStub.Initialize(_manager.Orders);

                foreach (string symbol in EquitiesSymbols)
                {
                    _dataSourceStub.AddSuggestedSymbol(new Symbol(string.Empty, symbol));
                }

                foreach (string symbol in ForexSymbols)
                {
                    _dataSourceStub.AddSuggestedSymbol(new Symbol("FX", symbol));
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

            base.GetObjectData(info, context);
        }

        #endregion

        #region Implementation

        protected override bool OnStart(out string operationResultMessage)
        {
            bool constructResult = Construct();

            MBTradingConnectionManager manager = _manager;
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

            if (manager.Login(Username, Password) == false)
            {
                operationResultMessage = "Failed to log in to MBT.";
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createNewDefault">Should a new default one (empty group) be created and returned (also a warning is posted).</param>
        public Symbol? GetSymbolByName(string name, bool beginSymbolDelivery)
        {
            lock (this)
            {
                if (_usedSymbols.ContainsKey(name))
                {
                    return _usedSymbols[name];
                }
            }

            if (beginSymbolDelivery)
            {
                GeneralHelper.FireAndForget(delegate()
                {// Make sure to do this in another pass, since otherwise we might block.
                    EstablishSymbolUsage(new Symbol(name), TimeSpan.FromSeconds(8));
                });
            }

            return null;
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

        void Quotes_QuoteUpdateEvent(MBTradingQuote keeper, MBTradingQuote.SessionQuoteInformation information)
        {
            RuntimeDataSessionInformation session = _dataSourceStub.GetSymbolSessionInformation(information.Symbol);
            if (session != null)
            {
                _dataSourceStub.UpdateQuote(session.Info, information.Quote, null);
            }
            else
            {
                SystemMonitor.OperationWarning("Symbol session not found, quote missed.");
            }
        }

        #endregion

        #region Implementation Members

        public DataHistoryUpdate GetDataHistoryUpdate(DataSessionInfo session, DataHistoryRequest request)
        {
            MBTradingConnectionManager manager = _manager;
            if (manager != null)
            {
                DataHistoryOperation operation = new DataHistoryOperation(session.Symbol.Name, request);
                if (manager.History.Place(operation) == false)
                {
                    SystemMonitor.OperationError("Failed to place data history operation.");
                    return null;
                }

                if (operation.CompletionEvent.WaitOne(TimeSpan.FromSeconds(120)) == false)
                {
                    SystemMonitor.OperationError("Data history operation timed out.");
                    return null;
                }

                return operation.Responce;
            }

            return null;
        }

        public Quote? GetQuoteUpdate(DataSessionInfo session)
        {
            MBTradingConnectionManager manager = _manager;
            if (manager != null)
            {
                MBTradingQuote.SessionQuoteInformation info = manager.Quotes.GetSymbolSessionInformation(session.Symbol.Name);
                if (info != null)
                {
                    return info.Quote;
                }
            }

            return null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<Symbol, TimeSpan[]> SearchSymbols(string symbolMatch, int resultLimit)
        {
            Dictionary<Symbol, TimeSpan[]> result = new Dictionary<Symbol,TimeSpan[]>();
            
            foreach (Symbol symbol in _dataSourceStub.SearchSuggestedSymbols(symbolMatch, resultLimit))
            {
                result.Add(symbol, DefaultAvailablePeriods);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public Symbol? EstablishSymbolUsage(Symbol inputSymbol, TimeSpan timeOut)
        {
            MBTradingConnectionManager manager = _manager;
            if (manager == null || manager.Quotes == null)
            {
                return null;
            }

            // Filter the baseCurrency trough its name and what symbols we know of.
            Symbol? knownSymbol = _dataSourceStub.MapSymbolToRunningSession(inputSymbol.Name);
            if (knownSymbol.HasValue == false)
            {
                if (_dataSourceStub.IsSuggestedSymbol(inputSymbol) 
                    && string.IsNullOrEmpty(inputSymbol.Group) == false)
                {
                    knownSymbol = inputSymbol;
                }
                else
                {// Upper case input baseCurrency.
                    inputSymbol = new Symbol(inputSymbol.Group, inputSymbol.Name.ToUpper());

                    string symbolMarket;
                    if (manager.Quotes.GetSingleSymbolQuote(inputSymbol.Name, timeOut, out symbolMarket).HasValue)
                    {// BaseCurrency found and established from broker.
                        knownSymbol = new Symbol(symbolMarket, inputSymbol.Name);
                    }
                    else
                    {
                        SystemMonitor.OperationError("Failed to map symbol with this name.");
                        return null;
                    }
                }
            }

            lock (this)
            {
                if (_usedSymbols.ContainsKey(knownSymbol.Value.Name) == false)
                {
                    _usedSymbols.Add(knownSymbol.Value.Name, knownSymbol.Value);
                }
            }

            return knownSymbol;
        }

        /// <summary>
        /// 
        /// </summary>
        public RuntimeDataSessionInformation GetSymbolSessionRuntimeInformation(Symbol inputSymbol)
        {
            // Filter the baseCurrency trough its name and what symbols we know of.
            Symbol? knownSymbol = EstablishSymbolUsage(inputSymbol, TimeSpan.FromSeconds(6));
            if (knownSymbol.HasValue == false)
            {// Failed to start / establish symbol usage.
                return null;
            }

            DataSessionInfo sessionInfo = new DataSessionInfo(Guid.NewGuid(), knownSymbol.Value.Name,
                knownSymbol.Value, DefaultLotSize, 5);

            RuntimeDataSessionInformation session = new RuntimeDataSessionInformation(sessionInfo);
            session.AvailableDataBarPeriods.AddRange(DefaultAvailablePeriods);
            return session;
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

            MBTradingConnectionManager manager = _manager;
            if (manager == null)
            {
                return;
            }

            DataSubscriptionInfo combinedInfo = combined.GetCombinedDataSubscription();
            if (combinedInfo.QuoteSubscription)
            {
                manager.Quotes.SubscribeSymbolSession(session.Symbol);
                GeneralHelper.FireAndForget(delegate()
                {// Run an async update of quotes.
                    string market;
                    Quote? quote = _manager.Quotes.GetSingleSymbolQuote(session.Symbol.Name, TimeSpan.FromSeconds(10), out market);
                    if (quote.HasValue)
                    {
                        _dataSourceStub.UpdateQuote(session, quote, null);
                    }
                });
            }
            else
            {
                manager.Quotes.UnSubscribeSymbolSession(session.Symbol.Name);
            }
        }

        #endregion

    }
}
