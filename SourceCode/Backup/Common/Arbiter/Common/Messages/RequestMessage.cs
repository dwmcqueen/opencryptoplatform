using System;
using System.Collections.Generic;
using System.Text;

namespace Arbiter
{
    /// <summary>
    /// Base class for general request messages.
    /// </summary>
    [Serializable]
    public class RequestMessage : TransportMessage
    {
        private bool _performSynchronous = false;
        /// <summary>
        /// Optional, not applicable to all requests,
        /// should the request be performed synchronously.
        /// </summary>
        public bool PerformSynchronous
        {
            get { return _performSynchronous; }
            set { _performSynchronous = value; }
        }

        protected bool _requestResponce = true;
        public virtual bool RequestResponce
        {
            get { return _requestResponce; }
            set { _requestResponce = value; }
        }
    }
}
