using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace CommonSupport
{
    /// <summary>
    /// Extended version of the WinForms Virtual List View.
    /// </summary>
    [Serializable]
    public class VirtualListViewEx : ListView
    {
        public class ColumnManagementInfo
        {
            public bool FillWhiteSpace = false;
            public ColumnHeaderAutoResizeStyle AutoResizeMode = ColumnHeaderAutoResizeStyle.None;
            public int MinWidth = 0;
        }

        Dictionary<int, ColumnManagementInfo> _advancedColumnManagement = new Dictionary<int, ColumnManagementInfo>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<int, ColumnManagementInfo> AdvancedColumnManagement
        {
            get { lock (this) { return _advancedColumnManagement; } }
            set { lock (this) { _advancedColumnManagement = value; } }
        }

        // Needed to evade StackOverflows.
        volatile bool _isUpdatingColumnWidths = false;

        Size _lastSize;

        public delegate void ChangeDelegate(int i, int item);
        //public event ChangeDelegate TopItemChangeEvent;

        bool _autoScroll = true;
        public bool AutoScroll
        {
            get { return _autoScroll; }
            set { _autoScroll = value; }
        }

        int _autoScrollSlack = 3;
        /// <summary>
        /// 
        /// </summary>
        public int AutoScrollSlack
        {
            get { return _autoScrollSlack; }
            set { _autoScrollSlack = value; }
        }

        public new ListViewItem TopItem
        {
            get { return null; }
            set
            {
            }
        }

        int _lastEnsureUpdateValue = 0;
        public new int VirtualListSize
        {
            get
            {
                lock (this) { return base.VirtualListSize; }
            }

            set
            {
                try
                {
                    if (this.IsHandleCreated == false)
                    {
                        SystemMonitor.Warning("Call to early, list not yet created.");
                        return;
                    }

                    lock (this)
                    {
                        //this.SuspendLayout();

                        if (_autoScroll)
                        {
                            // Selection update is needed, since on size change the virtual list redraws also where the selected indeces are
                            // and this causes massive flicker on auto update and auto scroll - the scroll skipping back and forth (since selection is somewhere back).
                            SelectedIndices.Clear();
                        }

                        if (base.VirtualListSize > value)
                        {// There is a bug in the list and it crashes if you change size after scroll is not 0.
                            if (_autoScroll)
                            {
                                EnsureVisible(Math.Max(0, value - _autoScrollSlack));
                            }
                            else
                            {
                                EnsureVisible(0);
                            }
                        }


                        #region BUG EVASION
                        // There is a bug in .NET Virtual List and this fixes it (partially).
                        // See more here: http://social.msdn.microsoft.com/forums/en-US/winforms/thread/f24ffbc5-59f0-4f18-800a-ff2fbbe418e0/

                        // Must set top value to at least one less than value due to
                        // off-by-one error in base.VirtualListSize

                        int topIndex = this.TopItem == null ? -1 : this.TopItem.Index;
                        topIndex = Math.Min(topIndex, Math.Abs(value - 1));

                        if (this.VirtualListSize != 0 && topIndex >= 0)
                        {
                            this.TopItem = this.Items[topIndex];
                        }

                        #endregion

                        // There is some calculatory magic here, do not change with no reason.
                        //bool moved = value > _lastEnsureUpdateValue + 5;

                        base.VirtualListSize = value;

                        if (VirtualListSize > 5 && _autoScroll)
                        {// If we move closer to -1 the list crashes. So we need to wait, 
                            // it is best if a timer calls the UpdateAutoScrollPosition() after a while to scroll all the way down.
                            EnsureVisible(Math.Max(0, value - _autoScrollSlack));
                            
                            _lastEnsureUpdateValue = value;
                        }

                        //this.ResumeLayout();
                    }
                }
                catch (Exception ex)
                {
                    SystemMonitor.Error("UI Logic Error [" + ex.Message +"]");
                }

                // This helps when the Size is set too early (even before the List is first drawn)
                // so we deploy a request to update widths, and it gets run when all other pending
                // events are processed.
                WinFormsHelper.BeginManagedInvoke(this, UpdateColumnWidths);
            }
        }

        public new View View
        {
            get { return View.Details; }
            set { }
        }

        /// <summary>
        /// 
        /// </summary>
        public VirtualListViewEx()
        {
            this.VirtualMode = true;

            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            base.View = View.Details;

            //this.TopItemChangeEvent += new ChangeDelegate(VirtualListViewEx_TopItemChangeEvent);

            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        //void VirtualListViewEx_TopItemChangeEvent(int i, int item)
        //{
        //    // This causes a bad overdraw flicker, do not uncomment!
        //    //UpdateColumnWidths();
        //}

        //int _lastTopItem = -1;
        //protected override void OnNotifyMessage(Message m)
        //{
        //    lock (this)
        //    {
        //        base.OnNotifyMessage(m);
        //        if (this != null && this.Visible && TopItemChangeEvent != null && m.Msg != 0x1027)
        //        {// TODO : under MONO the message code may be different.

        //            ListViewItem topItem = this.TopItem;

        //            // 1027 is escaped to prevent stack overflows, since call to TopItem causes it.
        //            if (topItem != null && _lastTopItem != topItem.Index)
        //            {
        //                TopItemChangeEvent(topItem.Index, 0);
        //                _lastTopItem = topItem.Index;
        //            }
        //        }
        //    }
        //}

        // Handle this to provide items.
        //protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        //{
        //}

        public void UpdateAutoScrollPosition()
        {
            try
            {
                if (VirtualListSize > 5 && _autoScroll)
                {// If we move closer to -1 the list crashes.
                    //EnsureVisible(VirtualListSize - 1);
                }
            }
            catch (Exception ex)
            {
                SystemMonitor.Warning("UI logic error (ListView bug).");
            }
        }

        public void UpdateColumnWidths()
        {
            lock (this)
            {
                if (_isUpdatingColumnWidths)
                {
                    return;
                }
                _isUpdatingColumnWidths = true;

                // Step 1 - all columns size to content or to header.
                foreach (int index in _advancedColumnManagement.Keys)
                {
                    if (this.Columns.Count <= index)
                    {
                        continue;
                    }

                    ColumnManagementInfo info = _advancedColumnManagement[index];
                    if (info.AutoResizeMode != ColumnHeaderAutoResizeStyle.None)
                    {
                        this.Columns[index].AutoResize(info.AutoResizeMode);
                    }


                }

                // Step 3 - all columns min width.
                foreach (int index in _advancedColumnManagement.Keys)
                {
                    if (this.Columns.Count <= index)
                    {
                        continue;
                    }

                    ColumnManagementInfo info = _advancedColumnManagement[index];

                    if (info.MinWidth > 0)
                    {
                        this.Columns[index].Width = Math.Max(this.Columns[index].Width, info.MinWidth);
                    }
                }

                // Step 2 - all columns fill white space to content or to header.
                foreach (int index in _advancedColumnManagement.Keys)
                {
                    if (this.Columns.Count <= index)
                    {
                        continue;
                    }
                    ColumnManagementInfo info = _advancedColumnManagement[index];

                    // Handle auto fill white space column resize.
                    if (info.FillWhiteSpace)
                    {
                        int totalWidth = 0;
                        foreach (ColumnHeader header in this.Columns)
                        {
                            totalWidth += header.Width;
                        }

                        int margin = this.Width - totalWidth - 20;
                        this.Columns[index].Width = Math.Max(0, Columns[index].Width + margin);
                    }
                }

                _isUpdatingColumnWidths = false;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            
            //UpdateColumnWidths();
        }

        protected override void OnVirtualItemsSelectionRangeChanged(ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            base.OnVirtualItemsSelectionRangeChanged(e);

            ///UpdateColumnWidths();
        }

        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            base.OnRetrieveVirtualItem(e);
        }

        protected override void OnRegionChanged(EventArgs e)
        {
            base.OnRegionChanged(e);
        }

        protected override void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
        {
            base.OnColumnWidthChanging(e);
        }

        protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
        {
            base.OnColumnWidthChanged(e);
            //UpdateColumnWidths();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            //if (this.Size != _lastSize)
            //{// Optimization to lower the number of updates called.
                UpdateColumnWidths();
                lock (this)
                {
                    _lastSize = this.Size;
                }
            //}
        }


    }
}
