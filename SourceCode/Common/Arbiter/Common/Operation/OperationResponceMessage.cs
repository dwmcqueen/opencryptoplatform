using System;
using System.Collections.Generic;
using System.Text;
using Arbiter;

namespace Arbiter
{
    /// <summary>
    /// Base class for operation responce messages.
    /// </summary>
    [Serializable]
    public class OperationResponceMessage : ResponceMessage
    {
        int _operationID;
        public int OperationID
        {
            get { return _operationID; }
        }

        /// <summary>
        /// 
        /// </summary>
        public OperationResponceMessage(int operationID, bool operationResult)
            : base(operationResult)
        {
            _operationID = operationID;
        }
    }
}
