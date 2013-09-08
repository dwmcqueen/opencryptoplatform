using System;
using System.Collections.Generic;
using System.Text;
using Arbiter;
using CommonFinancial;

namespace ForexPlatform
{
    /// <summary>
    /// Operation responce requestMessage, that is related to an operation on a given sessionInformation.
    /// </summary>
    [Serializable]
    public class DataSessionResponceMessage : ResponceMessage
    {
        DataSessionInfo _sessionInfo;
        public DataSessionInfo SessionInfo
        {
            get { return _sessionInfo; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataSessionResponceMessage(DataSessionInfo sessionInfo, bool operationResult)
            : base(operationResult)
        {
            _sessionInfo = sessionInfo;
        }

    }
}
