using System;
using System.Reflection;
using System.Windows.Forms;
using CommonSupport;
using ForexPlatformFrontEnd.Properties;

namespace ForexPlatformFrontEnd
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.ThreadExit += new EventHandler(Application_ThreadExit);

            if (args.Length == 0)
            {
                MessageBox.Show("Use ForexPlatformClient.exe to run the platform.", "Application Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // The application starts in global diagnostics mode by default. When the platform initializes, it restores it setting on that.
            // No major warnings/errors are expected in normal operation before the initialization of the platform.
            SystemMonitor.GlobalDiagnosticsMode = true;

            if (args[0].ToLower() == "ManagedLaunch".ToLower())
            {// Default managed starting procedure.
                try
                {
                    // Single instance mode check.
                    bool createdNew;
                    GeneralHelper.CreateCheckApplicationMutex(Application.ProductName, out createdNew);

                    if (createdNew == false)
                    {
                        if (Settings.Default.SingleInstanceMode)
                        {
                            MessageBox.Show("Application already running and single instance mode set (config file).", Application.ProductName + " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else
                        {
                            if (MessageBox.Show("Another instance of the application is already running, do you wish to continue?", Application.ProductName + " Note", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }

                    // Log file.
                    string logFile = Settings.Default.TraceLogFile;

                    if (string.IsNullOrEmpty(logFile) == false)
                    {
                        TracerHelper.Tracer.Add(new FileTracerItemSink(TracerHelper.Tracer,
                             GeneralHelper.MapRelativeFilePathToExecutingDirectory(logFile)));
                    }

                    if (createdNew == false)
                    {
                        TracerHelper.Trace("Running as second (multiple) instance.");
                    }

                    Form mainForm = new openforexplatformBeta();
                    Application.Run(mainForm);
                }
                catch (Exception ex)
                {
                    SystemMonitor.Error(ex.GetType().Name + "; " + ex.Message);
                }
                finally
                {
                    GeneralHelper.DestroyApplicationMutex();
                }
            }
            else if (args[0].ToLower() == "experthost" && args.Length >= 4)
            {// Start as an expert host.
                    Uri uri = new Uri(args[1]);
                    Type expertType = Type.ReflectionOnlyGetType(args[2], true, true);
                    string expertName = args[3];

                    RemoteExpertHostForm hostForm = new RemoteExpertHostForm(uri, expertType, expertName);
                    Application.Run(hostForm);
            }
            else
            {
                MessageBox.Show("Starting parameters not recognized. Process will not start.", "Error in starting procedure.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            SystemMonitor.Error(e.Exception.Message);
        }

        static void Application_ThreadExit(object sender, EventArgs e)
        {

        }
    }
}