using System;
using System.Collections.Generic;
using System.Text;
using CommonSupport;

namespace CommonFinancial
{
    /// <summary>
    /// Trading baseCurrency information structure.
    /// </summary>
    [Serializable]
    public struct Symbol : IComparable<Symbol>
    {
        /// <summary>
        /// Group here is optional, and there could be symbols of Forex or Stocks
        /// but with different group symbol (like FX etc.)
        /// </summary>
        public enum SymbolGroup
        {
            Forex,
            Stocks
        }

        string _source;
        /// <summary>
        /// Name of the source providing this symbol, optional and applicable
        /// where multiple sources provide service trough a single provider.
        /// </summary>
        public string Source
        {
            get { return _source; }
        }

        string _group;
        public string Group
        {
            get { return _group; }
        }

        public string Market
        {
            get { return _group; }
        }

        string _name;
        public string Name
        {
            get { return _name; }
        }

        bool _isForexPair;
        public bool IsForexPair
        {
            get { return _isForexPair; }
        }

        string _forexCurrency1;
        /// <summary>
        /// Only available in Forex Pair symbols.
        /// </summary>
        public string ForexCurrency1
        {
            get { return _forexCurrency1; }
        }

        string _forexCurrency2;
        /// <summary>
        /// Only available in Forex Pair symbols.
        /// </summary>
        public string ForexCurrency2
        {
            get { return _forexCurrency2; }
        }

        /// <summary>
        /// Empty baseCurrency instance.
        /// </summary>
        public static Symbol Emtpy
        {
            get { return new Symbol(string.Empty, string.Empty); }
        }
    
        #region Operators

        /// <summary>
        /// 
        /// </summary>
        public static bool operator ==(Symbol a, Symbol b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool operator !=(Symbol a, Symbol b)
        {
            return !(a == b);
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Symbol == false)
            {
                return false;
            }

            return this.CompareTo((Symbol)obj) == 0;
        }


        /// <summary>
        /// 
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        public bool IsEmpty
        {
            get { return this == Emtpy; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseCurrency"></param>
        public Symbol(string symbol)
        {
            _group = string.Empty;
            _source = string.Empty;
            _name = symbol;

            _isForexPair = false;
            _forexCurrency1 = string.Empty;
            _forexCurrency2 = string.Empty;
            
            Construct();
        }

        /// <summary>
        /// 
        /// </summary>
        public Symbol(string group, string symbol, string source)
        {
            _name = symbol;
            _group = group;
            _source = source;

            _isForexPair = false;
            _forexCurrency1 = string.Empty;
            _forexCurrency2 = string.Empty;

            Construct();
        }

        /// <summary>
        /// 
        /// </summary>
        public Symbol(string group, string symbol)
        {
            _name = symbol;
            _group = group;
            _source = string.Empty;

            _isForexPair = false;
            _forexCurrency1 = string.Empty;
            _forexCurrency2 = string.Empty;

            Construct();
        }

        /// <summary>
        /// 
        /// </summary>
        public Symbol(SymbolGroup group, string symbol)
        {
            _name = symbol;
            _group = group.ToString();
            _source = string.Empty;

            _isForexPair = false;
            _forexCurrency1 = string.Empty;
            _forexCurrency2 = string.Empty;
            
            Construct();
        }

        void Construct()
        {
            if (string.IsNullOrEmpty(_name) == false)
            {
                _isForexPair = SplitForexSymbol(out _forexCurrency1, out _forexCurrency2);
            }
        }

        /// <summary>
        /// This will try to split the current symbol in 2 currencies for a forex pair.
        /// </summary>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <returns></returns>
        public bool SplitForexSymbol(out string currency1, out string currency2)
        {
            currency1 = string.Empty;
            currency2 = string.Empty;
            if (string.IsNullOrEmpty(this.Name))
            {
                return false;
            }

            string name = this.Name.ToUpper();
            string[] currencyNames = Enum.GetNames(typeof(CommonFinancial.Webservicex.CurrencyConvertor.Currency));
            foreach (string currencyName in currencyNames)
            {
                if (name.StartsWith(currencyName.ToUpper()))
                {// Found 1, try the other.
                    string subName = name.Substring(currencyName.Length);

                    foreach (string currencyName2 in currencyNames)
                    {
                        if (subName.Contains(currencyName2))
                        {// Found 2, we have a forex pair.
                            currency1 = currencyName;
                            currency2 = currencyName2;
                            return true;
                        }
                    }

                    // Failed to find second part.
                    return false;
                }
            }

            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        public bool MatchesSearchCriteria(string nameMatch)
        {
            if (nameMatch == "*" || string.IsNullOrEmpty(nameMatch))
            {
                return true;
            }
            return _name.ToLower().Contains(nameMatch.ToLower());
        }

        #region IComparable<BaseCurrency> Members

        public int CompareTo(Symbol other)
        {
            int compare = GeneralHelper.CompareNullable(_name, other._name);
            if (compare != 0)
            {
                return compare;
            }

            compare = GeneralHelper.CompareNullable(_source, other._source);
            if (compare != 0)
            {
                return compare;
            }

            return compare;

            // NOTE: Having this enabled causes a nasty bug in the DataSourceStub implementation.
            //return GeneralHelper.CompareNullable(_group, other._group);
        }

        #endregion

    }
}
