//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Collections.ObjectModel;
//using CommonSupport;
//using Arbiter;
//using CommonFinancial;
//using System.Runtime.Serialization;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Allows the execution of orders on a remote execution source, order the arbiter communication
//    /// mechanism.
//    /// </summary>
//    [Serializable]
//    public class RemoteOrderExecutor : RemoteSessionSourceOperational, IOrderExecutor, IDisposable
//    {
//        #region IOrderExecutor Members

//        //volatile IOrderExecutionProvider _orderProvider;
//        ///// <summary>
//        ///// Set the data provider this order execution provider should use as a price reference.
//        ///// </summary>
//        //public IDataProvider DataProvider
//        //{
//        //    get { return _orderProvider.DataProvider; }
//        //}

//        //[field: NonSerialized]
//        //public event OrderUpdateDelegate OrderUpdatedEvent;

//        //[field: NonSerialized]
//        //public event AccountInfoUpdateDelegate AccountInfoUpdateEvent;

//        ///// <summary>
//        ///// Default constructor.
//        ///// </summary>
//        //public RemoteOrderExecution(string name, SessionInfo session, List<ArbiterClientId?> forwardTransportation) 
//        //    : base(name, session, forwardTransportation)
//        //{
//        //    this.DefaultTimeOut = TimeSpan.FromSeconds(14);
//        //    ChangeOperationalState(OperationalStateEnum.Initializing);
//        //}

//        /// <summary>
//        /// Deserialization constructor.
//        /// </summary>
//        /// <param name="info"></param>
//        /// <param name="context"></param>
//        //public RemoteOrderExecution(SerializationInfo info, StreamingContext context)
//        //    : base(info, context)
//        //{
//        //    _orderProvider = (IOrderExecutionProvider)info.GetValue("orderProvider", typeof(IOrderExecutionProvider));
//        //    ChangeOperationalState(OperationalStateEnum.Initializing);
//        //}

//        ///// <summary>
//        ///// Serialization routine.
//        ///// </summary>
//        ///// <param name="info"></param>
//        ///// <param name="context"></param>
//        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
//        //{
//        //    base.GetObjectData(info, context);
//        //    info.AddValue("orderProvider", _orderProvider);
//        //}

//        //#region Order Interface

//        //public bool SetInitialParameters(IOrderExecutionProvider provider)
//        //{
//        //    _orderProvider = provider;
//        //    ChangeOperationalState(OperationalStateEnum.Initializing);

//        //    return true;
//        //}

//        //public bool Initialize()
//        //{
//        //    IDataProvider provider = _orderProvider.DataProvider;
//        //    if (provider != null && provider.Quotes != null)
//        //    {
//        //        provider.Quotes.QuoteUpdateEvent += new QuotationProviderUpdateDelegate(Quote_QuoteUpdateEvent);
//        //    }

//        //    base.Initialize(_orderProvider.SessionInfo);

//        //    ChangeOperationalState(OperationalStateEnum.Operational);

//        //    this.Name = "Order Execution Provider for " + SessionInfo.Name;

//        //    return true;
//        //}

//        //public new void UnInitialize()
//        //{
//        //    ChangeOperationalState(OperationalStateEnum.UnInitialized);

//        //    base.UnInitialize();

//        //    lock (this)
//        //    {
//        //        IDataProvider provider = _orderProvider.DataProvider;
//        //        if (provider != null && provider.Quotes != null)
//        //        {
//        //            provider.Quotes.QuoteUpdateEvent -= new QuotationProviderUpdateDelegate(Quote_QuoteUpdateEvent);
//        //        }
//        //    }
//        //}

//        //#endregion

//        //[MessageReceiver]
//        //protected override ResponceMessage Receive(SubscriptionToSessionTerminatedMessage message)
//        //{
//        //    ChangeOperationalState(OperationalStateEnum.NotOperational);
//        //    return base.Receive(message);
//        //}

//        //[MessageReceiver]
//        //protected override void Receive(SubscriptionToSessionStartedMessage message)
//        //{
//        //    base.Receive(message);
//        //    ChangeOperationalState(OperationalStateEnum.Operational);
//        //}

//        ///// <summary>
//        ///// Helper.
//        ///// </summary>
//        ///// <param name="order"></param>
//        ///// <param name="updateType"></param>
//        //void RaiseOrderUpdateEvent(Order order, Order.UpdateTypeEnum updateType)
//        //{
//        //    if (OrderUpdatedEvent != null)
//        //    {
//        //        OrderUpdatedEvent(this, order, updateType);
//        //    }
//        //}

//        //void UpdateOrderInformation(Order order)
//        //{
//        //    string operationResultMessage;
//        //    OrderInfo? information = GetOrderInformation(order.Id, out operationResultMessage);
//        //    if (information.HasValue)
//        //    {
//        //        SystemMonitor.OperationWarning("Failed to update order [" + order.Id + "] information.");
//        //        return;
//        //    }
//        //    else
//        //    {
//        //        order.AdoptExistingOrderInformation(information.Value);
//        //    }
//        //}

//        /// <summary>
//        /// 
//        /// </summary>
//        void Quote_QuoteUpdateEvent(IQuotationProvider provider)
//        {
//            IOrderManagement orders = _orderProvider.Account.Orders;
//            if (orders == null)
//            {
//                return;
//            }
//            lock (orders)
//            {
//                foreach (Order order in orders.OrdersUnsafe)
//                {
//                    if (order.State == OrderInfo.StateEnum.PlacedPending)
//                    {// Target open price check; if near target, request update.
//                        if ((order.Type == OrderTypeEnum.OrderType_BUYLIMIT && provider.Bid <= order.OpenPrice) ||
//                        (order.Type == OrderTypeEnum.OrderType_BUYSTOP && provider.Bid >= order.OpenPrice) ||
//                        (order.Type == OrderTypeEnum.OrderType_SELLLIMIT && provider.Ask >= order.OpenPrice) ||
//                        (order.Type == OrderTypeEnum.OrderType_SELLSTOP && provider.Ask <= order.OpenPrice))
//                        {
//                            GeneralHelper.FireAndForget(new GeneralHelper.GenericDelegate<Order>(UpdateOrderInformation), order);
//                        }
//                    }
//                    else
//                        if (order.State == OrderInfo.StateEnum.Opened)
//                        {
//                            if (order.RemoteStopLoss.HasValue
//                                && order.RemoteStopLoss.Value != 0)
//                            {// Check for SL.
//                                if ((order.IsBuy && provider.Ask <= order.RemoteStopLoss) ||
//                                    (order.IsSell && provider.Bid >= order.RemoteStopLoss))
//                                {
//                                    GeneralHelper.FireAndForget(new GeneralHelper.GenericDelegate<Order>(UpdateOrderInformation), order);
//                                }
//                            }

//                            if (order.RemoteTakeProfit.HasValue
//                                && order.RemoteTakeProfit.Value != 0)
//                            {// Check for SL.
//                                if ((order.IsBuy && provider.Ask >= order.RemoteTakeProfit) ||
//                                    (order.IsSell && provider.Bid <= order.RemoteTakeProfit))
//                                {
//                                    GeneralHelper.FireAndForget(new GeneralHelper.GenericDelegate<Order>(UpdateOrderInformation), order);
//                                }
//                            }
//                        }
//                } // foreach
//            } // lock
//        }

//        ///// <summary>
//        ///// Helper.
//        ///// </summary>
//        //public OrderInfo? GetOrderInformation(string orderId, out string operationResultMessage)
//        //{
//        //    OrderInfo[] resultInformations;
//        //    bool result = GetOrdersInformation(new string[] { orderId }, out resultInformations, out operationResultMessage);
//        //    if (resultInformations != null && resultInformations.Length > 0)
//        //    {
//        //        return resultInformations[0];
//        //    }
//        //    else
//        //    {
//        //        return null;
//        //    }
//        //}

//        ///// <summary>
//        ///// Obtain infos for these orders.
//        ///// </summary>
//        ///// <param name="orderIds"></param>
//        ///// <param name="informations"></param>
//        ///// <param name="operationResultMessage"></param>
//        ///// <returns></returns>
//        //public bool GetOrdersInformation(string[] orderIds, out OrderInfo[] informations, out string operationResultMessage)
//        //{
//        //    TracerHelper.Trace(this.Name);

//        //    informations = new OrderInfo[] { };
//        //    if (OperationalState != OperationalStateEnum.Operational)
//        //    {
//        //        operationResultMessage = "Attempted operations on non operational order executioner.";
//        //        SystemMonitor.Error(operationResultMessage);
//        //        return false;
//        //    }

//        //    if (orderIds == null || orderIds.Length == 0)
//        //    {
//        //        operationResultMessage = "No informations required, none retrieved.";
//        //        return true;
//        //    }

//        //    OrdersInformationMessage message = new OrdersInformationMessage(SessionInfo, orderIds);
//        //    OrdersInformationResponceMessage responceMessage = this.SendAndReceiveForwarding<OrdersInformationResponceMessage>
//        //        (ForwardTransportationArray, message);

//        //    if (responceMessage == null)
//        //    {
//        //        operationResultMessage = "Getting order information failed due to time out.";
//        //        return false;
//        //    }
//        //    else
//        //        if (responceMessage.OperationResult == false)
//        //        {
//        //            operationResultMessage = responceMessage.ExceptionMessage;
//        //            return false;
//        //        }

//        //    operationResultMessage = "Order information retrieved.";
//        //    informations = responceMessage.OrderInformations;
//        //    return true;
//        //}

//        #endregion

//        //#region IDisposable Members

//        //void IDisposable.Dispose()
//        //{
//        //    base.Dispose();
//        //}

//        //#endregion

//        /// <summary>
//        /// 
//        /// </summary>
//        //[MessageReceiver]
//        //void Receive(AccountInformationUpdateMessage message)
//        //{
//        //    if (AccountInfoUpdateEvent != null && message.OperationResult && message.AccountInfo.HasValue)
//        //    {
//        //        AccountInfoUpdateEvent(this, message.AccountInfo.Value);
//        //    }
//        //}


//        #region IOrderExecutor Members

//        public bool PlaceOrder(Order order, OrderTypeEnum orderType, decimal volume, decimal? allowedSlippage, decimal? desiredPrice,
//            decimal? takeProfit, decimal? stopLoss, string comment, out decimal openingPrice,
//            out DateTime openingTime, out OrderInfo.StateEnum resultState, out string id, out string operationResultMessage)
//        {
//            //TracerHelper.Trace(this.Name);

//            //resultState = OrderInfo.StateEnum.Unknown;
//            //openingPrice = decimal.MinValue;
//            //openingTime = DateTime.MinValue;
//            //id = string.Empty;

//            //if (OperationalState != OperationalStateEnum.Operational)
//            //{
//            //    operationResultMessage = "Attempted operations on non operational order executioner.";
//            //    SystemMonitor.Error(operationResultMessage);
//            //    return false;
//            //}

//            //IDataProvider dataProvider = DataProvider;

//            //if (dataProvider == null)
//            //{
//            //    operationResultMessage = "Data provider for executioner not assigned.";
//            //    return false;
//            //}

//            //if (desiredPrice.HasValue == false)
//            //{
//            //    desiredPrice = OrderInfo.TypeIsBuy(orderType) ? dataProvider.Quotes.Bid : dataProvider.Quotes.Ask;
//            //}

//            //OpenOrderMessage message = new OpenOrderMessage(SessionInfo, orderType, volume, desiredPrice, allowedSlippage,
//            //    takeProfit, stopLoss, comment);
//            //OpenOrderResponceMessage responceMessage = this.SendAndReceiveForwarding<OpenOrderResponceMessage>
//            //    (ForwardTransportationArray, message);


//            //if (responceMessage == null)
//            //{// Time out.
//            //    operationResultMessage = "Failed receive result for order request. In this scenario inconsistency may occur!";
//            //    SystemMonitor.Error(operationResultMessage);
//            //    return false;
//            //}

//            //if (responceMessage.OperationResult == false)
//            //{
//            //    operationResultMessage = responceMessage.ExceptionMessage;
//            //    return false;
//            //}

//            //if (orderType == OrderTypeEnum.OrderType_BUY
//            //    || orderType == OrderTypeEnum.OrderType_SELL)
//            //{// Immediate order.
//            //    resultState = OrderInfo.StateEnum.Opened;
//            //}
//            //else
//            //{// Delayed pending order.
//            //    resultState = OrderInfo.StateEnum.PlacedPending;
//            //}

//            //operationResultMessage = "Order opened.";
//            //id = responceMessage.OrderId.ToString();
//            //openingPrice = responceMessage.OpeningPrice;
//            //openingTime = responceMessage.OpeningDateTime;

//            //_orderProvider.Account.Orders.AddOrder(order);

//            //RaiseOrderUpdateEvent(order, Order.UpdateTypeEnum.Placed);

//            //return true;
//        }


//        /// <summary>
//        /// Pass null to any of the decimal parameters to signify not changed, pass decimal.MinValue to signify
//        /// set value to "not assigned".
//        /// </summary>
//        public bool ModifyOrder(Order order, decimal? stopLoss, decimal? takeProfit, decimal? targetOpenPrice,
//            out string modifiedId, out string operationResultMessage)
//        {
//            //TracerHelper.Trace(this.Name);
//            //modifiedId = order.Id;

//            //if (OperationalState != OperationalStateEnum.Operational)
//            //{
//            //    operationResultMessage = "Attempted operations on non operational order executioner.";
//            //    SystemMonitor.Error(operationResultMessage);
//            //    return false;
//            //}

//            //ModifyOrderMessage message = new ModifyOrderMessage(SessionInfo, order.Id, stopLoss, takeProfit, targetOpenPrice, null);

//            //SessionOperationResponceMessage responceMessage = this.SendAndReceiveForwarding<SessionOperationResponceMessage>(
//            //    ForwardTransportationArray, message);

//            //if (responceMessage == null)
//            //{// Time out.
//            //    operationResultMessage = "Timeout, failed receive result for order modification request. In this scenario inconsistency may occur!";
//            //    SystemMonitor.Error(operationResultMessage);
//            //    return false;
//            //}

//            //if (responceMessage.OperationResult == false)
//            //{
//            //    operationResultMessage = responceMessage.ExceptionMessage;
//            //    return false;
//            //}

//            //ModifyOrderResponceMessage castedResponceMessage = (ModifyOrderResponceMessage)responceMessage;
//            //SystemMonitor.CheckError(string.IsNullOrEmpty(castedResponceMessage.OrderModifiedId) == false, "Modified not assigned.");
//            //modifiedId = castedResponceMessage.OrderModifiedId;
//            //operationResultMessage = "Order modified.";

//            //RaiseOrderUpdateEvent(order, Order.UpdateTypeEnum.Modified);

//            //return true;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public bool DecreaseOrderVolume(Order order, decimal volumeDecreasal, decimal? allowedSlippage, decimal? desiredPrice,
//            out decimal decreasalPrice, out string modifiedId, out string operationResultMessage)
//        {
//            //TracerHelper.Trace(this.Name);
//            //modifiedId = order.Id;
//            //decreasalPrice = decimal.MinValue;

//            //if (OperationalState != OperationalStateEnum.Operational)
//            //{
//            //    operationResultMessage = "Attempted operations on non operational order executioner.";
//            //    SystemMonitor.Error(operationResultMessage);
//            //    return false;
//            //}

//            //CloseOrderVolumeMessage message = new CloseOrderVolumeMessage(SessionInfo, order.Id, volumeDecreasal, desiredPrice, allowedSlippage);
//            //CloseOrderVolumeResponceMessage responceMessage = this.SendAndReceiveForwarding<CloseOrderVolumeResponceMessage>
//            //    (ForwardTransportationArray, message);

//            //if (responceMessage == null)
//            //{// Time out.
//            //    operationResultMessage = "Failed receive result for order request. In this scenario inconsistency may occur!";
//            //    SystemMonitor.Error(operationResultMessage);
//            //    return false;
//            //}

//            //if (responceMessage.OperationResult == false)
//            //{
//            //    operationResultMessage = responceMessage.ExceptionMessage;
//            //    return false;
//            //}

//            //operationResultMessage = "Order volume decreased.";
//            //decreasalPrice = responceMessage.ClosingPrice;

//            //if (string.IsNullOrEmpty(responceMessage.OrderModifiedId))
//            //{// Since the original order has changed its ticket number; and we have failed to establish the new one - we can no longer track it so unregister.
//            //    SystemMonitor.OperationWarning("Failed to establish new modified order ticket; order will be re-aquired.");
//            //    _orderProvider.Account.Orders.RemoveOrder(order);
//            //    _orderProvider.Account.BeginUpdate();
//            //}
//            //else
//            //{
//            //    // When modified, order changes its Id.
//            //    modifiedId = responceMessage.OrderModifiedId;
//            //}

//            //RaiseOrderUpdateEvent(order, Order.UpdateTypeEnum.VolumeChanged);

//            //return true;
//        }

//        /// <summary>
//        /// Operation not supported.
//        /// </summary>
//        public bool IncreaseOrderVolume(Order order, decimal volumeIncrease, decimal? allowedSlippage, decimal? desiredPrice, out decimal increasalPrice,
//            out string modifiedId, out string operationResultMessage)
//        {
//            //operationResultMessage = "Remote Order Execution Provider does not support volume increase.";
//            //SystemMonitor.OperationError(operationResultMessage);
//            //increasalPrice = 0;
//            //modifiedId = string.Empty;
//            //return false;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        public bool CancelPendingOrder(Order order, out string modifiedId, out string operationResultMessage)
//        {
//            //decimal closingPrice;
//            //DateTime closingDateTime;
//            //if (DoCloseOrder(order, -1, 0, out closingPrice, out closingDateTime, out modifiedId, out operationResultMessage))
//            //{
//            //    RaiseOrderUpdateEvent(order, Order.UpdateTypeEnum.Canceled);
//            //    return true;
//            //}

//            //return false;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public bool CloseOrder(Order order, decimal? allowedSlippage, decimal? desiredPrice, out decimal closingPrice,
//            out DateTime closingTime, out string modifiedId, out string operationResultMessage)
//        {
//            //if (DoCloseOrder(order, allowedSlippage, desiredPrice, out closingPrice,
//            //out closingTime, out modifiedId, out operationResultMessage))
//            //{
//            //    RaiseOrderUpdateEvent(order, Order.UpdateTypeEnum.Closed);
//            //    return true;
//            //}

//            //return false;
//        }

//        ///// <summary>
//        ///// Helper.
//        ///// </summary>
//        //bool DoCloseOrder(Order order, decimal? allowedSlippage, decimal? desiredPrice, out decimal closingPrice,
//        //    out DateTime closingTime, out string modifiedId, out string operationResultMessage)
//        //{
//        //    TracerHelper.Trace(this.Name);

//        //    closingPrice = decimal.MinValue;
//        //    closingTime = DateTime.MinValue;
//        //    modifiedId = order.Id;

//        //    if (OperationalState != OperationalStateEnum.Operational)
//        //    {
//        //        operationResultMessage = "Attempted operations on non operational order executioner.";
//        //        SystemMonitor.Error(operationResultMessage);
//        //        return false;
//        //    }

//        //    CloseOrderVolumeMessage message = new CloseOrderVolumeMessage(SessionInfo, order.Id, desiredPrice, allowedSlippage);
//        //    CloseOrderVolumeResponceMessage responceMessage = this.SendAndReceiveForwarding<CloseOrderVolumeResponceMessage>
//        //        (ForwardTransportationArray, message);

//        //    if (responceMessage == null)
//        //    {// Time out.
//        //        operationResultMessage = "Failed receive result for order request. In this scenario inconsistency may occur!";
//        //        SystemMonitor.Error(operationResultMessage);
//        //        return false;
//        //    }

//        //    if (responceMessage.OperationResult)
//        //    {
//        //        operationResultMessage = "Order closed.";
//        //        closingPrice = responceMessage.ClosingPrice;
//        //        closingTime = responceMessage.ClosingDateTime;

//        //        SystemMonitor.CheckError(order.Id == responceMessage.OrderId.ToString(), "Order id mismatch.");

//        //        modifiedId = responceMessage.OrderModifiedId.ToString();
//        //        return true;
//        //    }

//        //    operationResultMessage = responceMessage.ExceptionMessage;
//        //    return false;
//        //}

//        #endregion

//        ///// <summary>
//        ///// Obtains all the orders from the order executioner.
//        ///// </summary>
//        //public bool SynchronizeOrders(string[] updateOrdersIds, out string operationResultMessage)
//        //{
//        //    TracerHelper.Trace(this.Name);

//        //    if (OperationalState != OperationalStateEnum.Operational)
//        //    {
//        //        operationResultMessage = "Executioner not operational.";
//        //        return false;
//        //    }

//        //    if (_orderProvider == null || _orderProvider.Account == null
//        //        || _orderProvider.Account.Orders == null)
//        //    {
//        //        operationResultMessage = "Remote order execution operation can not continue due to lack of required refereces.";
//        //        SystemMonitor.OperationWarning(operationResultMessage);
//        //        return false;
//        //    }

//        //    IOrderManagement management = _orderProvider.Account.Orders;

//        //    List<string> ordersIds = new List<string>();
//        //    List<string> historicalOrdersIds = new List<string>();

//        //    if (updateOrdersIds != null && updateOrdersIds.Length != 0)
//        //    {// Update only the orders from the request.
//        //        ordersIds.AddRange(updateOrdersIds);
//        //    }
//        //    else
//        //    {// Obtain full list of orders from the source.

//        //        string[] openPlatformIds = new string[] { };
//        //        string[] historicalPlatformsIds = new string[] { };

//        //        if (GetAllOrdersIds(out openPlatformIds, out historicalPlatformsIds, out operationResultMessage) == false)
//        //        {
//        //            return false;
//        //        }

//        //        ordersIds.AddRange(openPlatformIds);
//        //        historicalOrdersIds.AddRange(historicalPlatformsIds);

//        //        // Since we obtained all, filter existing.
//        //        foreach (string orderId in ordersIds.ToArray())
//        //        {
//        //            Order order = management.GetOrderById(orderId);
//        //            if (order != null && order.IsOpenOrPending)
//        //            {
//        //                ordersIds.Remove(orderId);
//        //            }
//        //        }

//        //        foreach (string orderId in historicalOrdersIds.ToArray())
//        //        {
//        //            Order order = management.GetOrderById(orderId);
//        //            if (order != null && order.IsOpenOrPending == false)
//        //            {
//        //                historicalOrdersIds.Remove(orderId);
//        //            }
//        //        }
//        //    }

//        //    operationResultMessage = "Orders obtained successfully.";
//        //    bool operationResult = true;

//        //    if (ordersIds.Count == 0 && historicalOrdersIds.Count == 0)
//        //    {// Nothing to update.
//        //        return operationResult;
//        //    }

//        //    // Add the hitorical to ordersIds, than process them all.
//        //    ordersIds.AddRange(historicalOrdersIds);

//        //    // Get informations for each.
//        //    OrderInfo[] informations;
//        //    if (GetOrdersInformation(ordersIds.ToArray(), out informations, out operationResultMessage) == false)
//        //    {// One failure is enough to establish the entire operation as failed.
//        //        operationResult = false;
//        //        operationResultMessage = "Some orders were not obtained properly.";
//        //    }

//        //    if (informations == null)
//        //    {
//        //        return operationResult;
//        //    }

//        //    foreach (OrderInfo info in informations)
//        //    {// Check if the order already exists.
//        //        Order order = _orderProvider.Account.Orders.GetOrderById(info.Id);

//        //        if (order == null)
//        //        {// Not existing we need to create a brand new one.
//        //            order = new Order(_orderProvider);
//        //            if (order.Initialize() == false)
//        //            {
//        //                SystemMonitor.Error("Failed to initialize order.");
//        //                continue;
//        //            }
//        //        }

//        //        if (order.AdoptExistingOrderInformation(info) == false)
//        //        {
//        //            order.UnInitialize();

//        //            operationResult = false;
//        //            operationResultMessage = "Some orders were not obtained properly.";

//        //            continue;
//        //        }

//        //        // Existing order will simply be skipped, but make sure to add for new orders case.
//        //        _orderProvider.Account.Orders.AddOrder(order);
//        //    }

//        //    return operationResult;
//        //}

//        ///// <summary>
//        ///// Obtain orders Ids from the server.
//        ///// </summary>
//        //public bool GetAllOrdersIds(out string[] activeOrdersIds, out string[] inactiveOrdersIds,
//        //    out string operationResultMessage)
//        //{
//        //    TracerHelper.Trace(this.Name);

//        //    activeOrdersIds = new string[] { };
//        //    inactiveOrdersIds = new string[] { };

//        //    if (OperationalState != OperationalStateEnum.Operational)
//        //    {
//        //        operationResultMessage = "Attempted operations on non operational order executioner.";
//        //        SystemMonitor.Error(operationResultMessage);
//        //        return false;
//        //    }

//        //    SessionOperationResponceMessage responceMessage =
//        //        this.SendAndReceiveForwarding<SessionOperationResponceMessage>(ForwardTransportationArray, new GetAllOrdersIDsMessage(SessionInfo));

//        //    if (responceMessage == null)
//        //    {
//        //        operationResultMessage = "Timeout";
//        //        return false;
//        //    }

//        //    if (responceMessage.OperationResult == false)
//        //    {
//        //        operationResultMessage = responceMessage.ExceptionMessage;
//        //        return false;
//        //    }

//        //    GetOrdersIDsResponceMessage castedResponceMessage = (GetOrdersIDsResponceMessage)responceMessage;

//        //    activeOrdersIds = new string[castedResponceMessage.OpenTickets.Length];
//        //    for (int i = 0; i < castedResponceMessage.OpenTickets.Length; i++)
//        //    {
//        //        activeOrdersIds[i] = castedResponceMessage.OpenTickets[i].ToString();
//        //    }

//        //    inactiveOrdersIds = new string[castedResponceMessage.HistoricalTickets.Length];
//        //    for (int i = 0; i < castedResponceMessage.HistoricalTickets.Length; i++)
//        //    {
//        //        inactiveOrdersIds[i] = castedResponceMessage.HistoricalTickets[i].ToString();
//        //    }

//        //    operationResultMessage = "Orders obtained properly.";
//        //    return true;
//        //}

//        /// <summary>
//        /// Download latest account info from source.
//        /// </summary>
//        /// <returns></returns>
//        public AccountInfo? GetAccountInfoUpdate()
//        {
//            TracerHelper.Trace(this.Name);

//            SessionOperationResponceMessage responce
//                = this.SendAndReceiveForwarding<SessionOperationResponceMessage>(
//                    ForwardTransportationArray, new AccountInformationMessage(SessionInfo));

//            if (responce == null || responce.OperationResult == false)
//            {
//                SystemMonitor.OperationWarning("Failed to obtain account information.");
//                return null;
//            }

//            return ((AccountInformationUpdateMessage)responce).AccountInfo;
//        }

//    }
//}
