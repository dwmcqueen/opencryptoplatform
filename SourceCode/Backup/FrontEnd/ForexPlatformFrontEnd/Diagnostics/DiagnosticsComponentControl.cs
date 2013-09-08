using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CommonSupport;
using ForexPlatform;

namespace ForexPlatformFrontEnd
{
    /// <summary>
    /// UI control for the platform diagnostics component.
    /// </summary>
    public partial class DiagnosticsComponentControl : PlatformComponentControl
    {
        TracerStatusStripOperator _statusStripOperator = new TracerStatusStripOperator();

        public Tracer Tracer
        {
            get { return TracerHelper.Tracer; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DiagnosticsComponentControl() 
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public DiagnosticsComponentControl(PlatformDiagnostics diagnostics)
            : base(diagnostics)
        {
            InitializeComponent();
            tracerControl1.Load += new EventHandler(tracerControl1_Load);
         
            // This is needed to spike an event and assign naming properly; it is specific to this control,
            // since it does not wish to generate a Layout in its InitializeComponent() method.
            PerformLayout();
        }

        void tracerControl1_Load(object sender, EventArgs e)
        {
            SplitterEx splitter = new SplitterEx();

            tracerControl1.panelLeft.Controls.Add(splitter);
            splitter.Dock = DockStyle.Bottom;
            splitter.Height = 8;

            tracerControl1.panelLeft.Controls.Add(applicationDiagnosticsInformationControl1);
            applicationDiagnosticsInformationControl1.Dock = DockStyle.Bottom;
        }

        private void DiagnosticsControl_Load(object sender, EventArgs e)
        {
            tracerControl1.Tracer = Tracer;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SetApplicationStatusStrip(StatusStrip strip)
        {
            base.SetApplicationStatusStrip(strip);

            if (strip != null)
            {
                _statusStripOperator.Load(this, this.timerUI, Tracer, strip);
            }
            else
            {
                _statusStripOperator.UnLoad();
            }
        }

    }
}
