using System;
using System.Collections.Generic;
using System.Text;

namespace CommonSupport
{
    /// <summary>
    /// Filters tracer items based on their priority.
    /// </summary>
    public class PriorityFilter : TracerFilter
    {
        volatile TracerItem.PriorityEnum _minimumPriority = TracerItem.PriorityEnum.Trivial;
        /// <summary>
        /// Minimum allowed (inclusive) priority allowed.
        /// </summary>
        public TracerItem.PriorityEnum MinimumPriority
        {
            get { return _minimumPriority; }
            set 
            {
                if (_minimumPriority != value)
                {
                    _minimumPriority = value;
                    RaiseFilterUpdatedEvent();
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PriorityFilter(Tracer tracer)
            : base(tracer)
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override bool FilterItem(TracerItem item)
        {
            return item.Priority >= _minimumPriority;
        }
    }
}
