using System.Windows.Forms;
using ForexPlatform;
using System;

namespace ForexPlatformFrontEnd
{

    public partial class RemoteExpertHostForm : Form
    {
        //RemoteExpertHost _expertHost;

        /// <summary>
        /// 
        /// </summary>
        public RemoteExpertHostForm(Uri platformUri, Type expertType, string expertName)
        {
            InitializeComponent();

            //_expertHost = new RemoteExpertHost(platformUri, expertType, expertName);

        }

        private void RemoteExpertHostForm_Load(object sender, System.EventArgs e)
        {
            //this.Text = (this.Tag as string).Replace("{0}", _expertHost.ExpertName);
        }
    }
}
