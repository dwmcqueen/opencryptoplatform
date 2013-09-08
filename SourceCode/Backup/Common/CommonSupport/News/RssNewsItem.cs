using System;
using System.Collections.Generic;
using System.Text;
using Rss;

namespace CommonSupport
{
    public class RssNewsItem : NewsItem
    {
        string _author = String.Empty;
        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }

        string _comments = String.Empty;
        public string Comments
        {
            get { return _comments; }
            set { _comments = value; }
        }

        string _guid = null;
        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public RssNewsItem()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public RssNewsItem(NewsSource source) 
            : base(source)
        {
        }

        /// <summary>
        /// Use a typical RssItem as data source.
        /// </summary>
        public RssNewsItem(RssNewsSource source, RssItem item) : base(source)
        {
            // If created from an rss item this means this is a new item, not known to the DB yet.
            this.IsRead = false;

            Author = item.Author;
            Comments = item.Comments;
            Description = item.Description;
            if (item.Guid != null)
            {
                Guid = item.Guid.Name;
            }
            Link = item.Link;
            DateTime = item.PubDate;
            Title = item.Title.Trim();
        }

        public override int CompareTo(NewsItem other)
        {
            RssNewsItem otherItem = (RssNewsItem)other;
            if (string.IsNullOrEmpty(Guid) == false)
            {// Just compare the Guids, if they are present, since otherwise some
                // sources republish items and this causes multiplication.
                return _guid.CompareTo(otherItem._guid);
            }

            int compare = base.CompareTo(other);
            if (compare != 0 || other.GetType() != this.GetType())
            {
                return compare;
            }
            
            compare = _author.CompareTo(otherItem.Author);
            if (compare != 0)
            {
                return compare;
            }

            compare = _comments.CompareTo(otherItem._comments);
            if (compare != 0)
            {
                return compare;
            }

            compare = GeneralHelper.CompareNullable(_guid, otherItem._guid);
            return compare;
        }
    }
}
