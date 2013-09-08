//using System;
//using System.Collections.Generic;
//using System.Text;
//using Arbiter;
//using CommonFinancial;
//using CommonSupport;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Message contains information on all orders ids on a given source.
//    /// </summary>
//    [Serializable]
//    public class GetOrdersIDsResponceMessage : AccountResponceMessage
//    {
//        string[] _openCustomIDs;
//        public string[] OpenCustomIDs
//        {
//            get { return _openCustomIDs; }
//        }

//        string[] _openTickets;
//        public string[] OpenTickets
//        {
//            get { return _openTickets; }
//        }

//        string[] _historicalCustomIDs;
//        public string[] HistoricalCustomIDs
//        {
//            get { return _historicalCustomIDs; }
//        }

//        string[] _historicalTickets;
//        public string[] HistoricalTickets
//        {
//            get { return _historicalTickets; }
//        }
        
//        /// <summary>
//        /// 
//        /// </summary>
//        public GetOrdersIDsResponceMessage(AccountInfo accountInfo, 
//            string[] openCustomIDs, string[] openTickets,
//            string[] historicalCustomIDs, string[] historicalTickets,
//            bool operationResult)
//            : base(accountInfo, operationResult)
//        {
//            _openCustomIDs = (string[])openCustomIDs.Clone();
//            _openTickets = (string[])openTickets.Clone();

//            _historicalCustomIDs = (string[])historicalCustomIDs.Clone();
//            _historicalTickets = (string[])historicalTickets.Clone();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="expertID"></param>
//        public GetOrdersIDsResponceMessage(AccountInfo accountInfo, 
//            int[] openCustomIDs, int[] openTickets,
//            int[] historicalCustomIDs, int[] historicalTickets, 
//            bool operationResult)
//            : base(accountInfo, operationResult)
//        {
//            _openCustomIDs = GeneralHelper.ToStrings<int>(openCustomIDs).ToArray();
//            _openTickets = GeneralHelper.ToStrings<int>(openTickets).ToArray();

//            _historicalCustomIDs = GeneralHelper.ToStrings<int>(historicalCustomIDs).ToArray();
//            _historicalTickets = GeneralHelper.ToStrings<int>(historicalTickets).ToArray();
//        }
//    }
//}
