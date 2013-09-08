using System;
using System.Drawing;
using System.Windows.Forms;

namespace CommonSupport
{
    /// <summary>
    /// Control allows the visualization of tracer and tracer items, filters etc.
    /// It is designed to operate on the items of the first tracer item sink keeper in tracer.
    /// </summary>
    public partial class TracerControl : UserControl
    {
        MethodTracerFilter _methodFilter;
        StringTracerFilter _stringFilter;
        TypeTracerFilter _typeFilter;
        PriorityFilter _priorityFilter;

        string _markingMatch = string.Empty;

        volatile bool _itemsModified = false;

        volatile TracerItemKeeperSink _itemKeeperSink = null;

        volatile Tracer _tracer;
        public Tracer Tracer
        {
            get { return _tracer; }
            set
            {
                if (_itemKeeperSink != null)
                {
                    _itemKeeperSink.ItemAddedEvent -= new TracerItemKeeperSink.ItemAddedDelegate(_tracer_ItemAddedEvent);
                    _itemKeeperSink.FilterUpdateEvent -= new TracerItemSink.SinkUpdateDelegate(_itemSink_FilterUpdateEvent);
                    _itemKeeperSink.ClearFilters();
                }

                if (_tracer != null)
                {
                    _methodFilter = null;
                    _stringFilter = null;
                    
                    _typeFilter = null;
                    _priorityFilter = null;

                    _tracer = null;

                    methodTracerFilterControl1.Filter = null;
                    typeTracerFilterControl1.Filter = null;
                }

                _tracer = value;
                //_mode = ModeEnum.Default;

                if (_tracer != null)
                {
                    foreach (ITracerItemSink sink in _tracer.ItemSinksArray)
                    {
                        if (sink is TracerItemKeeperSink)
                        {
                            _itemKeeperSink = sink as TracerItemKeeperSink;
                            break;
                        }
                    }

                    if (_itemKeeperSink != null)
                    {
                        _itemKeeperSink.ItemAddedEvent += new TracerItemKeeperSink.ItemAddedDelegate(_tracer_ItemAddedEvent);
                        _itemKeeperSink.FilterUpdateEvent += new TracerItemSink.SinkUpdateDelegate(_itemSink_FilterUpdateEvent);

                        _methodFilter = new MethodTracerFilter(_tracer);
                        _stringFilter = new StringTracerFilter(_tracer);
                        _typeFilter = new TypeTracerFilter(_tracer);
                        _priorityFilter = new PriorityFilter(_tracer);

                        _itemKeeperSink.AddFilter(_methodFilter);
                        _itemKeeperSink.AddFilter(_stringFilter);
                        _itemKeeperSink.AddFilter(_typeFilter);
                        _itemKeeperSink.AddFilter(_priorityFilter);

                        methodTracerFilterControl1.Filter = _methodFilter;
                        typeTracerFilterControl1.Filter = _typeFilter;
                    }

                }

                DoUpdateUI();
            }
        }

        /// <summary>
        /// Is the method filter control visible.
        /// </summary>
        public bool ShowMethodFilter
        {
            get { return this.methodTracerFilterControl1.Visible; }
            set { this.methodTracerFilterControl1.Visible = value; }
        }

        /// <summary>
        /// Is the item details (message) pane visible.
        /// </summary>
        public bool ShowDetails
        {
            get { return this.panelSelected.Visible; }
            set { panelSelected.Visible = value; }
        }

        /// <summary>
        /// Is the control auto updating upon receiving new messages.
        /// </summary>
        public bool AutoUpdate
        {
            get { return toolStripButtonAutoUpdate.Checked; }
            set { toolStripButtonAutoUpdate.Checked = true; }
        }

        /// <summary>
        /// Is the detailed properties of the selected item visibile.
        /// </summary>
        public bool DetailsVisible
        {
            get { return this.propertyGridItem.Visible; }
            set { propertyGridItem.Visible = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TracerControl()
        {
            InitializeComponent();
            _tracer = new Tracer();

            listView.VirtualItemsSelectionRangeChanged += new ListViewVirtualItemsSelectionRangeChangedEventHandler(listView_VirtualItemsSelectionRangeChanged);

            listView.AdvancedColumnManagement.Add(0, new VirtualListViewEx.ColumnManagementInfo() { AutoResizeMode = ColumnHeaderAutoResizeStyle.ColumnContent });
            listView.AdvancedColumnManagement.Add(1, new VirtualListViewEx.ColumnManagementInfo() { FillWhiteSpace = true });
        }

        private void TracerControl_Load(object sender, EventArgs e)
        {
            toolStripButtonDetails.Checked = false;
            toolStripButtonAutoScroll.Checked = listView.AutoScroll;

            toolStripComboBoxPriority.DropDownItems.Add("All").Click += new EventHandler(TracerControlPriorityItem_Click);
            toolStripComboBoxPriority.DropDownItems.Add(new ToolStripSeparator());

            foreach (string name in Enum.GetNames(typeof(TracerItem.PriorityEnum)))
            {
                ToolStripItem item = toolStripComboBoxPriority.DropDownItems.Add(name + " and above");
                item.Tag = Enum.Parse(typeof(TracerItem.PriorityEnum), name);
                item.Click += new EventHandler(TracerControlPriorityItem_Click);
            }
        }

        void TracerControlPriorityItem_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            PriorityFilter filter = _priorityFilter;
            if (filter == null)
            {
                return;
            }
            
            if (item.Tag != null)
            {
                filter.MinimumPriority = (TracerItem.PriorityEnum)item.Tag;
            }
            else
            {
                filter.MinimumPriority = TracerItem.PriorityEnum.Trivial;
            }

            UpdateUI();
        }

        void _tracer_ItemAddedEvent(TracerItemKeeperSink sink, TracerItem item)
        {
            _itemsModified = true;
        }

        void _itemSink_FilterUpdateEvent(TracerItemSink tracer)
        {
            WinFormsHelper.BeginFilteredManagedInvoke(this, DoUpdateUI);
        }

        const int CleanVirtualItemsCount = 3;

        /// <summary>
        /// 
        /// </summary>
        public void UpdateUI()
        {
            WinFormsHelper.DirectOrManagedInvoke(this, DoUpdateUI);
        }

        /// <summary>
        /// Update user interface based on the underlying information.
        /// </summary>
        void DoUpdateUI()
        {
            try
            {
                if (this.Tracer == null || _itemKeeperSink == null || this.DesignMode)
                {
                    return;
                }

                if (_priorityFilter == null || _priorityFilter.MinimumPriority == TracerItem.PriorityEnum.Minimum)
                {
                    toolStripComboBoxPriority.Text = "Priority [All]";
                }
                else
                {
                    toolStripComboBoxPriority.Text = "Priority [+" + _priorityFilter.MinimumPriority.ToString() + "]";
                }

                this.toolStripButtonEnabled.Checked = this.Tracer.Enabled;
                //toolStripButtonDetails.Checked = panelSelected.Visible;

                //_itemKeeperSink.ReFilterItems();

                // Give some slack for the vlist, since it has problems due to Microsoft List implementation.
                listView.VirtualListSize = _itemKeeperSink.FilteredItemsCount + CleanVirtualItemsCount;
                
                //ListViewItem item = listView.TopItem;
                // Needed to update scroll state and evade a bug in the list control (problem in any win list control).
                //this.listView.Scrollable = false;
                //this.listView.Scrollable = true;
                //if (_itemKeeperSink.FilteredItemsCount < 30 && listView.VirtualListSize > 0)
                //{
                //    //listView.EnsureVisible(0);
                //}

                //if (toolStripButtonAutoScroll.Checked && listView.VirtualListSize > 0)
                //{
                //    // Selection update is needed, since on size change the virtual list redraws also where the selected indeces are
                //    // and this causes massive flicker on auto update and auto scroll - the scroll skipping back and forth (since selection is somewhere back).
                //    listView.SelectedIndices.Clear();
                //    listView.SelectedIndices.Add(listView.VirtualListSize - 1);
                //    //listView.EnsureVisible(listView.VirtualListSize - 1);
                //}
            }
            catch (Exception ex)
            {
                SystemMonitor.Error("UI Logic Error [" + ex.Message + "]");
            }
        }

        void listView_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            
        }

        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            if (_itemKeeperSink != null)
            {
                _itemKeeperSink.ReFilterItems();
            }

            //DoUpdateUI();
        }

        private void listViewMain_Resize(object sender, EventArgs e)
        {
            this.listView.Columns[listView.Columns.Count - 1].Width = -2;
            this.listView.Invalidate();
        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            this.Tracer.Clear(false);

            //_mode = ModeEnum.Default;
            DoUpdateUI();
        }

        private void listViewMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            TracerItem item = null;
            if (_itemKeeperSink != null && listView.SelectedIndices.Count > 0)
            {
                int index = listView.SelectedIndices[0];

                lock (_itemKeeperSink)
                {
                    if (index < _itemKeeperSink.FilteredItemsCount && index < listView.VirtualListSize - CleanVirtualItemsCount)
                    {
                        item = _itemKeeperSink.FilteredItemsUnsafe[index];
                    }
                }
            }

            LoadTracerItem(item);
        }

        protected void LoadTracerItem(TracerItem item)
        {
            if (item == null)
            {
                textBoxSelectedItemMessage.Text = string.Empty;
            }
            else
            {
                textBoxSelectedItemMessage.Text = item.PrintMessage();
            }
            this.propertyGridItem.SelectedObject = item;
        }

        private void toolStripTextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                toolStripButtonSearch_Click(sender, EventArgs.Empty);
            }
        }

        private void toolStripButtonSearchClear_Click(object sender, EventArgs e)
        {
            toolStripTextBoxSearch.Text = "";
            toolStripButtonSearch_Click(sender, e);
        }

        private void toolStripButtonSearch_Click(object sender, EventArgs e)
        {
            _stringFilter.PositiveFilterString = toolStripTextBoxSearch.Text;
        }

        private void toolStripButtonMark_Click(object sender, EventArgs e)
        {
            _markingMatch = toolStripTextBoxMark.Text;
            DoUpdateUI();
            this.Refresh();
        }

        protected Color GetPriorityColor(TracerItem.PriorityEnum color)
        {
            switch (color)
            {
                case TracerItem.PriorityEnum.Trivial:
                case TracerItem.PriorityEnum.VeryLow:
                case TracerItem.PriorityEnum.Low:
                case TracerItem.PriorityEnum.Medium:
                    return Color.Transparent;

                case TracerItem.PriorityEnum.High:
                    return Color.MistyRose;
                case TracerItem.PriorityEnum.VeryHigh:
                    return Color.LightSalmon;
                case TracerItem.PriorityEnum.Critical:
                    return Color.Red;
            }

            return Color.Transparent;
        }

        private void listView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = new ListViewItem();

            TracerItem tracerItem = null;

            // If we are in the last items, make sure to always leave them blank.
            if (e.ItemIndex <= listView.VirtualListSize - CleanVirtualItemsCount)
            {
                lock (_itemKeeperSink)
                {// Hold the tracer not allowing it to modify its collection before we read it.
                    if (_itemKeeperSink != null && _itemKeeperSink.FilteredItemsCount > e.ItemIndex)
                    {
                        tracerItem = _itemKeeperSink.FilteredItemsUnsafe[e.ItemIndex];
                    }
                }
            }

            if (tracerItem == null)
            {
                e.Item.SubItems.Clear();
                for (int i = 0; i < listView.Columns.Count; i++)
                {
                    e.Item.SubItems.Add(string.Empty);
                }
                return;
            }

            switch (tracerItem.FullType)
            {
                case TracerItem.TypeEnum.MethodEntry:
                    e.Item.ImageIndex = 3;
                    break;
                case TracerItem.TypeEnum.MethodExit:
                    e.Item.ImageIndex = 4;
                    break;
                case TracerItem.TypeEnum.Trace:
                    e.Item.ImageIndex = 0;
                    break;
                case TracerItem.TypeEnum.System:
                    e.Item.ImageIndex = 6;
                    break;
                case TracerItem.TypeEnum.Warning:
                case (TracerItem.TypeEnum.Warning | TracerItem.TypeEnum.Operation):
                    e.Item.ImageIndex = 5;
                    break;
                case (TracerItem.TypeEnum.Error | TracerItem.TypeEnum.Operation):
                case TracerItem.TypeEnum.Error:
                    e.Item.ImageIndex = 2;
                    break;
            }

            if (e.Item.UseItemStyleForSubItems)
            {
                e.Item.UseItemStyleForSubItems = false;
            }

            Color color = GetPriorityColor(tracerItem.Priority);
            if (color != e.Item.SubItems[0].BackColor)
            {
                e.Item.SubItems[0].BackColor = color;
            }

            string day = tracerItem.DateTime.Day.ToString();
            if (tracerItem.DateTime.Day == 1)
            {
                day += "st";
            }
            else if (tracerItem.DateTime.Day == 2)
            {
                day += "nd";
            }
            else if (tracerItem.DateTime.Day == 3)
            {
                day += "rd";
            }
            else
            {
                day += "th";
            }

            string time = day + tracerItem.DateTime.ToString(", HH:mm:ss:ffff");

            e.Item.Text = tracerItem.Index/*(Tracer.TotalItemsCount - (Tracer.FilteredItemsCount - e.ItemIndex)).ToString()*/ + ", " + time;

            e.Item.SubItems.Add(tracerItem.PrintMessage());

            if (string.IsNullOrEmpty(_markingMatch) == false)
            {
                if (StringTracerFilter.FilterItem(tracerItem, _markingMatch, null))
                {
                    e.Item.BackColor = Color.MistyRose;
                }
            }
        }

        private void toolStripButtonAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            timerUpdate.Enabled = toolStripButtonAutoUpdate.Checked;
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if (_itemsModified)
            {
                _itemsModified = false;
                DoUpdateUI();
            }
            else
            {
                listView.UpdateAutoScrollPosition();
            }

            //TracerHelper.TraceEntry();
        }

        private void toolStripButtonEnabled_CheckStateChanged(object sender, EventArgs e)
        {
            if (Tracer == null)
            {
                return;
            }

            this.Tracer.Enabled = toolStripButtonEnabled.Checked;
        }

        private void markAllFromThisMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count == 0 || _itemKeeperSink == null)
            {
                return;
            }

            MethodTracerItem item = null;
            lock (_itemKeeperSink)
            {
                item = _itemKeeperSink.FilteredItemsUnsafe[listView.SelectedIndices[0]] as MethodTracerItem;
            }

            if (item != null)
            {
                toolStripTextBoxSearch.Text = item.MethodBase.DeclaringType.Name + "." + item.MethodBase.Name;
            }

            toolStripButtonMark_Click(sender, e);
        }

        private void markAllFromThisClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count == 0 || _itemKeeperSink == null)
            {
                return;
            }

            MethodTracerItem item = null;
            lock (_itemKeeperSink)
            {
                item = _itemKeeperSink.FilteredItemsUnsafe[listView.SelectedIndices[0]] as MethodTracerItem;
            }

            if (item != null)
            {
                toolStripTextBoxSearch.Text = item.MethodBase.DeclaringType.Module.Name.Substring(0, item.MethodBase.DeclaringType.Module.Name.LastIndexOf(".")) + "." + item.MethodBase.DeclaringType.Name;
            }

            toolStripButtonMark_Click(sender, e);
        }

        private void markAllFromThisModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count == 0 || _itemKeeperSink == null)
            {
                return;
            }

            MethodTracerItem item = null;
            lock (_itemKeeperSink)
            {
                item = _itemKeeperSink.FilteredItemsUnsafe[listView.SelectedIndices[0]] as MethodTracerItem;
            }

            if (item != null)
            {
                toolStripTextBoxSearch.Text = "[" + item.MethodBase.DeclaringType.Module.Name.Substring(0, item.MethodBase.DeclaringType.Module.Name.LastIndexOf(".")) + ".";
            }

            toolStripButtonMark_Click(sender, e);
        }

        private void ofThisMethodToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count == 0 || _itemKeeperSink == null)
            {
                return;
            }

            MethodTracerItem item = null;
            lock (_itemKeeperSink)
            {
                item = _itemKeeperSink.FilteredItemsUnsafe[listView.SelectedIndices[0]] as MethodTracerItem;
            }

            if (item != null)
            {
                toolStripTextBoxSearch.Text = item.MethodBase.DeclaringType.Name + "." + item.MethodBase.Name;
            }

            toolStripButtonSearch_Click(sender, e);
        }

        private void ofThisClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count == 0 || _itemKeeperSink == null)
            {
                return;
            }

            MethodTracerItem item = null;
            lock (_itemKeeperSink)
            {
                item = _itemKeeperSink.FilteredItemsUnsafe[listView.SelectedIndices[0]] as MethodTracerItem;
            }

            if (item != null)
            {
                toolStripTextBoxSearch.Text = item.MethodBase.DeclaringType.Module.Name.Substring(0, item.MethodBase.DeclaringType.Module.Name.LastIndexOf(".")) + "." + item.MethodBase.DeclaringType.Name;
            }

            toolStripButtonSearch_Click(sender, e);
        }

        private void ofThisModuleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count == 0 || _itemKeeperSink == null)
            {
                return;
            }

            MethodTracerItem item = null;
            lock (_itemKeeperSink)
            {
                item = _itemKeeperSink.FilteredItemsUnsafe[listView.SelectedIndices[0]] as MethodTracerItem;
            }

            if (item != null)
            {
                toolStripTextBoxSearch.Text = "[" + item.MethodBase.DeclaringType.Module.Name.Substring(0, item.MethodBase.DeclaringType.Module.Name.LastIndexOf(".")) + ".";
            }

            toolStripButtonSearch_Click(sender, e);
        }

        private void toolStripButtonAutoScroll_CheckedChanged(object sender, EventArgs e)
        {
            this.listView.AutoScroll = toolStripButtonAutoScroll.Checked;
        }

        private void toolStripButtonDetails_CheckedChanged(object sender, EventArgs e)
        {
            panelSelected.Visible = toolStripButtonDetails.Checked;
            splitterDetails.Visible = panelSelected.Visible;
        }

        private void toolStripButtonClearMark_Click(object sender, EventArgs e)
        {
            toolStripTextBoxMark.Text = string.Empty;
            toolStripButtonMark_Click(sender, e);
        }

        private void toolStripButtonClearExclude_Click(object sender, EventArgs e)
        {
            toolStripTextBoxExclude.Text = string.Empty;
            toolStripButtonExclude_Click(sender, e);
        }

        private void toolStripButtonExclude_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(toolStripTextBoxExclude.Text))
            {
                _stringFilter.NegativeFilterStrings = null;
            }
            else
            {
               _stringFilter.NegativeFilterStrings = toolStripTextBoxExclude.Text.Split(';');
            }
        }

        private void toolStripTextBoxExclude_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                toolStripButtonExclude_Click(sender, EventArgs.Empty);
            }
        }

        private void toolStripTextBoxMark_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                toolStripButtonMark_Click(sender, EventArgs.Empty);
            }
        }

    }
}
