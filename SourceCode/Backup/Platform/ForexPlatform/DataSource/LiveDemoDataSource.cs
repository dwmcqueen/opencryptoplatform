using System;
using System.Runtime.Serialization;
using System.Timers;
using Arbiter;
using CommonFinancial;
using CommonSupport;
using System.Collections.Generic;

namespace ForexPlatform
{
    /// <summary>
    /// This class shows how to build a basic live demo source. The source generates random dataDelivery that can be used
    /// by any component trough a dataDelivery provider or directly (by handing messages).
    /// </summary>
    [Serializable]
    [UserFriendlyName("Live Demo Data Source")]
    public class LiveDemoSource : PlatformSource, DataSourceStub.IImplementation
    {
        DateTime _dateTime;

        Timer _liveDataTimer;

        TimeSpan _period = TimeSpan.FromMinutes(15);

        RuntimeDataSessionInformation _sessionInformation;

        List<DataBar> _history = new List<DataBar>();

        DataBar LastBar
        {
            get
            {
                if (_history.Count > 0)
                {
                    return _history[_history.Count - 1];
                }
                return DataBar.Empty;
            }
        }

        DataSourceStub _dataStub;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LiveDemoSource()
        {
            _dataStub = new DataSourceStub(Name, false);
            _sessionInformation = new RuntimeDataSessionInformation(new DataSessionInfo(Guid.NewGuid(), "DEMO150", new Symbol("Unknown", "DEMO10D15"), 10000, 4), _period);
            Construct();
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        public LiveDemoSource(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _sessionInformation = (RuntimeDataSessionInformation)info.GetValue("sessionInformation", typeof(RuntimeDataSessionInformation));
            _dataStub = (DataSourceStub)info.GetValue("dataStub", typeof(DataSourceStub));
            Construct();
        }

        protected void Construct()
        {
            _dataStub.Initialize(this);
            _dataStub.AddSuggestedSymbol(_sessionInformation.Info.Symbol);
            _dataStub.AddSession(_sessionInformation);

            _dateTime = DateTime.Now;

            _liveDataTimer = new Timer(750);
            _liveDataTimer.Elapsed += new System.Timers.ElapsedEventHandler(TimerTimeout);
            _liveDataTimer.AutoReset = false;

            _liveDataTimer.Start();

            for (int i = 0; i < 100; i++)
            {
                GenerateNextRandomBar();
            }
        }

        /// <summary>
        /// Serialization; although the parent class will also persist the sessions, lets to it here too,
        /// for convenience.
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sessionInformation", _sessionInformation);
            info.AddValue("dataStub", _dataStub);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// This is called only one time in the lifetime of the source (just after creation, before very first init). 
        /// It allows it to read dataDelivery from settings, if it needs to.
        /// </summary>
        /// <param name="dataDelivery"></param>
        /// <returns></returns>
        protected override bool OnSetInitialState(PlatformSettings data)
        {
            return true;
        }

        /// <summary>
        /// Source is starting.
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        protected override bool OnInitialize(Platform platform)
        {
            Arbiter.AddClient(_dataStub);

            ChangeOperationalState(OperationalStateEnum.Operational);
            return true;
        }

        protected override bool OnUnInitialize()
        {
            Arbiter.RemoveClient(_dataStub);
            ChangeOperationalState(OperationalStateEnum.NotOperational);
            _liveDataTimer.Stop();
            return base.OnUnInitialize();
        }

        /// <summary>
        /// 
        /// </summary>
        DataBar GenerateNextRandomBar()
        {
            DataBar result;
            if (_history.Count == 0)
            {
                decimal startPrice = 0.9m;
                result = new DataBar() { DateTime = _dateTime, Close = startPrice, Open = startPrice - 0.005m, High = startPrice + 0.005m, Low = startPrice - 0.012m, Volume = 12 };
            }
            else
            {
                _dateTime = _dateTime.Add(_period);

                decimal movement = Math.Round(GeneralHelper.Random(-0.01m, 0.01m), 4);
                decimal open = LastBar.Close;
                decimal close = open + movement;
                decimal high = Math.Max(open, close) + Math.Round(GeneralHelper.Random(0, 0.005m), 4);
                decimal low = Math.Min(open, close) + Math.Round(GeneralHelper.Random(-0.005m, 0), 4);
                decimal volume = GeneralHelper.Random(1, 20);
                result = new DataBar(_dateTime, open, high, low, close, volume);
            }

            _history.Add(result);
            return result;
        }


        /// <summary>
        /// On timer timeout send a newly generated bar to receivers.
        /// </summary>
        public void TimerTimeout(object source, System.Timers.ElapsedEventArgs e)
        {
            GenerateNextRandomBar();

            CombinedDataSubscriptionInformation info = _dataStub.GetUnsafeSessionSubscriptions(_sessionInformation.Info);

            if (info != null && info.GetCombinedDataSubscription().QuoteSubscription)
            {
                _dataStub.UpdateQuote(_sessionInformation.Info, new Quote(LastBar.Open, LastBar.Open - 0.002m, null, DateTime.Now), null);
                _dataStub.UpdateDataHistory(_sessionInformation.Info, new DataHistoryUpdate(_period, new DataBar[] { LastBar }));
            }

            if (OperationalState != OperationalStateEnum.UnInitialized &&
                OperationalState != OperationalStateEnum.Disposed)
            {
                _liveDataTimer.Start();
            }
        }


        #region Implementation Members

        public DataHistoryUpdate GetDataHistoryUpdate(DataSessionInfo session, DataHistoryRequest request)
        {
            if (request.IsTickBased)
            {
                SystemMonitor.NotImplementedWarning();
                return new DataHistoryUpdate(request.Period, new DataTick[] { });
            }

            if (request.Period != _period)
            {
                return new DataHistoryUpdate(request.Period, new DataBar[] { });
            }
            
            lock (this)
            {
                return new DataHistoryUpdate(request.Period, _history.ToArray());
            }
        }

        public Quote? GetQuoteUpdate(DataSessionInfo session)
        {
            return new Quote(LastBar.Open, LastBar.Close, null, null);
        }

        public Dictionary<Symbol, TimeSpan[]> SearchSymbols(string symbolMatch, int resultLimit)
        {
            Dictionary<Symbol, TimeSpan[]> result = new Dictionary<Symbol, TimeSpan[]>();

            lock (this)
            {
                if (_sessionInformation.Info.Symbol.MatchesSearchCriteria(symbolMatch))
                {
                    result.Add(_sessionInformation.Info.Symbol, new TimeSpan[] { _period });
                }
            }

            return result;
        }

        public RuntimeDataSessionInformation GetSymbolSessionRuntimeInformation(Symbol symbol)
        {
            return _dataStub.GetSymbolSessionInformation(symbol);
        }

        public void SessionDataSubscriptionUpdate(DataSessionInfo session, bool subscribe, DataSubscriptionInfo? info)
        {
        }

        #endregion
    }
}

