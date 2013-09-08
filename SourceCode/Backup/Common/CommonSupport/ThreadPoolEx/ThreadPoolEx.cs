using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace CommonSupport
{
    /// <summary>
    /// Class implements a custom thread pool.
    /// </summary>
    public class ThreadPoolEx : IDisposable
    {
        static List<Type> OwnerTypes = new List<Type>(new Type[] { typeof(ThreadPoolEx) });

        /// <summary>
        /// Internal data storage class - for a queued thread entity.
        /// </summary>
        class TargetInfo
        {
            internal TargetInfo(string invokerName, Delegate d, object[] args)
            {
                InvokerName = invokerName;
                Target = d;
                Args = args;
            }

            internal readonly string InvokerName = string.Empty;
            internal readonly Delegate Target;
            internal readonly object[] Args;
        }
        
        /// <summary>
        /// Internal data storage class - for a running thread.
        /// </summary>
        class ThreadInfo
        {
            internal ManualResetEvent Event = new ManualResetEvent(false);
            internal volatile bool Activated = false;
        }

        volatile string _name = string.Empty;
        /// <summary>
        /// Name of this thread pool.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Running threads.
        /// </summary>
        volatile int _maximumSimultaniouslyRunningThreadsAllowed = 25;
        public int MaximumSimultaniouslyRunningThreadsAllowed
        {
            get { return _maximumSimultaniouslyRunningThreadsAllowed; }
            set { _maximumSimultaniouslyRunningThreadsAllowed = value; }
        }

        /// <summary>
        /// Total threads (running, sleeping, suspended, etc.)
        /// </summary>
        volatile int _maximumTotalThreadsAllowed = 50;
        public int MaximumTotalThreadsAllowed
        {
            get { return _maximumTotalThreadsAllowed; }
            set { _maximumTotalThreadsAllowed = value; }
        }

        volatile ApartmentState _threadsApartmentState = ApartmentState.STA;
        public ApartmentState ThreadsApartmentState
        {
            get { return _threadsApartmentState; }
            set { _threadsApartmentState = value; }
        }

        Dictionary<Thread, ThreadInfo> _threads = new Dictionary<Thread, ThreadInfo>();

        List<TargetInfo> _queue = new List<TargetInfo>();

        volatile int _activeRunningThreadsCount = 0;
        /// <summary>
        /// Number of currently actively running threads.
        /// </summary>
        public int ActiveRunningThreadsCount
        {
            get { return _activeRunningThreadsCount; }
        }

        /// <summary>
        /// Number of thread slots available.
        /// </summary>
        public int FreeThreadsCount
        {
            get
            {
                lock (_threads)
                {
                    return Math.Max(0, _maximumSimultaniouslyRunningThreadsAllowed - _activeRunningThreadsCount);
                }
            }
        }

        public int QueuedItemsCount
        {
            get { return _queue.Count; }
        }

        delegate void InvokeDelegate(TargetInfo info);

        volatile bool _running = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of this thread pool</param>
        public ThreadPoolEx(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Waits for a period of time (25sec) and aborts all living threads, if any are left.
        /// </summary>
        public void Dispose()
        {
            _running = false;
            lock (_threads)
            {// Wake up all sleeping threads.
                foreach (ThreadInfo info in _threads.Values)
                {
                    info.Event.Set();
                }

                Stopwatch totalWaitStopwatch = new Stopwatch();
                totalWaitStopwatch.Start();

                while (_threads.Count > 0)
                {
                    Dictionary<Thread, ThreadInfo>.KeyCollection.Enumerator enumerator = _threads.Keys.GetEnumerator();

                    if (enumerator.MoveNext() == false)
                    {
                        return;
                    }

                    Thread thread = enumerator.Current;

                    if (thread.ThreadState == System.Threading.ThreadState.Running
                        || thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                    {
                        // Some thread is still working, see if we can wait any further.
                        if (totalWaitStopwatch.Elapsed < TimeSpan.FromSeconds(25))
                        {// Continue waiting for some more time.
                            Thread.Sleep(500);
                            continue;
                        }

                        // Cause an exception in all of the still sleeping threads, so they know to finish now.
                        thread.Abort();
                        Thread.Sleep(500);

                        if (thread.ThreadState == System.Threading.ThreadState.Running
                            || thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                        {// Also try this on the thread.
                            thread.Interrupt();
                        }

                    }

                    _threads.Remove(thread);
                }

                totalWaitStopwatch.Stop();
            }
        }

        /// <summary>
        /// Add a new delegate call to be executed.
        /// </summary>
        public void Queue(Delegate d, params object[] args)
        {
            if (d == null)
            {
                return;
            }

            lock (_queue)
            {
                string callerName = string.Empty;
                if (Debugger.IsAttached)
                {
                    MethodBase method = ReflectionHelper.GetExternalCallingMethod(2, OwnerTypes);
                    if (method != null)
                    {
                        callerName = method.Name;
                    }
                    else
                    {
                        callerName = "NA";
                    }
                }

                TargetInfo info = new TargetInfo(callerName, d, args);
                _queue.Add(info);
            }

            ProcessQueue();
        }

        /// <summary>
        /// Helper, process the items gathered in the execution queue.
        /// </summary>
        void ProcessQueue()
        {
            SystemMonitor.Variables.SetValue(this, this.Name + ".Queue", _queue.Count);
            lock (_queue)
            {
                if (_queue.Count == 0)
                {
                    return;
                }
            }

            SystemMonitor.Variables.SetValue(this, this.Name + ".TotalThreads", _threads.Count);
            SystemMonitor.Variables.SetValue(this, this.Name + ".ActiveThreads", _activeRunningThreadsCount);

            lock (_threads)
            {
                if (MaximumTotalThreadsAllowed <= _threads.Count)
                {
                    SystemMonitor.OperationWarning("[" + _name + "] Too many total threads, suspeding.");
                    return;
                }

                // Keep these inside the lock section.
                if (_activeRunningThreadsCount >= MaximumSimultaniouslyRunningThreadsAllowed)
                {
                    SystemMonitor.OperationWarning("[" + _name + "] Too many running threads, suspeding.");
                    return;
                }

                foreach (ThreadInfo info in _threads.Values)
                {
                    if (info.Activated == false)
                    {// Found a sleeping thread - wake it up and do the job.
                        info.Event.Set();
                        return;
                    }
                }

                // Running threads are below limit and nobody is sleeping, so run a new one.
                Thread newThread = new Thread(new ThreadStart(Execute));
                newThread.SetApartmentState(_threadsApartmentState);
                _threads.Add(newThread, new ThreadInfo() { Activated = true, Event = new ManualResetEvent(false) });
                _activeRunningThreadsCount++;

                newThread.Name = this.GetType().Name;
                newThread.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void Execute()
        {
            while (_running)
            {
                TargetInfo targetInfo;
                lock (_queue)
                {
                    if (_queue.Count == 0)
                    {
                        targetInfo = null;
                    }
                    else
                    {
                        targetInfo = _queue[0];
                        _queue.RemoveAt(0);
                    }
                }

                if (targetInfo != null)
                {// New task found, get to executing it.
                    try
                    {
                        //Thread.CurrentThread.Name = this.GetType().Name + ":" + targetInfo.InvokerName + " >> " + targetInfo.Target.Method.Name;
                        object invokeResult = targetInfo.Target.DynamicInvoke(targetInfo.Args);
                    }
                    catch (Exception ex)
                    {
                        SystemMonitor.OperationWarning("[" + _name + "] Thread executed caused an exception: " + ex.Message, TracerItem.PriorityEnum.VeryHigh);
                    }

                    //Thread.CurrentThread.Name = this.GetType().Name + " Inactive";
                }
                else
                {// No new task found in pending tasks.
                    ThreadInfo threadInfo;
                    lock (_threads)
                    {
                        threadInfo = _threads[Thread.CurrentThread];
                        _activeRunningThreadsCount--;
                        threadInfo.Activated = false;
                    }

                    if (threadInfo.Event.WaitOne(TimeSpan.FromSeconds(4)))
                    {
                        _activeRunningThreadsCount++;
                        threadInfo.Activated = true;
                        threadInfo.Event.Reset();
                    }
                    else
                    {// We waited long enough, no new tasks, so release ourselves.
                        lock (_threads)
                        {
                            _threads.Remove(Thread.CurrentThread);
                        }

                        SystemMonitor.Variables.SetValue(this, this.Name + ".TotalThreads", _threads.Count);
                        SystemMonitor.Variables.SetValue(this, this.Name + ".ActiveThreads", _activeRunningThreadsCount);

                        return;
                    }

                }
            }

            ProcessQueue();
        }
    }  
}
