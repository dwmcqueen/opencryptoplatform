using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace CommonSupport
{
    /// <summary>
    /// Class provides extended functionality to store items and manage, and is thread safe.
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    [Serializable]
    public class GenericContainer<ItemType> : IDeserializationCallback
        where ItemType : class
    {
        ListEx<ItemType> _items = new ListEx<ItemType>();

        bool _itemIsOperational = false;

        /// <summary>
        /// Provides a thread *unsafe* way to access the items, usefull when speed is essential.
        /// Make sure to lock this GenericContainer instance, while interating.
        /// </summary>
        public ListEx<ItemType> ListUnsafe
        {
            get { return _items; }
        }

        /// <summary>
        /// Retirieve items count.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this) { return _items.Count; } 
            }
        }
        
        /// <summary>
        /// Delegates for events applied in class.
        /// </summary>
        public delegate void ItemUpdateDelegate(GenericContainer<ItemType> keeper, ItemType item);
        public delegate bool ConfirmativeItemUpdateDelegate(GenericContainer<ItemType> keeper, ItemType item);

        /// <summary>
        /// 
        /// </summary>
        [field:NonSerialized]
        public event ItemUpdateDelegate ItemAddedEvent;
        
        [field: NonSerialized]
        public event ItemUpdateDelegate ItemRemovedEvent;
        
        /// <summary>
        /// Only applies when items are IOperational.
        /// </summary>
        [field: NonSerialized]
        public event ItemUpdateDelegate ItemOperationalStatusChangedEvent;

        /// <summary>
        /// Confirmation events allow to externally control the process of adding and removing
        /// items, approving or dissaproving the actions.
        /// </summary>
        [NonSerialized]
        ConfirmativeItemUpdateDelegate ItemAddDelegate;

        [NonSerialized]
        ConfirmativeItemUpdateDelegate ItemRemoveDelegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GenericContainer()
        {
            // Establish if item type is operational type.
            Type itemType = typeof(ItemType);
            foreach (Type type in itemType.GetInterfaces())
            {
                if (type == typeof(IOperational))
                {
                    _itemIsOperational = true;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Extended constructor.
        /// </summary>
        public GenericContainer(ConfirmativeItemUpdateDelegate itemAddDelegate,
            ConfirmativeItemUpdateDelegate itemRemoveDelegate)
        {
            SetupDelegates(itemAddDelegate, itemRemoveDelegate);
        }

        public void OnDeserialization(object sender)
        {
            ItemType[] serializedItems;
            lock (this)
            {
                serializedItems = _items.ToArray();
                _items.Clear();
            }

            foreach (ItemType item in serializedItems)
            {// Resubscribe for events etc.
                Add(item);
            }
        }

        /// <summary>
        /// Setup confirmative controlling delegates.
        /// </summary>
        public bool SetupDelegates(ConfirmativeItemUpdateDelegate itemAddDelegate,
            ConfirmativeItemUpdateDelegate itemRemoveDelegate)
        {
            ItemAddDelegate = itemAddDelegate;
            itemRemoveDelegate = ItemRemoveDelegate;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Add(ItemType item)
        {
            if (ItemAddDelegate != null)
            {
                if (ItemAddDelegate(this, item) == false)
                {// Confirmative owner delegate denied item addition.
                    return false;
                }
            }

            lock (this)
            {
                if (_items.Add(item) == false)
                {// Item add failed.
                    return false;
                }

                if (_itemIsOperational)
                {
                    ((IOperational)item).OperationalStateChangedEvent += new OperationalStateChangedDelegate(GenericKeeper_OperationalStatusChangedEvent);
                }
            }


            if (ItemAddedEvent != null)
            {
                ItemAddedEvent(this, item);
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        void GenericKeeper_OperationalStatusChangedEvent(IOperational operational, OperationalStateEnum previousOperationState)
        {
            if (ItemOperationalStatusChangedEvent != null)
            {
                ItemOperationalStatusChangedEvent(this, (ItemType)operational);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Remove(ItemType item)
        {
            if (ItemRemoveDelegate != null)
            {
                if (ItemRemoveDelegate(this, item) == false)
                {// Confirmative owner delegate denied item addition.
                    return false;
                }
            }

            lock (this)
            {
                if (_items.Remove(item) == false)
                {// Item remove failed.
                    return false;
                }

                if (_itemIsOperational)
                {
                    ((IOperational)item).OperationalStateChangedEvent -= new OperationalStateChangedDelegate(GenericKeeper_OperationalStatusChangedEvent);
                }
            }


            if (ItemRemovedEvent != null)
            {
                ItemRemovedEvent(this, item);
            }

            return true;
        }

        /// <summary>
        /// Access items as an array.
        /// </summary>
        public ItemType[] ToArray()
        {
            lock (this)
            {
                return _items.ToArray();
            }
        }

        /// <summary>
        /// Clear all items from storage.
        /// </summary>
        public void Clear(bool raiseRemoveEvents)
        {
            if (raiseRemoveEvents)
            {
                foreach (ItemType item in ToArray())
                {
                    Remove(item);
                }
            }
            else
            {
                lock (this)
                {
                    _items.Clear();
                }
            }
        }

    }
}
