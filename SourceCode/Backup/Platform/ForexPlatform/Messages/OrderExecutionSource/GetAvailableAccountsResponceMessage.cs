﻿using System;
using System.Collections.Generic;
using System.Text;
using Arbiter;
using CommonFinancial;

namespace ForexPlatform
{
    /// <summary>
    /// Message allows to obtain currently available accountInfos.
    /// </summary>
    [Serializable]
    public class GetAvailableAccountsResponceMessage : ResponceMessage
    {
        AccountInfo[] _accounts;

        public AccountInfo[] Accounts
        {
            get { return _accounts; }
        }

        /// <summary>
        /// 
        /// </summary>
        public GetAvailableAccountsResponceMessage(AccountInfo[] accounts, bool operationResult)
            : base(operationResult)
        {
            _accounts = accounts;
        }
    }
}
