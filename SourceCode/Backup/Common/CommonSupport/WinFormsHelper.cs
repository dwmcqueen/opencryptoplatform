using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CommonSupport
{
    /// <summary>
    /// Helper class handles common operations related to windows forms.
    /// </summary>
    public static class WinFormsHelper
    {
        static InvokeWatchDog _invokeWatchDog = new InvokeWatchDog();

        /// <summary>
        /// 
        /// </summary>
        public struct AsyncResultInfo
        {
            public string ControlName;
            public string MethodName;
            
            public IAsyncResult AsyncResult;
            public DateTime PublishTime;
            
            /// <summary>
            /// 
            /// </summary>
            public AsyncResultInfo(IAsyncResult asyncResult, Delegate d, Control control)
            {
                AsyncResult = asyncResult;
                PublishTime = DateTime.Now;

                MethodName = d.Method.Name;
                ControlName = control.GetType().Name + ";" + control.Name;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        class MethodInvocationInformation
        {
            DateTime _lastInvocation = DateTime.MinValue;

            volatile bool _isCallPending = false;

            public bool IsCallCompleted
            {
                get
                {
                    if (_currentExecutionResult != null && _currentExecutionResult.IsCompleted == false)
                    {
                        return false;
                    }

                    return true;
                }
            }

            object[] _lastInvocationParameters = null;

            TimeSpan _minimumCallInterval = TimeSpan.MaxValue;

            Control _control = null;

            Delegate _delegate = null;

            IAsyncResult _currentExecutionResult = null;

            /// <summary>
            /// 
            /// </summary>
            protected bool LastInvocationTimedOut
            {
                get
                {
                    return (DateTime.Now - _lastInvocation) >= _minimumCallInterval;
                }
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            public MethodInvocationInformation(Control control, Delegate d)
            {
                _control = control;
                _delegate = d;
            }
            
            /// <summary>
            /// Invoked by the timer based invocation monitor, allows to execute pending calls, after a time interval has passed.
            /// </summary>
            public void CheckCall()
            {
                lock (this)
                {
                    if (_isCallPending == false || LastInvocationTimedOut == false)
                    {// No call pending, or last invocation too soon.
                        return;
                    }
                }

                Invoke(TimeSpan.MaxValue, _lastInvocationParameters);
            }

            /// <summary>
            /// Submit an invoke request, if currently an invoke is done, this will be put as pending.
            /// </summary>
            public bool Invoke(TimeSpan minimumCallInterval, params object[] parameters)
            {
                lock(this)
                {
                    bool lastInvocationTimedOut = LastInvocationTimedOut;

                    if ((_currentExecutionResult != null && _currentExecutionResult.IsCompleted == false)
                        || (_isCallPending && lastInvocationTimedOut == false))
                    {// Current call in progress, or a call already pending and not time for execution yet.
                        
                        _isCallPending = true;
                        _lastInvocationParameters = parameters;
                        
                        _minimumCallInterval = TimeSpan.FromMilliseconds(Math.Min(minimumCallInterval.TotalMilliseconds, _minimumCallInterval.TotalMilliseconds));
                        
                        return false;
                    }

                    _isCallPending = false;
                    _lastInvocationParameters = null;
                    _lastInvocation = DateTime.Now;
                    // Reset the minimum call interval.
                    _minimumCallInterval = TimeSpan.MaxValue;
                    _currentExecutionResult = WinFormsHelper.BeginManagedInvoke(_control, _delegate, parameters);
                    //System.Diagnostics.Trace.WriteLine(_control.Name, _delegate.Method.Name);
                }


                return true;
            }
        }

        static Dictionary<Control, Dictionary<MethodInfo, MethodInvocationInformation>> _filteredInvokes = new Dictionary<Control, Dictionary<MethodInfo, MethodInvocationInformation>>();

        static System.Timers.Timer _periodicInvokeTimer = new System.Timers.Timer();

        /// <summary>
        /// Explicit static constructor to tell C# compiler not to mark type 
        /// as BeforeFieldInit. Required for thread safety of static elements.
        /// </summary>
        static WinFormsHelper()
        {
            _periodicInvokeTimer.AutoReset = false;
            _periodicInvokeTimer.Interval = 100;
            _periodicInvokeTimer.Elapsed += new System.Timers.ElapsedEventHandler(_periodicInvokeTimer_Elapsed);
            _periodicInvokeTimer.Start();
        }

        static void _periodicInvokeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_filteredInvokes)
            {
                foreach (KeyValuePair<Control, Dictionary<MethodInfo, MethodInvocationInformation>> pair in _filteredInvokes)
                {
                    foreach (KeyValuePair<MethodInfo, MethodInvocationInformation> subPair in pair.Value)
                    {
                        subPair.Value.CheckCall();
                    }
                }
            }

            _periodicInvokeTimer.Start();
        }

        /// <summary>
        /// Since the filtered invoke mechanism keeps references to controls that were invoked,
        /// this method allows you to clear any left over ones, that are not still executing.
        /// </summary>
        public static void CleanUpFilteredInvokesReferences()
        {
            lock (_filteredInvokes)
            {
                List<Control> removingControls = new List<Control>();

                foreach (KeyValuePair<Control, Dictionary<MethodInfo, MethodInvocationInformation>> pair in _filteredInvokes)
                {
                    bool activeOperationFound = false;
                    foreach (KeyValuePair<MethodInfo, MethodInvocationInformation> subPair in pair.Value)
                    {
                        if (subPair.Value.IsCallCompleted == false)
                        {
                            activeOperationFound = true;
                            break;
                        }
                    }

                    if (activeOperationFound == false)
                    {// Control cleared for removing.
                        removingControls.Add(pair.Key);
                    }
                }

                foreach (Control control in removingControls)
                {
                    _filteredInvokes.Remove(control);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color GetBrushBasicColor(Brush brush, Color defaultColor)
        {
            if (brush is SolidBrush)
            {
                return ((SolidBrush)brush).Color;
            }
            else if (brush is LinearGradientBrush)
            {
                return ((LinearGradientBrush)brush).LinearColors[0];
            }

            return defaultColor;
        }

        /// <summary>
        /// Helper, moves all toolstrip items from one toolstrip to another.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void MoveToolStripItems(ToolStrip source, ToolStrip destination)
        {
            foreach (ToolStripItem item in GeneralHelper.EnumerableToList<ToolStripItem>(source.Items))
            {
                destination.Items.Add(item);
            }
        }

        /// <summary>
        /// Activates the invoke watch dog mechanism; use in cases of failing or missin Invoke() calls.
        /// </summary>
        public static void ActivateInvokeWatchDog()
        {
            _invokeWatchDog.Start();
        }

        /// <summary>
        /// Helper.
        /// </summary>
        public static bool BeginFilteredManagedInvoke(Control control, TimeSpan minimumInvocationInterval, GeneralHelper.DefaultDelegate d)
        {
            return BeginFilteredManagedInvoke(control, minimumInvocationInterval, (Delegate)d);
        }

        /// <summary>
        /// Helper.
        /// </summary>
        /// <param name="assurePending">Makes sure at least one call is still pending on the call.</param>
        public static bool BeginFilteredManagedInvoke(Control control, GeneralHelper.DefaultDelegate d)
        {
            return BeginFilteredManagedInvoke(control, TimeSpan.FromMilliseconds(250), (Delegate)d);
        }

        /// <summary>
        /// Helper.
        /// </summary>
        public static bool BeginFilteredManagedInvoke(Control control, Delegate d, params object[] args)
        {
            return BeginFilteredManagedInvoke(control, TimeSpan.FromMilliseconds(250), (Delegate)d, args);
        }

        /// <summary>
        /// Filtered invokes make sure only one instance of the given method call is in action at the given moment.
        /// This is usefull for calls that are made very many times, but we wish to allow the control to only
        /// process as much as it can in real time (not delaying any calls for later).
        /// Filtering is done on a method AND instance basis, so if you have 2 instances of same class alive,
        /// they will not interupt each others calls and each of them will receive what is due.
        /// </summary>
        /// <param name="minimumInterInvocationInterval">The interval between two consecutive invokes of the method; pass null for default; TimeSpan.Zero for immediate (based on a 100ms timer, so delay up to 100ms).</param>
        /// <returns>Result shows if the given invoke was placed, or if there is another one pending executing and this one was dropped.</returns>
        public static bool BeginFilteredManagedInvoke(Control control, TimeSpan minimumInterInvocationInterval, Delegate d, params object[] args)
        {
            MethodInvocationInformation information = null;
            
            // Obtain the corresponding invocation information item.
            lock (_filteredInvokes)
            {
                if (_filteredInvokes.ContainsKey(control) == false)
                {
                    _filteredInvokes.Add(control, new Dictionary<MethodInfo, MethodInvocationInformation>());
                }

                Dictionary<MethodInfo, MethodInvocationInformation> dictionary = _filteredInvokes[control];

                if (dictionary.ContainsKey(d.Method) == false)
                {
                    information = new MethodInvocationInformation(control, d);
                    dictionary.Add(d.Method, information);
                }
                else
                {
                    information = dictionary[d.Method];
                }
            }

            return information.Invoke(minimumInterInvocationInterval, args);
        }

        /// <summary>
        /// Will perform managed invoke if needed, or a direct one if not.
        /// </summary>
        public static void DirectOrManagedInvoke(Control invocationControl, GeneralHelper.DefaultDelegate d)
        {
            if (invocationControl.InvokeRequired)
            {
                BeginManagedInvoke(invocationControl, d);
            }
            else
            {
                d.Invoke();
            }
        }


        /// <summary>
        /// Helper.
        /// </summary>
        public static IAsyncResult BeginManagedInvoke(Control invocationControl, GeneralHelper.DefaultDelegate d)
        {
            return BeginManagedInvoke(invocationControl, (Delegate)d);
        }

        /// <summary>
        /// Helper, automates invocation on a control; also supports a monitoring mechanism agains blocking invokes.
        /// </summary>
        public static IAsyncResult BeginManagedInvoke(Control invocationControl, Delegate d, params object[] args)
        {
            //Trace.WriteLine(invocationControl.Name + "." + d.Method.Name + "." + args.Length);

            if (invocationControl.IsHandleCreated
                && invocationControl.IsDisposed == false
                && invocationControl.Disposing == false)
            {
                IAsyncResult result = invocationControl.BeginInvoke(d, args);
                _invokeWatchDog.Add(new AsyncResultInfo(result, d, invocationControl));
                return result;
            }

            return null;
        }


    }
}
