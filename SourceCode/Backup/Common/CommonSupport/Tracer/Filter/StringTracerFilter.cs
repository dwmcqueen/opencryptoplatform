using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;

namespace CommonSupport
{
    /// <summary>
    /// Filter for tracer items, based on string contents.
    /// </summary>
    [Serializable]
    public class StringTracerFilter : TracerFilter
    {
        volatile string _positiveFilterString = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string PositiveFilterString
        {
            get { return _positiveFilterString; }

            set
            {
                if (value != _positiveFilterString)
                {
                    _positiveFilterString = value;
                    RaiseFilterUpdatedEvent();
                }
            }
        }

        volatile string[] _negativeFilterStrings = null;

        public string[] NegativeFilterStrings
        {
            get { return _negativeFilterStrings; }
            set 
            { 
                _negativeFilterStrings = value;
                RaiseFilterUpdatedEvent();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StringTracerFilter(Tracer tracer)
            : base(tracer)
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override bool FilterItem(TracerItem item)
        {
            if (string.IsNullOrEmpty(_positiveFilterString) == false || _negativeFilterStrings != null)
            {
                return FilterItem(item, _positiveFilterString, _negativeFilterStrings);
            }

            return true;
        }

        public static bool FilterItem(TracerItem item, string positiveFilterString, string[] negativeFilterStrings)
        {
            string message = item.PrintMessage().ToLower();
            
            // Positive filter check.
            if (string.IsNullOrEmpty(positiveFilterString) == false
                && message.Contains(positiveFilterString.ToLower()) == false)
            {
                return false;
            }

            if (negativeFilterStrings != null)
            {
                // Negative filter check.
                foreach (string filter in negativeFilterStrings)
                {
                    if (string.IsNullOrEmpty(filter) == false && message.Contains(filter.ToLower()))
                    {
                        return false;
                    }
                }
            }

            // Pass.
            return true;
        }
    }
}
