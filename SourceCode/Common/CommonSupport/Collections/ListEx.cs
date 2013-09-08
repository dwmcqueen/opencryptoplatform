using System;
using System.Collections.Generic;
using System.Text;

namespace CommonSupport
{
    /// <summary>
    /// Extends the bahaviour of the List class to provide single entry mode, etc.
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    [Serializable]
    public class ListEx<TClass> : List<TClass>
    {
        bool _singleEntryMode = true;
        /// <summary>
        /// An item is allowed to enter only once.
        /// </summary>
        public bool SingleEntryMode
        {
            get { return _singleEntryMode; }
            set { _singleEntryMode = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ListEx()
        {
        }

        /// <summary>
        /// Add/Update item entry.
        /// </summary>
        public void UpdateItem(TClass item, bool isAdded)
        {
            if (isAdded)
            {
                Add(item);
            }
            else
            {
                Remove(item);
            }
        }

        /// <summary>
        /// Add operation override.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public new bool Add(TClass item)
        {
            if (SingleEntryMode && this.Contains(item))
            {
                return false;
            }

            base.Add(item);
            return true;
        }

        public new void AddRange(IEnumerable<TClass> collection)
        {
            if (SingleEntryMode)
            {
                List<TClass> items = new List<TClass>();
                foreach (TClass item in collection)
                {
                    if (this.Contains(item) == false)
                    {
                        items.Add(item);
                    }
                }
                base.AddRange(items);
            }
            else
            {
                base.AddRange(collection);
            }
        }

        public new void Insert(int index, TClass item)
        {
            if (SingleEntryMode && this.Contains(item))
            {
                return;
            }

            base.Insert(index, item);
        }

        public new void InsertRange(int index, IEnumerable<TClass> collection)
        {
            if (SingleEntryMode)
            {
                List<TClass> items = new List<TClass>();
                foreach (TClass item in collection)
                {
                    if (this.Contains(item) == false)
                    {
                        items.Add(item);
                    }
                }
                base.InsertRange(index, items);
            }
            else
            {
                base.InsertRange(index, collection);
            }
            
        }

        public void RemoveRange(IEnumerable<TClass> items)
        {
            foreach (TClass item in items)
            {
                base.Remove(item);
            }
        }
    }
}
