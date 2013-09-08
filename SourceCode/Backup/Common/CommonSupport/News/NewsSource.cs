using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;

namespace CommonSupport
{
    /// <summary>
    /// Base abstract class for all common types of news sources in the system.
    /// </summary>
    public abstract class NewsSource : DBPersistent, IOperational
    {
        /// <summary>
        /// Specify the type of news item a news source uses.
        /// </summary>
        public class NewsItemTypeAttribute : Attribute
        {
            Type _type;
            public Type TypeValue
            {
                get { return _type; }
            }

            /// <summary>
            /// 
            /// </summary>
            public NewsItemTypeAttribute(Type type)
            {
                _type = type;
            }
        }

        volatile bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set 
            { 
                _enabled = value;
                if (EnabledChangedEvent != null)
                {
                    EnabledChangedEvent(this);
                }
            }
        }

        protected volatile string _address = string.Empty;
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        protected volatile string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected volatile string _description = string.Empty;
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Ugly container, but makes sure that items belong to corresponding channel, and each channel has its items sort accessible by date time.
        /// The string is the channel name, the DateTime is the time of the channel and items that are corresponding to this time.
        /// </summary>
        protected Dictionary<string, SortedDictionary<DateTime, List<NewsItem>>> _channelsItems = new Dictionary<string, SortedDictionary<DateTime, List<NewsItem>>>();

        protected Dictionary<string, bool> _channels = new Dictionary<string, bool>();

        public List<string> ChannelsNames
        {
            get
            {
                lock (this)
                {
                    return new List<string>(_channels.Keys);
                }
            }
        }

        [DBPersistence(DBPersistenceAttribute.PersistenceTypeEnum.Binary)]
        public SerializationInfoEx Data
        {
            get
            {
                SerializationInfoEx info = new SerializationInfoEx();

                lock (this)
                {
                    info.AddValue("ChannelsNames", GeneralHelper.EnumerableToArray<string>(_channels.Keys));
                    info.AddValue("ChannelsEnabled", GeneralHelper.EnumerableToArray<bool>(_channels.Values));
                }

                return info;
            }

            set
            {
                if (value == null)
                {
                    return;
                }

                lock (this)
                {
                    _channels.Clear();
                    _channelsItems.Clear();

                    string[] names = value.GetValue<string[]>("ChannelsNames");
                    bool[] enabled = value.GetValue<bool[]>("ChannelsEnabled");

                    for (int i = 0; i < names.Length; i++)
                    {
                        AddChannel(names[i], enabled[i]);
                    }
                }
            }
        }

        protected volatile OperationalStateEnum _operationalState = OperationalStateEnum.UnInitialized;
        [DBPersistence(false)]
        public OperationalStateEnum OperationalState
        {
            get { return _operationalState; }
        }


        public delegate void EnabledChangedDelegate(NewsSource source);
        public delegate void ItemsUpdateDelegate(NewsSource source, IEnumerable<NewsItem> items);
        
        public event ItemsUpdateDelegate ItemsAddedEvent;
        public event ItemsUpdateDelegate ItemsUpdatedEvent;
        public event EnabledChangedDelegate EnabledChangedEvent;
        public event OperationalStateChangedDelegate OperationalStateChangedEvent;

        /// <summary>
        /// 
        /// </summary>
        public NewsSource()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            if (this.Enabled)
            {
                OnUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void OnUpdate();

        /// <summary>
        /// Is the channel enabled.
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public bool IsChannelEnabled(string channelName)
        {
            lock (this)
            {
                if (this.Enabled == false ||
                    _channels.ContainsKey(channelName) == false)
                {
                    return false;
                }

                return _channels[channelName];
            }
        }

        /// <summary>
        /// Set channel enabled.
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="enabled"></param>
        public void SetChannelEnabled(string channelName, bool enabled)
        {
            lock (this)
            {
                if (_channels.ContainsKey(channelName) == false ||
                    _channels[channelName] == enabled)
                {// No change.
                    return;
                }

                _channels[channelName] = enabled;
            }

            RaisePersistenceDataUpdatedEvent();
        }

        /// <summary>
        /// Source shortcut icon, may be null.
        /// </summary>
        /// <returns></returns>
        public virtual Image GetShortcutIcon()
        {
            return null;
        }

        protected void AddChannel(string name, bool enabled)
        {
            _channels.Add(name, enabled);
            _channelsItems.Add(name, new SortedDictionary<DateTime, List<NewsItem>>());
        }

        /// <summary>
        /// It is needed to specify the item type here, since otherwise when the result is empty,
        /// it can not be cast to the actual type in real time and causes an exception.
        /// The problem is casting an array ot base type to children type is not possible, when the array
        /// is coming from a list of base types converted with ToArray().
        /// </summary>
        public SortedDictionary<DateTime, List<NewsItemType>> GetChannelItems<NewsItemType>(string channelName)
            where NewsItemType : NewsItem
        {
            SortedDictionary<DateTime, List<NewsItemType>> result = new SortedDictionary<DateTime, List<NewsItemType>>();

            if (IsChannelEnabled(channelName))
            {
                foreach (DateTime time in _channelsItems[channelName].Keys)
                {
                    List<NewsItemType> list = new List<NewsItemType>();
                    result.Add(time, list);
                    if (_channelsItems[channelName][time].Count > 0)
                    {
                        foreach (NewsItemType item in _channelsItems[channelName][time])
                        {
                            list.Add(item);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// It is needed to specify the item type here, since otherwise when the result is empty,
        /// it can not be cast to the actual type in real time and causes an exception.
        /// The problem is casting an array ot base type to children type is not possible, when the array
        /// is coming from a list of base types converted with ToArray().
        /// </summary>
        public List<NewsItemType> GetAllItemsFlat<NewsItemType>()
            where NewsItemType : NewsItem
        {
            List<NewsItemType> result = new List<NewsItemType>();
            lock (this)
            {
                foreach(string channel in _channelsItems.Keys)
                {
                    if (IsChannelEnabled(channel))
                    {
                        foreach (DateTime time in _channelsItems[channel].Keys)
                        {
                            foreach (NewsItemType item in _channelsItems[channel][time])
                            {
                                result.Add(item);
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// It is needed to specify the item type here, since otherwise when the result is empty,
        /// it can not be cast to the actual type in real time and causes an exception.
        /// The problem is casting an array ot base type to children type is not possible, when the array
        /// is coming from a list of base types converted with ToArray().
        /// </summary>
        public SortedList<DateTime, List<NewsItemType>> GetAllItems<NewsItemType>()
            where NewsItemType : NewsItem
        {
            SortedList<DateTime, List<NewsItemType>> result = new SortedList<DateTime, List<NewsItemType>>();
            
            lock (this)
            {
                foreach(string channel in _channelsItems.Keys)
                {
                    if (IsChannelEnabled(channel))
                    {
                        foreach (DateTime time in _channelsItems[channel].Keys)
                        {
                            List<NewsItemType> list;
                            if (result.ContainsKey(time) == false)
                            {
                                list = new List<NewsItemType>();
                                result[time] = list;
                            }
                            else
                            {
                                list = result[time];
                            }

                            foreach (NewsItemType item in _channelsItems[channel][time])
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
            }

            return result;
        }

        protected void RaiseOperationalStatusChangedEvent(OperationalStateEnum previousState)
        {
            if (OperationalStateChangedEvent != null)
            {
                OperationalStateChangedEvent(this, previousState);
            }
        }

        public void HandleItemsUpdated(IEnumerable<NewsItem> items)
        {
            if (ItemsUpdatedEvent != null)
            {
                ItemsUpdatedEvent(this, items);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void AddItems(NewsItem[] items)
        {
            List<NewsItem> addedItems = new List<NewsItem>();
            lock (this)
            {
                string[] channelsNames = GeneralHelper.EnumerableToArray<string>(ChannelsNames);
                foreach (NewsItem newItem in items)
                {
                    if (channelsNames.Length <= newItem.ChannelId)
                    {
                        SystemMonitor.Warning("Item points to a not available channel.");
                        continue;
                    }

                    string channelName = channelsNames[newItem.ChannelId];

                    //if (_channels[channelName] == false)
                    //{// Channel disabled.
                    //    continue;
                    //}

                    NewsItem existingItem = null;
                    foreach (DateTime time in _channelsItems[channelName].Keys)
                    {// Item may be a repost with another time, so check all time periods.
                        foreach (NewsItem item in _channelsItems[channelName][time])
                        {
                            if (item.CompareTo(newItem) == 0)
                            {// Item already exists.
                                existingItem = item;
                                break;
                            }
                        }

                        if (existingItem != null)
                        {
                            break;
                        }
                    }

                    if (existingItem != null)
                    {
                        continue;
                    }

                    if (_channelsItems[channelName].ContainsKey(newItem.DateTime) == false)
                    {
                        _channelsItems[channelName][newItem.DateTime] = new List<NewsItem>();
                    }

                    _channelsItems[channelName][newItem.DateTime].Add(newItem);
                    addedItems.Add(newItem);
                }
            }

            if (ItemsAddedEvent != null && addedItems.Count > 0)
            {
                ItemsAddedEvent(this, addedItems.AsReadOnly());
            }
        }

    }
}
