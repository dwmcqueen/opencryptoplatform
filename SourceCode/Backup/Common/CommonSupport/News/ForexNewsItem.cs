using System;
using System.Collections.Generic;
using System.Text;

namespace CommonSupport
{
    /// <summary>
    /// Specific forex type news item.
    /// </summary>
    public class ForexNewsItem : NewsItem
    {
        public enum ImpactEnum
        {
            NA,
            Low,
            Medium,
            High
        }

        ImpactEnum _impact = ImpactEnum.NA;
        public ImpactEnum Impact
        {
            get { return _impact; }
            set { _impact = value; }
        }

        string _currency;
        public string Currency
        {
            get { return _currency; }
            set { _currency = value; }
        }

        TimeSpan? _timeSpan = null;
        public TimeSpan? TimeSpan
        {
            get { return _timeSpan; }
            set { _timeSpan = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ForexNewsItem(ForexNewsSource source) : base(source)
        {
        }

        public override int CompareTo(NewsItem other)
        {
            int compare = base.CompareTo(other);
            if (compare != 0 && other.GetType() != this.GetType())
            {
                return compare;
            }

            ForexNewsItem otherItem = (ForexNewsItem)other;
            compare = _impact.CompareTo(otherItem._impact);
            if (compare != 0)
            {
                return compare;
            }

            compare = _currency.CompareTo(otherItem._currency);
            if (compare != 0)
            {
                return compare;
            }

            compare = GeneralHelper.CompareNullable(_timeSpan.Value, otherItem._timeSpan.Value);
            return compare;
        }
    }
}
