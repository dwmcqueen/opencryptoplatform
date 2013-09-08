using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Timers;
using Rss;
using System.Net;
using System.IO;
using System.Runtime.Serialization;

namespace CommonSupport
{
    /// <summary>
    /// Manages news delivery. Implements custom serialization procedure.
    /// Implements custom serialization routine.
    /// </summary>
    [Serializable]
    public class NewsManager : ISerializable, IDisposable
    {
        Timer _updateTimer = new Timer();

        /// <summary>
        /// 
        /// </summary>
        public bool AutoUpdateEnabled
        {
            get { return _updateTimer.Enabled; }
            set { _updateTimer.Enabled = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan AutoUpdateInterval
        {
            get 
            {
                return TimeSpan.FromMilliseconds(_updateTimer.Interval);
            }

            set
            {
                _updateTimer.Interval = value.TotalMilliseconds;
            }
        }

        volatile bool _isUpdating = false;
        public bool IsUpdating
        {
            get { return _isUpdating; }
        }

        List<NewsSource> _newsSources = new List<NewsSource>();
        public ReadOnlyCollection<NewsSource> NewsSourcesUnsafe
        {
            get { lock (this) { return _newsSources.AsReadOnly(); } }
        }

        public NewsSource[] NewsSourcesArray
        {
            get { lock (this) { return _newsSources.ToArray(); } }
        }

        public delegate void GeneralUpdateDelegate(NewsManager manager);
        
        public delegate void NewsSourceUpdateDelegate(NewsManager manager, NewsSource source);
        
        public event NewsSourceUpdateDelegate SourceAddedEvent;
        public event NewsSourceUpdateDelegate SourceRemovedEvent;

        public event GeneralUpdateDelegate UpdatingStartedEvent;
        public event GeneralUpdateDelegate UpdatingFinishedEvent;

        /// <summary>
        /// 
        /// </summary>
        public NewsManager()
        {
            // Update every 2 minutes.
            _updateTimer.Interval = 1000 * 60 * 2;
            _updateTimer.Enabled = true;
            _updateTimer.Elapsed += new ElapsedEventHandler(_updateTimer_Elapsed);
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        public NewsManager(SerializationInfo info, StreamingContext context)
        {
            _updateTimer.Interval = info.GetInt64("timerInterval");
            _updateTimer.Enabled = info.GetBoolean("timerEnabled");

            _updateTimer.Elapsed += new ElapsedEventHandler(_updateTimer_Elapsed);
        }

        public virtual void Dispose()
        {
        }

        #region ISerializable Members

        /// <summary>
        /// Serialization baseMethod.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("timerInterval", _updateTimer.Interval);
            info.AddValue("timerEnabled", _updateTimer.Enabled);
        }

        #endregion

        void _updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_isUpdating)
            {// Already updating.
                return;
            }
            UpdateFeeds();
        }

        public virtual bool AddSource(NewsSource source)
        {
            lock (this)
            {
                if (_newsSources.Contains(source) || string.IsNullOrEmpty(source.Address))
                {// Already contained or invalid address.
                    return false;
                }

                foreach (NewsSource iteratedSource in _newsSources)
                {
                    if (iteratedSource.Address == source.Address)
                    {// A source with this address already exists.
                        return false;
                    }
                }

                _newsSources.Add(source);
                source.EnabledChangedEvent += new NewsSource.EnabledChangedDelegate(source_EnabledChangedEvent);
            }

            if (SourceAddedEvent != null)
            {
                SourceAddedEvent(this, source);
            }

            return true;
        }

        protected virtual void source_EnabledChangedEvent(NewsSource source)
        {
        }

        public virtual bool RemoveSource(NewsSource source)
        {
            lock (this)
            {
                if (_newsSources.Remove(source) == false)
                {// Not found.
                    return false;
                }
                source.EnabledChangedEvent -= new NewsSource.EnabledChangedDelegate(source_EnabledChangedEvent);
            }

            if (SourceRemovedEvent != null)
            {
                SourceRemovedEvent(this, source);
            }

            return true;
        }

        public void UpdateFeeds()
        {
            NewsSource[] sources;
            lock (this)
            {
                if (_isUpdating)
                {// Already updating.
                    return;
                }
                sources = _newsSources.ToArray();
                _isUpdating = true;
            }

            if (UpdatingStartedEvent != null)
            {
                UpdatingStartedEvent(this);
            }

            foreach (NewsSource source in sources)
            {
                try
                {
                    source.Update();
                }
                catch (Exception ex)
                {
                    SystemMonitor.OperationWarning("Failed to update news source ["+ source.Name +", " + ex.Message +"].");
                }
            }

            lock (this)
            {
                _isUpdating = false;
            }

            if (UpdatingFinishedEvent != null)
            {
                UpdatingFinishedEvent(this);
            }
        }

    }
}
