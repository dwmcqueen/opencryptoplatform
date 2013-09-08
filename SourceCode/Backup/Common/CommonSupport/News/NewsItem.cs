using System;
using System.Collections.Generic;
using System.Text;

namespace CommonSupport
{
    /// <summary>
    /// A single news item.
    /// </summary>
    public class NewsItem : DBPersistent, IComparable<NewsItem>
    {
        bool _visible = true;
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        bool _isRead = false;
        public bool IsRead
        {
            get { return _isRead; }
            set { _isRead = value; }
        }

        bool _isFavourite = false;
        public bool IsFavourite
        {
            get { return _isFavourite; }
            set { _isFavourite = value; }
        }

        //bool _isNewlyAdded = false;

        ///// <summary>
        ///// Has item been aquired from DB or is it a new one.
        ///// This is not serialized and is used in comparitions.
        ///// </summary>
        //[DBPersistence(false)]
        //public bool IsNewlyAdded
        //{
        //    get { return _isNewlyAdded; }
        //    set { _isNewlyAdded = value; }
        //}

        int _channelId = 0;
        public int ChannelId
        {
            get { return _channelId; }
            set { _channelId = value; }
        }

        Uri _link;
        public Uri Link
        {
            get { return _link; }
            set { _link = value; }
        }

        string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        DateTime _dateTime;
        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        NewsSource _source;
        [DBPersistence(false)]
        public NewsSource Source
        {
            get { return _source; }
            set { _source = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NewsItem()
        {
            _source = null;
        }

        /// <summary>
        /// Extended constructor.
        /// </summary>
        public NewsItem(NewsSource source)
        {
            _source = source;
        }

        #region IComparable<NewsItem> Members

        public virtual int CompareTo(NewsItem other)
        {
            int compare = _dateTime.CompareTo(other.DateTime);
            if (compare != 0)
            {
                return compare;
            }
            compare = _description.CompareTo(other.Description);
            if (compare != 0)
            {
                return compare;
            }
            compare = _title.CompareTo(other.Title);
            if (compare != 0)
            {
                return compare;
            }
            compare = _link.AbsolutePath.CompareTo(other.Link.AbsolutePath);
            return compare;
        }

        #endregion
    }
}
