using System;
using System.Collections.Generic;
using System.Text;
using Rss;
using System.Net;
using System.Security;
using System.Drawing;

namespace CommonSupport
{
    /// <summary>
    /// Default RSS news source; delivers news items from web RSS feeds.
    /// </summary>
    [NewsSource.NewsItemType(typeof(RssNewsItem))]
    public class RssNewsSource : NewsSource
    {
        RssFeed _feed;
        public RssFeed Feed
        {
            get { lock (this) { return _feed; } }
        }

        Image _icon = null;
        [DBPersistence(DBPersistenceAttribute.PersistenceTypeEnum.Binary)]
        public Image Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        /// <summary>
        /// Constructor needed for persistence.
        /// </summary>
        /// <param name="feedUri"></param>
        public RssNewsSource()
        {
            AddChannel("Default", true);
        }

        /// <summary>
        /// 
        /// </summary>
        public RssNewsSource(string address)
        {
            Address = address;
        }

        public override Image GetShortcutIcon()
        {
            return Icon;
        }

        void UpdateItems()
        {
            lock (this)
            {
                if (_feed == null)
                {
                    return;
                }

                foreach (RssChannel channel in _feed.Channels)
                {
                    List<RssNewsItem> newItems = new List<RssNewsItem>();
                    foreach (RssItem item in channel.Items)
                    {
                        newItems.Add(new RssNewsItem(this, item));
                    }
                    
                    // Add all by default to the "Default" channel, since RSS feeds never seem to bother with proper inner channels.
                    base.AddItems(newItems.ToArray());
                }
            }
        }

        public override void OnUpdate()
        {
            OperationalStateEnum newState;
            try
            {
                if (_feed == null)
                {
                    _feed = RssFeed.Read(base.Address);

                    if (_feed.Channels.Count == 1)
                    {
                        // Some feeds have those symbols in their names.
                        _name = _feed.Channels[0].Title.Replace("\r", "").Replace("\n", "").Trim();
                    }
                    else
                    {
                        _name = _feed.Url.ToString();
                    }

                    List<string> names = new List<string>();
                    foreach (RssChannel channel in _feed.Channels)
                    {
                        names.Add(channel.Title);
                    }

                    foreach (string name in names)
                    {
                        if (ChannelsNames.Contains(name) == false)
                        {
                            base.AddChannel(name, true);
                        }
                    }

                    // Retrieve web site shortcut icon.
                    //if (_icon == null)
                    //{
                    //    _icon = GeneralHelper.GetWebSiteShortcutIcon(new Uri(Address));
                    //}
                }
                else
                {
                    _feed = RssFeed.Read(_feed);
                }

                newState = OperationalStateEnum.Operational;
            }
            catch (WebException we)
            {// Feed not found or some other problem.
                SystemMonitor.OperationWarning("Failed to initialize feed [" + Address + ", " + we.Message + "]");
                newState = OperationalStateEnum.NotOperational;
            }
            catch (Exception ex)
            {// RssFeed class launches IOExceptions too, so get safe here.
                SystemMonitor.OperationWarning("Failed to initialize feed [" + Address + ", " + ex.Message + "]");
                newState = OperationalStateEnum.NotOperational;
            }

            OperationalStateEnum oldState = _operationalState;
            _operationalState = newState;
            if (newState != _operationalState)
            {
                RaiseOperationalStatusChangedEvent(oldState);
            }

            UpdateItems();

            RaisePersistenceDataUpdatedEvent();
        }
    }
}
