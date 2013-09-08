using System;
using System.Collections.Generic;
using System.Text;
using Arbiter;
using CommonFinancial;

namespace ForexPlatform
{
    /// <summary>
    /// Base class responce for an operation on an account.
    /// </summary>
    [Serializable]
    public class AccountResponceMessage : ResponceMessage
    {
        AccountInfo _accountInfo = AccountInfo.Empty;

        public AccountInfo AccountInfo
        {
            get { return _accountInfo; }
            set { _accountInfo = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public AccountResponceMessage(AccountInfo accountInfo, bool operationResult)
            : base(operationResult)
        {
            _accountInfo = accountInfo;
        }
   }
}
