using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace CommonSupport
{
    /// <summary>
    /// Class stored tracer item items for further inspection.
    /// </summary>
    public class TracerItemKeeperSink : TracerItemSink
    {
        volatile int _maxItems = 200000;
        /// <summary>
        /// The maximum number of items, from the all items pool, to store in memory.
        /// Set to 0 to specify no limit.
        /// </summary>
        public int MaxItems
        {
            get { return _maxItems; }
            set { _maxItems = value; }
        }

        List<TracerItem> _items = new List<TracerItem>();

        /// <summary>
        /// Gathering and storing items by type is costly, so perform only when needed.
        /// </summary>
        Dictionary<TracerItem.TypeEnum, List<TracerItem>> _itemsByType = new Dictionary<TracerItem.TypeEnum, List<TracerItem>>();

        /// <summary>
        /// Set to null, to stop collecting items by type.
        /// Gathering and storing items by type is costly, so perform only when needed (needed by the TraceStatusStripOperator).
        /// </summary>
        public Dictionary<TracerItem.TypeEnum, List<TracerItem>> ItemsByTypeUnsafe
        {
            get { return _itemsByType; }
            set { _itemsByType = value; }
        }

        List<TracerItem> _filteredItems = new List<TracerItem>();
        /// <summary>
        /// Items passed trough filtering and were approved.
        /// Unsafe collection means the owner TracerItemKeeperSink class needs 
        /// to be locked before safe iteration.
        /// </summary>
        public ReadOnlyCollection<TracerItem> FilteredItemsUnsafe
        {
            get { return _filteredItems.AsReadOnly(); }
        }

        /// <summary>
        /// Fitlered items count.
        /// </summary>
        public int FilteredItemsCount
        {
            get { return _filteredItems.Count; }
        }

        public delegate void ItemAddedDelegate(TracerItemKeeperSink tracer, TracerItem item);

        [field: NonSerialized]
        public event ItemAddedDelegate ItemAddedEvent;
        
        /// <summary>
        /// 
        /// </summary>
        public TracerItemKeeperSink(Tracer tracer)
            : base(tracer)
        {
            SetupItemsByType();
        }

        /// <summary>
        /// Clear existing and filtered items.
        /// </summary>
        public override void Clear()
        {
            lock (this)
            {
                _items.Clear();
                _filteredItems.Clear();
                _itemsByType.Clear();
                SetupItemsByType();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void SetupItemsByType()
        {
            lock (this)
            {
                foreach (TracerItem.TypeEnum value in Enum.GetValues(typeof(TracerItem.TypeEnum)))
                {
                    if (_itemsByType.ContainsKey(value) == false)
                    {
                        _itemsByType.Add(value, new List<TracerItem>());
                    }
                }
            }
        }

        /// <summary>
        /// Put all items trough filtering again, to have a fresh set of FilteredItems.
        /// </summary>
        public void ReFilterItems()
        {
            Tracer tracer = Tracer;
            if (tracer == null)
            {
                return;
            }

            TracerFilter[] filters = FiltersArray;
            lock (this)
            {
                _filteredItems.Clear();
                
                foreach (TracerItem item in _items)
                {
                    if (Tracer.FilterItem(filters, item))
                    {
                        _filteredItems.Add(item);
                    }
                }
            }

            if (ItemAddedEvent != null)
            {
                ItemAddedEvent(this, null);
            }
        }

        protected override void filter_FilterUpdatedEvent(TracerFilter filter)
        {
            // This causes the filtering to be executed on the event raising thread.
            GeneralHelper.FireAndForget(ReFilterItems);

            base.filter_FilterUpdatedEvent(filter);
        }

        protected override bool OnReceiveItem(TracerItem item, bool isFilteredOutByTracer, bool isFilteredOutBySink)
        {
            if (isFilteredOutByTracer)
            {
                return true;
            }

            if (_maxItems > 0 && _items.Count > _maxItems)
            {// Remove the first 10%, only low importance items.
                lock (this)
                {
                    _items.RemoveRange(0, (int)((float)_maxItems / 10f));
                }

                // Also update at this moment filtered items.
                //ReFilterItems();
                if (_filteredItems.Count > _maxItems)
                {
                    lock (this)
                    {
                        _filteredItems.RemoveRange(0, (int)((float)_maxItems / 10f));
                    }
                }
            }

            lock (this)
            {
                _items.Add(item);

                if (_itemsByType != null)
                {
                    foreach (TracerItem.TypeEnum type in item.Types)
                    {
                        _itemsByType[type].Add(item);
                    }
                }

                if (isFilteredOutBySink == false)
                {
                    _filteredItems.Add(item);
                }
            }

            if (isFilteredOutBySink == false && ItemAddedEvent != null)
            {
                ItemAddedEvent(this, item);
            }

            return true;

        }
    }
}
