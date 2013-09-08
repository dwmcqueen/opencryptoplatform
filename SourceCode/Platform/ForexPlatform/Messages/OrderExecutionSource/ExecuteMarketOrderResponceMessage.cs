using System;
using System.Collections.Generic;
using System.Text;
using Arbiter;
using CommonFinancial;
using CommonSupport;

namespace ForexPlatform
{
    /// <summary>
    /// Message send as a responce to Open ActiveOrder request.
    /// </summary>
    [Serializable]
    public class ExecuteMarketOrderResponceMessage : AccountResponceMessage
    {
        OrderInfo? _info = null;
        public OrderInfo? Info
        {
            get { return _info; }
            set { _info = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ExecuteMarketOrderResponceMessage(AccountInfo sessionInfo, OrderInfo? orderInfo, bool operationResult)
            : base(sessionInfo, operationResult)
        {
            _info = orderInfo;
        }
    }
}
