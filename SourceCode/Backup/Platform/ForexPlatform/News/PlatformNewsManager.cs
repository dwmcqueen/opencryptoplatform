using System;
using System.Collections.Generic;
using System.Text;
using CommonSupport;
using ForexPlatformPersistence;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace ForexPlatform
{
    /// <summary>
    /// Implements the functionalities needed to allow the news manager to operate in the
    /// platform environment and in collaboration with other parts.
    /// </summary>
    [Serializable]
    public class PlatformNewsManager : NewsManager
    {
        [NonSerialized]
        ADOPersistenceHelper _persistenceHelper;

        [NonSerialized]
        Platform _platform;

        public Platform Platform
        {
            get { return _platform; }
        }

        /// <summary>
        /// 
        /// </summary>
        public PlatformNewsManager()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public PlatformNewsManager(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize(Platform platform)
        {
            SystemMonitor.CheckWarning(NewsSourcesUnsafe.Count == 0, "Manager already has assigned sources.");

            _platform = platform;

            _persistenceHelper = Platform.CreatePersistenceHelper(platform.Settings);

            _persistenceHelper.SetupTypeMapping(typeof(RssNewsItem), "RssNewsItems", null);
            _persistenceHelper.SetupTypeMapping(typeof(NewsSource), "NewsSources", null);
            _persistenceHelper.SetupTypeMapping(typeof(RssNewsSource), "NewsSources", null);

            _persistenceHelper.SetupTypeMapping(typeof(ForexNewsItem), "ForexNewsItems", null);
            _persistenceHelper.SetupTypeMapping(typeof(ForexNewsSource), "ForexNewsSources", null);


            // Make sure this load is before the accept events, so that they do not cause adding failure.
            List<NewsSource> sources = _persistenceHelper.SelectDynamicType<NewsSource>(null, "Type", null);

            base.SourceAddedEvent += new NewsSourceUpdateDelegate(PlatformNewsManager_SourceAddedEvent);
            base.SourceRemovedEvent += new NewsSourceUpdateDelegate(PlatformNewsManager_SourceRemovedEvent);

            foreach (NewsSource source in sources)
            {
                if (source.Enabled)
                {
                    List<RssNewsItem> items =
                        _persistenceHelper.Select<RssNewsItem>(new MatchExpression("NewsSourceId", source.Id), null);

                    foreach (RssNewsItem item in items)
                    {
                        item.Source = source;
                    }

                    // Handle the relation to persistence.
                    source.AddItems(items.ToArray());
                }

                base.AddSource(source);
            }


            GeneralHelper.FireAndForget(UpdateFeeds);
        }

        /// <summary>
        /// Handle enable changed, to load items from DB for source (since it may not have any loaded;
        /// implementing an load "On demand" mechanism"); also store change of Enabled to DB.
        /// </summary>
        /// <param name="source"></param>
        protected override void source_EnabledChangedEvent(NewsSource source)
        {
            if (source.Enabled)
            {// Extract items from DB, since it may have none at this point.
                List<RssNewsItem> items = 
                    _persistenceHelper.Select<RssNewsItem>(new MatchExpression("NewsSourceId", source.Id), null);
                foreach (RssNewsItem item in items)
                {
                    item.Source = source;
                }

                // Handle the relation to persistence.
                source.AddItems(items.ToArray());

            }

            // Update source to DB.
            source_PersistenceDataUpdatedEvent(source);

            base.source_EnabledChangedEvent(source);
        }

        public virtual void UnInitialize()
        {
        }

        void PlatformNewsManager_SourceAddedEvent(NewsManager manager, NewsSource source)
        {
            if (source.IsPersistedToDB == false)
            {// Already persisted to DB.
                SystemMonitor.CheckError(_persistenceHelper.InsertDynamicType<NewsSource>(source, "Type"), "Failed to add source to DB.");
            }

            source.PersistenceDataUpdatedEvent += new GeneralHelper.GenericDelegate<IDBPersistent>(source_PersistenceDataUpdatedEvent);
            source.ItemsAddedEvent += new NewsSource.ItemsUpdateDelegate(source_ItemsAddingAcceptEvent);
            source.ItemsUpdatedEvent += new NewsSource.ItemsUpdateDelegate(source_ItemsUpdatedEvent);

            // AddElement the items already in the source.
            foreach (string channelName in source.ChannelsNames)
            {
                source_ItemsAddingAcceptEvent(source, source.GetAllItemsFlat < NewsItem > ().AsReadOnly());
            }
        }

        void source_ItemsUpdatedEvent(NewsSource source, IEnumerable<NewsItem> items)
        {
            List<RssNewsItem> rssItems = new List<RssNewsItem>();
            foreach (RssNewsItem item in items)
            {
                rssItems.Add(item);
            }

            _persistenceHelper.UpdateToDB<RssNewsItem>(rssItems, null);
        }

        void PlatformNewsManager_SourceRemovedEvent(NewsManager manager, NewsSource source)
        {
            SystemMonitor.CheckError(_persistenceHelper.Delete<NewsSource>(new NewsSource[] { (source) }), "Failed to delete source from DB.");

            source.PersistenceDataUpdatedEvent -= new GeneralHelper.GenericDelegate<IDBPersistent>(source_PersistenceDataUpdatedEvent);
            source.ItemsAddedEvent -= new NewsSource.ItemsUpdateDelegate(source_ItemsAddingAcceptEvent);
            source.ItemsUpdatedEvent -= new NewsSource.ItemsUpdateDelegate(source_ItemsUpdatedEvent);

            _persistenceHelper.Delete<NewsSource>(source);

            _persistenceHelper.Delete<RssNewsItem>(new MatchExpression("NewsSourceId", source.Id));
        }

        void source_ItemsAddingAcceptEvent(NewsSource source, IEnumerable<NewsItem> items)
        {
            List<RssNewsItem> rssItems = new List<RssNewsItem>();
            foreach (RssNewsItem item in items)
            {
                if (item.IsPersistedToDB == false)
                {
                    rssItems.Add(item);
                }
            }

            if (rssItems.Count > 0)
            {
                _persistenceHelper.Insert<RssNewsItem>(rssItems, new KeyValuePair<string, object>("NewsSourceId", source.Id));
            }
        }

        void source_PersistenceDataUpdatedEvent(IDBPersistent source)
        {
            SystemMonitor.CheckError(_persistenceHelper.UpdateToDB((NewsSource)source, null), "Failed to update source.");
        }

    }
}
