using System;
using System.Collections.Generic;
using System.Text;

namespace CommonSupport
{
    /// <summary>
    /// Base class for tracer filters. A filter is capable of filtering out trace
    /// items based on some criteria.
    /// </summary>
    public abstract class TracerFilter
    {
        Tracer _tracer;
        public Tracer Tracer
        {
            get { return _tracer; }
        }

        public delegate void FilterUpdatedDelegate(TracerFilter filter);
        public event FilterUpdatedDelegate FilterUpdatedEvent;

        /// <summary>
        /// 
        /// </summary>
        public TracerFilter(Tracer tracer)
        {
            _tracer = tracer;
        }

        protected void RaiseFilterUpdatedEvent()
        {
            if (FilterUpdatedEvent != null)
            {
                FilterUpdatedEvent(this);
            }
        }

        /// <summary>
        /// Will return true if item is allowed to pass filter.
        /// </summary>
        public abstract bool FilterItem(TracerItem item);
    }
}
