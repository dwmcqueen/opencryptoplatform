using System;
using System.Collections.Generic;
using System.Text;
using MBTQUOTELib;
using CommonFinancial;
using CommonSupport;

namespace MBTradingAdapter
{
    /// <summary>
    /// 
    /// </summary>
    public class MBTradingQuote : IMbtQuotesNotify, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public class SessionQuoteInformation
        {
            public Symbol Symbol;
            public Quote? Quote;
        }

        volatile MbtQuotes _quotesClient;

        bool IsInitialized
        {
            get { return _quotesClient != null; }
        }

        Dictionary<string, SessionQuoteInformation> _sessions = new Dictionary<string, SessionQuoteInformation>();
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, SessionQuoteInformation> SessionsQuotesUnsafe
        {
            get { return _sessions; }
        }

        BackgroundMessageLoopOperator _messageLoopOperator;

        public delegate void QuoteUpdateDelegate(MBTradingQuote keeper, SessionQuoteInformation information);
        public event QuoteUpdateDelegate QuoteUpdateEvent;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MBTradingQuote(BackgroundMessageLoopOperator messageLoopOperator)
        {// Calls to the COM must be done in the requestMessage loop operator.
            _messageLoopOperator = messageLoopOperator;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_quotesClient != null)
                {
                    _quotesClient.UnadviseAll(this);
                    _quotesClient = null;
                }
            }
            catch (Exception ex)
            {
                SystemMonitor.OperationError(ex.Message);
            }

            _messageLoopOperator = null;
        }

        /// <summary>
        /// 
        /// </summary>
        internal bool Initialize(MbtQuotes quotesClient)
        {
            SystemMonitor.CheckError(_messageLoopOperator.InvokeRequred == false, "Init must better be called on message loop method.");

            if (_quotesClient != null)
            {
                return false;
            }

            _quotesClient = quotesClient;
            return true;
        }

        /// <summary>
        /// Call is non confirmative;
        /// Forwarded to requestMessage looped thread.
        /// </summary>
        /// <param name="baseCurrency"></param>
        public bool SubscribeSymbolSession(Symbol symbol)
        {
            if (IsInitialized == false)
            {
                return false;
            }

            lock (this)
            {
                if (_sessions.ContainsKey(symbol.Name))
                {// BaseCurrency already subscribed.
                    return true;
                }

                _sessions.Add(symbol.Name, new SessionQuoteInformation() { Symbol = symbol });
            }
            
            MbtQuotes client = _quotesClient;
            if (client != null)
            {
                _messageLoopOperator.BeginInvoke(delegate()
                {
                    client.AdviseSymbol(this, symbol.Name, (int)enumQuoteServiceFlags.qsfLevelOne);
                });
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UnSubscribeSymbolSession(string symbol)
        {
            if (IsInitialized == false)
            {
                return false;
            }

            lock (this)
            {
                if (_sessions.Remove(symbol) == false)
                {
                    return false;
                }
            }

            bool result = true;
            _messageLoopOperator.Invoke(delegate()
            {
                MbtQuotes client = _quotesClient;
                if (client == null)
                {
                    result = false;
                    return;
                }

                client.UnadviseSymbol(this, symbol, (int)enumQuoteServiceFlags.qsfLevelOne);
            });

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public SessionQuoteInformation GetSymbolSessionInformation(string symbol)
        {
            if (IsInitialized == false)
            {
                return null;
            }

            lock(this)
            {
                if (_sessions.ContainsKey(symbol))
                {
                    return _sessions[symbol];
                }
            }

            return null;
        }

        public Quote? GetSingleSymbolQuote(string symbolName, TimeSpan timeOut, out string symbolMarket)
        {
            symbolMarket = string.Empty;
            if (IsInitialized == false)
            {
                return null;
            }

            string market = string.Empty;
            Quote? result = null;
            
            _messageLoopOperator.Invoke(delegate()
            {
                MbtQuotes client = _quotesClient;
                if (client == null)
                {
                    return;
                }

                QUOTERECORD record = client.GetSingleQuote(symbolName, (int)timeOut.TotalMilliseconds);
                if (string.IsNullOrEmpty(record.bstrSymbol) == false
                    && record.bstrSymbol.ToUpper() == symbolName.ToUpper())
                {// Match found.
                    Quote quoteResult = new Quote();
                    ConvertQuoteRecord(ref quoteResult, record);
                    market = record.bstrMarket;
                    result = quoteResult;
                }
                else
                {
                    // Failed to find baseCurrency.
                    SystemMonitor.OperationWarning("Failed to find requested symbol.");
                }

            });

            if (result.HasValue)
            {
                symbolMarket = market;
            }

            return result;
        }

        #region IMbtQuotesNotify Members

        void IMbtQuotesNotify.OnLevel2Data(ref LEVEL2RECORD pRec)
        {
            throw new NotImplementedException();
        }

        void IMbtQuotesNotify.OnOptionsData(ref OPTIONSRECORD pRec)
        {
            throw new NotImplementedException();
        }

        void ConvertQuoteRecord(ref Quote quote, QUOTERECORD pRec)
        {
            quote.Ask = (0 == pRec.dAsk) ? (decimal?)null : (decimal)pRec.dAsk;
            quote.Bid = (0 == pRec.dBid) ? (decimal?)null : (decimal)pRec.dBid;

            quote.High = (0 == pRec.dHigh) ? (decimal?)null : (decimal)pRec.dHigh;
            quote.Low = (0 == pRec.dLow) ? (decimal?)null : (decimal)pRec.dLow;

            quote.Open = (decimal)pRec.dOpen;
            quote.Volume = pRec.lVolume;
            quote.Time = pRec.UTCDateTime;
        }

        void IMbtQuotesNotify.OnQuoteData(ref QUOTERECORD pRec)
        {
            SessionQuoteInformation information;
            Quote quote;
            lock (this)
            {
                if (_sessions.ContainsKey(pRec.bstrSymbol) == false)
                {
                    SystemMonitor.OperationWarning("Quote received for session not found.");
                    return;
                }

                information = _sessions[pRec.bstrSymbol];
                if (information.Quote.HasValue == false)
                {
                    information.Quote = new Quote();
                }

                quote = information.Quote.Value;
            }

            ConvertQuoteRecord(ref quote, pRec);

            lock (this)
            {
                information.Quote = quote;
            }

            if (QuoteUpdateEvent != null)
            {
                QuoteUpdateEvent(this, information);
            }
        }

        void IMbtQuotesNotify.OnTSData(ref TSRECORD pRec)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
