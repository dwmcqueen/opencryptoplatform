using System.Windows.Forms;
namespace CommonSupport
{
    partial class TracerControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required baseMethod for Designer support - do not modify 
        /// the contents of this baseMethod with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TracerControl));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonEnabled = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDetails = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonAutoUpdate = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonAutoScroll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripComboBoxPriority = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSearchClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBoxExclude = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButtonExclude = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonClearExclude = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBoxMark = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButtonMark = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonClearMark = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonClear = new System.Windows.Forms.ToolStripButton();
            this.propertyGridItem = new System.Windows.Forms.PropertyGrid();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.labelSelected = new System.Windows.Forms.Label();
            this.panelSelected = new System.Windows.Forms.Panel();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStripItemsList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.markAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ofThisMethodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ofThisClassToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ofThisModuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ofThisMethodToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ofThieClassToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ofThisModuleToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.methodTracerFilterControl1 = new CommonSupport.MethodTracerFilterControl();
            this.splitter2 = new CommonSupport.SplitterEx();
            this.typeTracerFilterControl1 = new CommonSupport.TypeTracerFilterControl();
            this.panelItemMessage = new System.Windows.Forms.Panel();
            this.textBoxSelectedItemMessage = new System.Windows.Forms.TextBox();
            this.splitterItemMessage = new System.Windows.Forms.Splitter();
            this.listView = new CommonSupport.VirtualListViewEx();
            this.columnGeneral = new System.Windows.Forms.ColumnHeader();
            this.columnMessage = new System.Windows.Forms.ColumnHeader();
            this.splitter3 = new CommonSupport.SplitterEx();
            this.splitterDetails = new CommonSupport.SplitterEx();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip.SuspendLayout();
            this.panelSelected.SuspendLayout();
            this.contextMenuStripItemsList.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.panelItemMessage.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 24);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 24);
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 24);
            // 
            // toolStrip
            // 
            this.toolStrip.AutoSize = false;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonEnabled,
            toolStripSeparator3,
            this.toolStripButtonRefresh,
            toolStripSeparator2,
            this.toolStripButtonDetails,
            toolStripSeparator4,
            this.toolStripButtonAutoUpdate,
            this.toolStripButtonAutoScroll,
            this.toolStripSeparator1,
            this.toolStripComboBoxPriority,
            this.toolStripSeparator7,
            this.toolStripTextBoxSearch,
            this.toolStripButtonSearch,
            this.toolStripButtonSearchClear,
            this.toolStripTextBoxExclude,
            this.toolStripButtonExclude,
            this.toolStripButtonClearExclude,
            this.toolStripSeparator5,
            this.toolStripTextBoxMark,
            this.toolStripButtonMark,
            this.toolStripButtonClearMark,
            this.toolStripSeparator6,
            this.toolStripButtonClear});
            this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 2, 2, 0);
            this.toolStrip.Size = new System.Drawing.Size(1261, 26);
            this.toolStrip.TabIndex = 5;
            this.toolStrip.Text = "toolStrip1";
            // 
            // toolStripButtonEnabled
            // 
            this.toolStripButtonEnabled.Checked = true;
            this.toolStripButtonEnabled.CheckOnClick = true;
            this.toolStripButtonEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonEnabled.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonEnabled.Image")));
            this.toolStripButtonEnabled.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonEnabled.Name = "toolStripButtonEnabled";
            this.toolStripButtonEnabled.Size = new System.Drawing.Size(65, 21);
            this.toolStripButtonEnabled.Text = "Enabled";
            this.toolStripButtonEnabled.CheckStateChanged += new System.EventHandler(this.toolStripButtonEnabled_CheckStateChanged);
            // 
            // toolStripButtonRefresh
            // 
            this.toolStripButtonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRefresh.Image")));
            this.toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRefresh.Name = "toolStripButtonRefresh";
            this.toolStripButtonRefresh.Size = new System.Drawing.Size(62, 21);
            this.toolStripButtonRefresh.Text = "Update";
            this.toolStripButtonRefresh.Click += new System.EventHandler(this.toolStripButtonRefresh_Click);
            // 
            // toolStripButtonDetails
            // 
            this.toolStripButtonDetails.AutoSize = false;
            this.toolStripButtonDetails.Checked = true;
            this.toolStripButtonDetails.CheckOnClick = true;
            this.toolStripButtonDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonDetails.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonDetails.Name = "toolStripButtonDetails";
            this.toolStripButtonDetails.Size = new System.Drawing.Size(47, 19);
            this.toolStripButtonDetails.Text = "Details";
            this.toolStripButtonDetails.ToolTipText = "Show Selected Trace Item Details";
            this.toolStripButtonDetails.CheckedChanged += new System.EventHandler(this.toolStripButtonDetails_CheckedChanged);
            // 
            // toolStripButtonAutoUpdate
            // 
            this.toolStripButtonAutoUpdate.AutoSize = false;
            this.toolStripButtonAutoUpdate.CheckOnClick = true;
            this.toolStripButtonAutoUpdate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAutoUpdate.Name = "toolStripButtonAutoUpdate";
            this.toolStripButtonAutoUpdate.Size = new System.Drawing.Size(77, 19);
            this.toolStripButtonAutoUpdate.Text = "Auto Update";
            this.toolStripButtonAutoUpdate.ToolTipText = "Auto Update [Every 500ms]";
            this.toolStripButtonAutoUpdate.CheckedChanged += new System.EventHandler(this.toolStripButtonAutoUpdate_CheckedChanged);
            // 
            // toolStripButtonAutoScroll
            // 
            this.toolStripButtonAutoScroll.AutoSize = false;
            this.toolStripButtonAutoScroll.CheckOnClick = true;
            this.toolStripButtonAutoScroll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAutoScroll.Name = "toolStripButtonAutoScroll";
            this.toolStripButtonAutoScroll.Size = new System.Drawing.Size(67, 19);
            this.toolStripButtonAutoScroll.Text = "Auto Scroll";
            this.toolStripButtonAutoScroll.ToolTipText = "Auto Scroll to End";
            this.toolStripButtonAutoScroll.CheckedChanged += new System.EventHandler(this.toolStripButtonAutoScroll_CheckedChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 24);
            // 
            // toolStripComboBoxPriority
            // 
            this.toolStripComboBoxPriority.Name = "toolStripComboBoxPriority";
            this.toolStripComboBoxPriority.Size = new System.Drawing.Size(54, 21);
            this.toolStripComboBoxPriority.Text = "Priority";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 24);
            // 
            // toolStripTextBoxSearch
            // 
            this.toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            this.toolStripTextBoxSearch.Size = new System.Drawing.Size(120, 24);
            this.toolStripTextBoxSearch.ToolTipText = "Value to Search or Mark by";
            this.toolStripTextBoxSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripTextBoxSearch_KeyDown);
            // 
            // toolStripButtonSearch
            // 
            this.toolStripButtonSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSearch.Name = "toolStripButtonSearch";
            this.toolStripButtonSearch.Size = new System.Drawing.Size(44, 21);
            this.toolStripButtonSearch.Text = "Search";
            this.toolStripButtonSearch.Click += new System.EventHandler(this.toolStripButtonSearch_Click);
            // 
            // toolStripButtonSearchClear
            // 
            this.toolStripButtonSearchClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonSearchClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSearchClear.Name = "toolStripButtonSearchClear";
            this.toolStripButtonSearchClear.Size = new System.Drawing.Size(36, 21);
            this.toolStripButtonSearchClear.Text = "Clear";
            this.toolStripButtonSearchClear.Click += new System.EventHandler(this.toolStripButtonSearchClear_Click);
            // 
            // toolStripTextBoxExclude
            // 
            this.toolStripTextBoxExclude.Name = "toolStripTextBoxExclude";
            this.toolStripTextBoxExclude.Size = new System.Drawing.Size(120, 24);
            this.toolStripTextBoxExclude.ToolTipText = "Value to Search or Mark by";
            this.toolStripTextBoxExclude.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripTextBoxExclude_KeyDown);
            // 
            // toolStripButtonExclude
            // 
            this.toolStripButtonExclude.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonExclude.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonExclude.Name = "toolStripButtonExclude";
            this.toolStripButtonExclude.Size = new System.Drawing.Size(48, 21);
            this.toolStripButtonExclude.Text = "Exclude";
            this.toolStripButtonExclude.Click += new System.EventHandler(this.toolStripButtonExclude_Click);
            // 
            // toolStripButtonClearExclude
            // 
            this.toolStripButtonClearExclude.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonClearExclude.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClearExclude.Name = "toolStripButtonClearExclude";
            this.toolStripButtonClearExclude.Size = new System.Drawing.Size(36, 21);
            this.toolStripButtonClearExclude.Text = "Clear";
            this.toolStripButtonClearExclude.Click += new System.EventHandler(this.toolStripButtonClearExclude_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 24);
            // 
            // toolStripTextBoxMark
            // 
            this.toolStripTextBoxMark.Name = "toolStripTextBoxMark";
            this.toolStripTextBoxMark.Size = new System.Drawing.Size(120, 24);
            this.toolStripTextBoxMark.ToolTipText = "Value to Search or Mark by";
            this.toolStripTextBoxMark.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripTextBoxMark_KeyDown);
            // 
            // toolStripButtonMark
            // 
            this.toolStripButtonMark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonMark.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonMark.Name = "toolStripButtonMark";
            this.toolStripButtonMark.Size = new System.Drawing.Size(34, 21);
            this.toolStripButtonMark.Text = "Mark";
            this.toolStripButtonMark.Click += new System.EventHandler(this.toolStripButtonMark_Click);
            // 
            // toolStripButtonClearMark
            // 
            this.toolStripButtonClearMark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonClearMark.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClearMark.Name = "toolStripButtonClearMark";
            this.toolStripButtonClearMark.Size = new System.Drawing.Size(36, 21);
            this.toolStripButtonClearMark.Text = "Clear";
            this.toolStripButtonClearMark.Click += new System.EventHandler(this.toolStripButtonClearMark_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 24);
            // 
            // toolStripButtonClear
            // 
            this.toolStripButtonClear.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonClear.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonClear.Image")));
            this.toolStripButtonClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClear.Name = "toolStripButtonClear";
            this.toolStripButtonClear.Size = new System.Drawing.Size(52, 21);
            this.toolStripButtonClear.Text = "Clear";
            this.toolStripButtonClear.Click += new System.EventHandler(this.toolStripButtonClear_Click);
            // 
            // propertyGridItem
            // 
            this.propertyGridItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridItem.HelpVisible = false;
            this.propertyGridItem.Location = new System.Drawing.Point(0, 13);
            this.propertyGridItem.Name = "propertyGridItem";
            this.propertyGridItem.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridItem.Size = new System.Drawing.Size(1261, 134);
            this.propertyGridItem.TabIndex = 5;
            this.propertyGridItem.ToolbarVisible = false;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "dot.png");
            this.imageList.Images.SetKeyName(1, "application_connection.png");
            this.imageList.Images.SetKeyName(2, "ERROR.PNG");
            this.imageList.Images.SetKeyName(3, "navigate_right.png");
            this.imageList.Images.SetKeyName(4, "navigate_left.png");
            this.imageList.Images.SetKeyName(5, "WARNING.PNG");
            this.imageList.Images.SetKeyName(6, "CONSOLE.PNG");
            // 
            // labelSelected
            // 
            this.labelSelected.AutoSize = true;
            this.labelSelected.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelSelected.Location = new System.Drawing.Point(0, 0);
            this.labelSelected.Name = "labelSelected";
            this.labelSelected.Size = new System.Drawing.Size(107, 13);
            this.labelSelected.TabIndex = 17;
            this.labelSelected.Text = "Selected Item Details";
            // 
            // panelSelected
            // 
            this.panelSelected.Controls.Add(this.propertyGridItem);
            this.panelSelected.Controls.Add(this.labelSelected);
            this.panelSelected.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSelected.Location = new System.Drawing.Point(0, 528);
            this.panelSelected.Name = "panelSelected";
            this.panelSelected.Size = new System.Drawing.Size(1261, 147);
            this.panelSelected.TabIndex = 18;
            // 
            // timerUpdate
            // 
            this.timerUpdate.Interval = 500;
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // contextMenuStripItemsList
            // 
            this.contextMenuStripItemsList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.markAllToolStripMenuItem,
            this.showAllToolStripMenuItem});
            this.contextMenuStripItemsList.Name = "contextMenuStripItemsList";
            this.contextMenuStripItemsList.Size = new System.Drawing.Size(126, 48);
            // 
            // markAllToolStripMenuItem
            // 
            this.markAllToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ofThisMethodToolStripMenuItem,
            this.ofThisClassToolStripMenuItem,
            this.ofThisModuleToolStripMenuItem});
            this.markAllToolStripMenuItem.Name = "markAllToolStripMenuItem";
            this.markAllToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.markAllToolStripMenuItem.Text = "Mark All";
            // 
            // ofThisMethodToolStripMenuItem
            // 
            this.ofThisMethodToolStripMenuItem.Name = "ofThisMethodToolStripMenuItem";
            this.ofThisMethodToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.ofThisMethodToolStripMenuItem.Text = "of this Method";
            this.ofThisMethodToolStripMenuItem.Click += new System.EventHandler(this.markAllFromThisMethodToolStripMenuItem_Click);
            // 
            // ofThisClassToolStripMenuItem
            // 
            this.ofThisClassToolStripMenuItem.Name = "ofThisClassToolStripMenuItem";
            this.ofThisClassToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.ofThisClassToolStripMenuItem.Text = "of this Class";
            this.ofThisClassToolStripMenuItem.Click += new System.EventHandler(this.markAllFromThisClassToolStripMenuItem_Click);
            // 
            // ofThisModuleToolStripMenuItem
            // 
            this.ofThisModuleToolStripMenuItem.Name = "ofThisModuleToolStripMenuItem";
            this.ofThisModuleToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.ofThisModuleToolStripMenuItem.Text = "of this Module";
            this.ofThisModuleToolStripMenuItem.Click += new System.EventHandler(this.markAllFromThisModuleToolStripMenuItem_Click);
            // 
            // showAllToolStripMenuItem
            // 
            this.showAllToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ofThisMethodToolStripMenuItem1,
            this.ofThieClassToolStripMenuItem,
            this.ofThisModuleToolStripMenuItem1});
            this.showAllToolStripMenuItem.Name = "showAllToolStripMenuItem";
            this.showAllToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.showAllToolStripMenuItem.Text = "Show All";
            // 
            // ofThisMethodToolStripMenuItem1
            // 
            this.ofThisMethodToolStripMenuItem1.Name = "ofThisMethodToolStripMenuItem1";
            this.ofThisMethodToolStripMenuItem1.Size = new System.Drawing.Size(154, 22);
            this.ofThisMethodToolStripMenuItem1.Text = "of this Method";
            this.ofThisMethodToolStripMenuItem1.Click += new System.EventHandler(this.ofThisMethodToolStripMenuItem1_Click);
            // 
            // ofThieClassToolStripMenuItem
            // 
            this.ofThieClassToolStripMenuItem.Name = "ofThieClassToolStripMenuItem";
            this.ofThieClassToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.ofThieClassToolStripMenuItem.Text = "of this Class";
            this.ofThieClassToolStripMenuItem.Click += new System.EventHandler(this.ofThisClassToolStripMenuItem_Click);
            // 
            // ofThisModuleToolStripMenuItem1
            // 
            this.ofThisModuleToolStripMenuItem1.Name = "ofThisModuleToolStripMenuItem1";
            this.ofThisModuleToolStripMenuItem1.Size = new System.Drawing.Size(154, 22);
            this.ofThisModuleToolStripMenuItem1.Text = "of this Module";
            this.ofThisModuleToolStripMenuItem1.Click += new System.EventHandler(this.ofThisModuleToolStripMenuItem1_Click);
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.methodTracerFilterControl1);
            this.panelLeft.Controls.Add(this.splitter2);
            this.panelLeft.Controls.Add(this.typeTracerFilterControl1);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 26);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(231, 494);
            this.panelLeft.TabIndex = 20;
            // 
            // methodTracerFilterControl1
            // 
            this.methodTracerFilterControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.methodTracerFilterControl1.Filter = null;
            this.methodTracerFilterControl1.Location = new System.Drawing.Point(0, 175);
            this.methodTracerFilterControl1.Name = "methodTracerFilterControl1";
            this.methodTracerFilterControl1.Size = new System.Drawing.Size(231, 319);
            this.methodTracerFilterControl1.TabIndex = 14;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter2.Location = new System.Drawing.Point(0, 169);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(231, 6);
            this.splitter2.TabIndex = 20;
            this.splitter2.TabStop = false;
            // 
            // typeTracerFilterControl1
            // 
            this.typeTracerFilterControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.typeTracerFilterControl1.Filter = null;
            this.typeTracerFilterControl1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.typeTracerFilterControl1.Location = new System.Drawing.Point(0, 0);
            this.typeTracerFilterControl1.Name = "typeTracerFilterControl1";
            this.typeTracerFilterControl1.Size = new System.Drawing.Size(231, 169);
            this.typeTracerFilterControl1.TabIndex = 19;
            // 
            // panelItemMessage
            // 
            this.panelItemMessage.Controls.Add(this.textBoxSelectedItemMessage);
            this.panelItemMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelItemMessage.Location = new System.Drawing.Point(239, 476);
            this.panelItemMessage.Name = "panelItemMessage";
            this.panelItemMessage.Size = new System.Drawing.Size(1022, 44);
            this.panelItemMessage.TabIndex = 22;
            // 
            // textBoxSelectedItemMessage
            // 
            this.textBoxSelectedItemMessage.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxSelectedItemMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxSelectedItemMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSelectedItemMessage.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxSelectedItemMessage.Location = new System.Drawing.Point(0, 0);
            this.textBoxSelectedItemMessage.Multiline = true;
            this.textBoxSelectedItemMessage.Name = "textBoxSelectedItemMessage";
            this.textBoxSelectedItemMessage.ReadOnly = true;
            this.textBoxSelectedItemMessage.Size = new System.Drawing.Size(1022, 44);
            this.textBoxSelectedItemMessage.TabIndex = 1;
            this.textBoxSelectedItemMessage.Text = "Selected item details.";
            // 
            // splitterItemMessage
            // 
            this.splitterItemMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitterItemMessage.Location = new System.Drawing.Point(239, 472);
            this.splitterItemMessage.Name = "splitterItemMessage";
            this.splitterItemMessage.Size = new System.Drawing.Size(1022, 4);
            this.splitterItemMessage.TabIndex = 23;
            this.splitterItemMessage.TabStop = false;
            // 
            // listView
            // 
            this.listView.AutoScroll = true;
            this.listView.AutoScrollSlack = 3;
            this.listView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnGeneral,
            this.columnMessage});
            this.listView.ContextMenuStrip = this.contextMenuStripItemsList;
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listView.FullRowSelect = true;
            this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(239, 26);
            this.listView.Name = "listView";
            this.listView.ShowGroups = false;
            this.listView.Size = new System.Drawing.Size(1022, 446);
            this.listView.SmallImageList = this.imageList;
            this.listView.TabIndex = 15;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.VirtualMode = true;
            this.listView.SelectedIndexChanged += new System.EventHandler(this.listViewMain_SelectedIndexChanged);
            this.listView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listView_RetrieveVirtualItem);
            // 
            // columnGeneral
            // 
            this.columnGeneral.Text = "";
            // 
            // columnMessage
            // 
            this.columnMessage.Text = "Message";
            // 
            // splitter3
            // 
            this.splitter3.Location = new System.Drawing.Point(231, 26);
            this.splitter3.Name = "splitter3";
            this.splitter3.Size = new System.Drawing.Size(8, 494);
            this.splitter3.TabIndex = 21;
            this.splitter3.TabStop = false;
            // 
            // splitterDetails
            // 
            this.splitterDetails.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitterDetails.Location = new System.Drawing.Point(0, 520);
            this.splitterDetails.Name = "splitterDetails";
            this.splitterDetails.Size = new System.Drawing.Size(1261, 8);
            this.splitterDetails.TabIndex = 16;
            this.splitterDetails.TabStop = false;
            // 
            // TracerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView);
            this.Controls.Add(this.splitterItemMessage);
            this.Controls.Add(this.panelItemMessage);
            this.Controls.Add(this.splitter3);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.splitterDetails);
            this.Controls.Add(this.panelSelected);
            this.DoubleBuffered = true;
            this.Name = "TracerControl";
            this.Size = new System.Drawing.Size(1261, 675);
            this.Load += new System.EventHandler(this.TracerControl_Load);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.panelSelected.ResumeLayout(false);
            this.panelSelected.PerformLayout();
            this.contextMenuStripItemsList.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelItemMessage.ResumeLayout(false);
            this.panelItemMessage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonRefresh;
        private System.Windows.Forms.ToolStripButton toolStripButtonClear;
        private System.Windows.Forms.PropertyGrid propertyGridItem;
        private MethodTracerFilterControl methodTracerFilterControl1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonSearchClear;
        private System.Windows.Forms.ToolStripButton toolStripButtonSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonMark;
        private System.Windows.Forms.ImageList imageList;
        private VirtualListViewEx listView;
        private ColumnHeader columnGeneral;
        private ColumnHeader columnMessage;
        private System.Windows.Forms.Label labelSelected;
        private System.Windows.Forms.Panel panelSelected;
        private ToolStripButton toolStripButtonAutoUpdate;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButtonAutoScroll;
        private Timer timerUpdate;
        private ToolStripButton toolStripButtonEnabled;
        private ContextMenuStrip contextMenuStripItemsList;
        private ToolStripMenuItem markAllToolStripMenuItem;
        private ToolStripMenuItem ofThisMethodToolStripMenuItem;
        private ToolStripMenuItem ofThisClassToolStripMenuItem;
        private ToolStripMenuItem ofThisModuleToolStripMenuItem;
        private ToolStripMenuItem showAllToolStripMenuItem;
        private ToolStripMenuItem ofThisMethodToolStripMenuItem1;
        private ToolStripMenuItem ofThieClassToolStripMenuItem;
        private ToolStripMenuItem ofThisModuleToolStripMenuItem1;
        private TypeTracerFilterControl typeTracerFilterControl1;
        private ToolStripButton toolStripButtonDetails;
        private SplitterEx splitter2;
        private SplitterEx splitter3;
        private SplitterEx splitterDetails;
        private Panel panelItemMessage;
        private TextBox textBoxSelectedItemMessage;
        private Splitter splitterItemMessage;
        public Panel panelLeft;
        private ToolStripTextBox toolStripTextBoxExclude;
        private ToolStripButton toolStripButtonExclude;
        private ToolStripButton toolStripButtonClearExclude;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripTextBox toolStripTextBoxMark;
        private ToolStripButton toolStripButtonClearMark;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripDropDownButton toolStripComboBoxPriority;
    }
}
