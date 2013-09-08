using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace CommonSupport
{
    public partial class NewsSourceSettingsControl : UserControl
    {
        NewsSource _source;

        /// <summary>
        /// 
        /// </summary>
        public NewsSourceSettingsControl()
        {
            InitializeComponent();
        }

        public NewsSourceSettingsControl(NewsSource source)
        {
            InitializeComponent();

            _source = source;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            listViewFeedChannels.Items.Clear();
            foreach (string channelName in _source.ChannelsNames)
            {
                ListViewItem item = listViewFeedChannels.Items.Add(channelName);
                item.Checked = _source.IsChannelEnabled(channelName);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listViewFeedChannels.Items.Count; i++)
            {
                if (listViewFeedChannels.Items[i].Checked != _source.IsChannelEnabled(_source.ChannelsNames[i]))
                {
                    _source.SetChannelEnabled(_source.ChannelsNames[i], listViewFeedChannels.Items[i].Checked);
                }
            }

            this.Hide();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }


    }
}
