
//namespace ForexPlatform
//{
//    /// <summary>
//    /// Allows the execution of orders on a remote order execution sources.
//    /// Provides a bridge for the orders to be delivered by the Expert Session to the source,
//    /// that need to do the executing.
//    /// </summary>
//    [Serializable]
//    public class RemoteOrderExecutionProvider : RemoteProvider, IOrderExecutionProvider, IOrderExecutor, IOrderHistory
//    {
//        //List<Order> _orders = new List<Order>();
//        //volatile IDataProvider _dataProvider;

//        //#region IOrderExecutioner Members

//        //RemoteExecutionAccount _executionAccount;
//        //public OrderExecutionAccount Account
//        //{
//        //    get { return _executionAccount; }
//        //}



//        //#endregion

//        ///// <summary>
//        ///// 
//        ///// </summary>
//        //public RemoteOrderExecutionProvider(SessionInfo sessionInfo, List<ArbiterClientId?> forwardTransportation)
//        //    : base("", sessionInfo, forwardTransportation)
//        //{
//        //    this.DefaultTimeOut = TimeSpan.FromSeconds(14);

//        //    _executionAccount = new RemoteExecutionAccount();
//        //    _executionAccount.UpdateRequestEvent += new OrderExecutionAccount.UpdateRequestDelegate(_executionAccount_UpdateRequestEvent);
//        //}

//        ///// <summary>
//        ///// Deserialization constructor.
//        ///// </summary>
//        //public RemoteOrderExecutionProvider(SerializationInfo info, StreamingContext context)
//        //    : base(info, context)
//        //{
//        //    _executionAccount = new RemoteExecutionAccount();
//        //    _executionAccount.UpdateRequestEvent += new OrderExecutionAccount.UpdateRequestDelegate(_executionAccount_UpdateRequestEvent);
//        //}


//        ///// <summary>
//        ///// Release all taken resources.
//        ///// </summary>
//        //public override void Dispose()
//        //{
//        //    base.Dispose();
//        //    lock (this)
//        //    {
//        //        foreach (Order order in _orders)
//        //        {
//        //            order.Dispose();
//        //        }

//        //        _orders.Clear();
//        //        _executionAccount.Dispose();
//        //        _executionAccount = null;
//        //    }
//        //}

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="provider"></param>
//        /// <returns></returns>
//        //public bool Initialize(IDataProvider provider)
//        //{
//            //lock (this)
//            //{
//            //    if (_dataProvider != null && _dataProvider.Quote != null)
//            //    {
//            //        _dataProvider.Quote.QuoteUpdateEvent -= new QuotationProviderUpdateDelegate(Quote_QuoteUpdateEvent);
//            //    }

//            //    _dataProvider = provider;

//            //    if (_dataProvider != null && _dataProvider.Quote != null)
//            //    {
//            //        _dataProvider.Quote.QuoteUpdateEvent += new QuotationProviderUpdateDelegate(Quote_QuoteUpdateEvent);
//            //    }

//            //    base.Initialize(_dataProvider.SessionInfo);
//            //}

//            //ChangeOperationalState(OperationalStateEnum.Operational);

//            //lock (this)
//            //{
//            //    _executionAccount.Initialize(this);
//            //}

//            //this.Name = "Order Execution Provider for " + SessionInfo.Name;

//            //GeneralHelper.FireAndForget(delegate() { _executionAccount.Update(); });

//            //Order[] orders;
//            //lock (this)
//            //{
//            //    orders = _orders.ToArray();
//            //}

//            //foreach (Order order in orders)
//            //{
//            //    if (order.State == OrderInformation.StateEnum.UnInitialized
//            //        && order.Initialize() == false)
//            //    {
//            //        SystemMonitor.Error("Failed to initialize order.");
//            //    }
//            //}

//            //return true;
//        //}

//        ///// <summary>
//        ///// 
//        ///// </summary>
//        //public new void UnInitialize()
//        //{
//        //    base.UnInitialize();

//        //    lock (this)
//        //    {
//        //        _executionAccount.UnInitialize();
//        //        foreach (Order order in _orders)
//        //        {
//        //            order.UnInitialize();
//        //        }
//        //    }
//        //}




//    }
//}
