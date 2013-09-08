using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Arbiter;
using CommonFinancial;
using CommonSupport;

namespace ForexPlatform
{
    /// <summary>
    /// Class servers as a gate to automated the messaging behind an order execution source. Allows to create
    /// a new source by simply implementing the IImplementation interface.
    /// </summary>
    [Serializable]
    public class OrderExecutionSourceStub : SourceStub
    {
        bool _supportsActiveOrderManagement = false;

        ListEx<AccountInfo> _accounts = new ListEx<AccountInfo>();

        ListEx<TransportInfo> _subscribers = new ListEx<TransportInfo>();

        IImplementation _implementation;
        
        /// <summary>
        /// Interface that stub implementation must implement.
        /// </summary>
        public interface IImplementation : IOperational
        {
            /// <summary>
            /// 
            /// </summary>
            AccountInfo? GetAccountInfoUpdate(AccountInfo accountInfo);

            /// <summary>
            /// 
            /// </summary>
            AccountInfo[] GetAvailableAccounts();

            /// <summary>
            /// 
            /// </summary>
            bool GetOrdersInfos(AccountInfo accountInfo, List<string> ordersIds, out OrderInfo[] ordersInfos, out string operationResultMessage);

            /// <summary>
            /// Allows to submit an order for execution and return operationResult, thus effectively an asynchronous operation.
            /// </summary>
            /// <returns>The ID of the newly submitted order, or null if submission failed.</returns>
            string SubmitOrder(AccountInfo account, Symbol symbol, OrderTypeEnum orderType,
                int volume, decimal? allowedSlippage, decimal? desiredPrice, decimal? takeProfit, decimal? stopLoss,
                string comment, out string operationResultMessage);

            /// <summary>
            /// Allows opening of orders for immediate or delayed execution.
            /// This may execute (open) the order synchronously, if the execution provider supports it
            /// and if the order is immediate, not delayed order.
            /// </summary>
            /// <param name="resultingOrderState">Result indicates the state that the order should be considered, once leaving this baseMethod. Some orders can be executed immediately, while orders only get placed (delayed orders).</param>
            /// <returns>Returns true if order is placed OK.</returns>
            bool ExecuteMarketOrder(AccountInfo accountInfo, Symbol symbol, OrderTypeEnum orderType, 
                int volume, decimal? allowedSlippage, decimal? desiredPrice, decimal? takeProfit, decimal? stopLoss, 
                string comment, out OrderInfo? orderPlaced, out string operationResultMessage);

            /// <summary>
            /// Change order execution related parameters.
            /// </summary>
            /// <param name="stopLoss">Applicable to open or pending orders only, pass null to skip, Decimal.MinValue to signify no value.</param>
            /// <param name="takeProfit">Applicable to open or pending orders only, pass null to skip, Decimal.MinValue to signify no value.</param>
            /// <param name="targetOpenPrice">Applicable to pending and opened orders only (pass null to skip)</param>
            /// <returns>Returns true if the operation was successful and order was modified.</returns>
            bool ModifyOrder(AccountInfo accountInfo, string orderId, Decimal? stopLoss, Decimal? takeProfit, Decimal? targetOpenPrice,
                out string modifiedId, out string operationResultMessage);

            /// <summary>
            /// Decrease already existing order closeVolume. This is in effect a partial close of the order.
            /// </summary>
            bool DecreaseOrderVolume(AccountInfo accountInfo, string orderId, Decimal volumeDecreasal, Decimal? allowedSlippage,
                Decimal? desiredPrice, out Decimal decreasalPrice, out string modifiedId, out string operationResultMessage);

            /// <summary>
            /// Increase of closeVolume allowed on pending orders exclusively.
            /// </summary>
            bool IncreaseOrderVolume(AccountInfo accountInfo, string orderId, Decimal volumeIncrease, Decimal? allowedSlippage,
                Decimal? desiredPrice, out Decimal increasalPrice, out string modifiedId, out string operationResultMessage);

            /// <summary>
            /// Cancel pending order.
            /// </summary>
            //bool CancelPendingOrder(AccountInfo accountInfo, string orderId, out string modifiedId, out string operationResultMessage);

            /// <summary>
            /// Close existing opened order or cancel a pending one.
            /// </summary>
            bool CloseOrCancelOrder(AccountInfo accountInfo, string orderId, string orderTag, Decimal? allowedSlippage, Decimal? desiredPrice, out Decimal closingPrice,
                out DateTime closingTime, out string modifiedId, out string operationResultMessage);

            /// <summary>
            /// Is this baseCurrency permited for trading.
            /// </summary>
            /// <param name="baseCurrency"></param>
            /// <returns></returns>
            bool IsPermittedSymbol(AccountInfo accountInfo, Symbol symbol);

            /// <summary>
            /// Returns the compatibility level, 0 means no compatibility, int.Max is highest.
            /// </summary>
            int IsDataSourceSymbolCompatible(ComponentId dataSourceId, Symbol symbol);
        }

        /// <summary>
        /// 
        /// </summary>
        public OrderExecutionSourceStub(string name, bool supportsActiveOrderManagement)
            : base(name, SourceTypeEnum.OrderExecution | SourceTypeEnum.Live)
        {
            _supportsActiveOrderManagement = supportsActiveOrderManagement;
            ChangeOperationalState(OperationalStateEnum.Constructed);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public OrderExecutionSourceStub(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ChangeOperationalState(OperationalStateEnum.Constructed);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Initialize(IImplementation implementation)
        {
            _implementation = implementation;
            
            StatusSynchronizationSource = implementation;
            
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateOrderInfo(AccountInfo account, Order.UpdateTypeEnum updateType, OrderInfo orderInfo)
        {
            UpdateOrdersInfo(account, new Order.UpdateTypeEnum[] { updateType }, new OrderInfo[] { orderInfo });
        }

        public void UpdateOrdersInfo(AccountInfo account, Order.UpdateTypeEnum[] updatesType, OrderInfo[] ordersInfo)
        {
            lock (this)
            {// Will only add it once, the remaining will just return false.
                if (_subscribers.Count > 0)
                {
                    // Establish account subscribers and deliver them the update.
                    this.SendRespondingToMany(_subscribers, new OrdersInformationUpdateResponceMessage(account, ordersInfo, updatesType, true));
                }
            }
        }

        public void UpdatePositionsInfo(AccountInfo account, PositionInfo[] positionsInfo)
        {
            lock (this)
            {// Will only add it once, the remaining will just return false.
                if (_subscribers.Count > 0)
                {
                    // Establish account subscribers and deliver them the update.
                    this.SendRespondingToMany(_subscribers, new PositionsInformationUpdateResponceMessage(account, positionsInfo, true));
                }
            }
        }

        public void UpdateAccountInfo(AccountInfo accountInfo)
        {
            lock (this)
            {// Will only add it once, the remaining will just return false.
                _accounts.Add(accountInfo);

                if (_subscribers.Count > 0)
                {
                    // Establish account subscribers and deliver them the update.
                    this.SendRespondingToMany(_subscribers, new AccountInformationUpdateMessage(accountInfo, true));
                }
            }
        }

        #region Arbiter Messages

        ///// <summary>
        ///// Implement a request to deliver orders ids.
        ///// </summary>
        //[MessageReceiver]
        //AccountResponceMessage Receive(GetAllOrdersIDsMessage message)
        //{
        //    IImplementation implementation = _implementation;
        //    if (implementation == null || OperationalState != OperationalStateEnum.Operational)
        //    {
        //        return new AccountResponceMessage(message.AccountInfo, false);
        //    }

        //    string operationResultMessage;
        //    string[] activeIds, inactiveIds;
        //    bool result = implementation.GetAllOrdersIds(message.AccountInfo, out activeIds, out inactiveIds, out operationResultMessage);

        //    GetOrdersIDsResponceMessage responce = new GetOrdersIDsResponceMessage(message.AccountInfo, 
        //        new string[] { }, activeIds, new string[] { }, inactiveIds, result);
        //    responce.ResultMessage = operationResultMessage;

        //    return responce;
        //}

        [MessageReceiver]
        AccountResponceMessage Receive(GetOrdersInformationMessage message)
        {
            IImplementation implementation = _implementation;
            if (implementation == null || OperationalState != OperationalStateEnum.Operational)
            {
                if (message.RequestResponce)
                {
                    return new AccountResponceMessage(message.AccountInfo, false);
                }

                return null;
            }

            string operationResultMessage;
            OrderInfo[] orderInfos;
            bool operationResult = implementation.GetOrdersInfos(message.AccountInfo, message.OrderTickets, out orderInfos, out operationResultMessage);

            OrdersInformationUpdateResponceMessage responce = new OrdersInformationUpdateResponceMessage(message.AccountInfo, 
                orderInfos, operationResult);
            responce.ResultMessage = operationResultMessage;

            if (message.RequestResponce)
            {
                return responce;
            }
            else
            {
                if (operationResult)
                {
                    SendResponding(message.TransportInfo, responce);
                }
            }
            return null;
        }

        [MessageReceiver]
        ExecuteMarketOrderResponceMessage Receive(ExecuteMarketOrderMessage message)
        {
            IImplementation implementation = _implementation;
            if (implementation == null || OperationalState != OperationalStateEnum.Operational)
            {
                return new ExecuteMarketOrderResponceMessage(message.AccountInfo, null, false);
            }

            string operationResultMessage;
            if (message.PerformSynchronous == false)
            {
                return new ExecuteMarketOrderResponceMessage(message.AccountInfo, null, true);
            }

            OrderInfo? info;
            if (implementation.ExecuteMarketOrder(message.AccountInfo, message.Symbol, message.OrderType, message.Volume, message.Slippage,
                message.DesiredPrice, message.TakeProfit, message.StopLoss, message.Comment, out info, out operationResultMessage) == false)
            {
                return new ExecuteMarketOrderResponceMessage(message.AccountInfo, null, false) { OperationResultMessage = operationResultMessage };
            }

            return new ExecuteMarketOrderResponceMessage(message.AccountInfo, info, true);
        }

        [MessageReceiver]
        SubmitOrderResponceMessage Receive(SubmitOrderMessage message)
        {
            IImplementation implementation = _implementation;
            if (implementation == null || OperationalState != OperationalStateEnum.Operational)
            {
                return new SubmitOrderResponceMessage(message.AccountInfo, string.Empty, false);
            }

            SubmitOrderResponceMessage responce;
            string operationResultMessage;
            string id;

            if (message.PerformSynchronous == false)
            {// We need to place order synchronously.
                responce = new SubmitOrderResponceMessage(message.AccountInfo, string.Empty, true);
            }
            else
            {// Just submit the order.

                id = implementation.SubmitOrder(message.AccountInfo, message.Symbol, message.OrderType, message.Volume, message.Slippage,
                    message.DesiredPrice, message.TakeProfit, message.StopLoss, message.Comment, out operationResultMessage);

                responce = new SubmitOrderResponceMessage(message.AccountInfo, id, string.IsNullOrEmpty(id) == false);
                responce.ResultMessage = operationResultMessage;
            }

            return responce;
        }

        [MessageReceiver]
        GetExecutionSourceParametersResponceMessage Receive(GetExecutionSourceParametersMessage message)
        {
            return new GetExecutionSourceParametersResponceMessage(_supportsActiveOrderManagement);
        }

        [MessageReceiver]
        AccountResponceMessage Receive(ModifyOrderMessage message)
        {
            IImplementation implementation = _implementation;
            if (implementation == null || OperationalState != OperationalStateEnum.Operational)
            {
                return new AccountResponceMessage(message.AccountInfo, false);
            }

            string modifiedId, operationResultMessage;
            ModifyOrderResponceMessage responce;

            if (implementation.ModifyOrder(message.AccountInfo, message.OrderId, message.StopLoss, message.TakeProfit, message.TargetOpenPrice,
                out modifiedId, out operationResultMessage))
            {
                responce = new ModifyOrderResponceMessage(message.AccountInfo, 
                    message.OrderId, modifiedId, true);
            }
            else
            {
                responce = new ModifyOrderResponceMessage(message.AccountInfo, 
                    message.OrderId, modifiedId, false);
            }

            responce.ResultMessage = operationResultMessage;
            return responce;
        }

        [MessageReceiver]
        AccountResponceMessage Receive(CloseOrderVolumeMessage message)
        {
            IImplementation implementation = _implementation;
            if (implementation == null || OperationalState != OperationalStateEnum.Operational)
            {
                return new AccountResponceMessage(message.AccountInfo, false);
            }

            decimal closingPrice;
            DateTime closingTime;
            string modifiedId;
            string operationResultMessage;

            CloseOrderVolumeResponceMessage responce;
            if (implementation.CloseOrCancelOrder(message.AccountInfo, message.OrderId, message.OrderTag, message.Slippage, message.Price, 
                out closingPrice, out closingTime, out modifiedId, out operationResultMessage))
            {
                responce = new CloseOrderVolumeResponceMessage(message.AccountInfo, message.OrderId, modifiedId,
                    closingPrice, closingTime, true);
            }
            else
            {
                responce = new CloseOrderVolumeResponceMessage(message.AccountInfo, message.OrderId, string.Empty,
                    decimal.Zero, DateTime.MinValue, false);
            }

            responce.ResultMessage = operationResultMessage;
            return responce;
        }

        [MessageReceiver]
        ResponceMessage Receive(SubscribeToSourceAccountsUpdatesMessage message)
        {
            lock (this)
            {
                if (message.Subscribe)
                {
                    _subscribers.Add(message.TransportInfo);
                }
                else
                {
                    _subscribers.Remove(message.TransportInfo);
                }
            }

            return new ResponceMessage(true);
        }


        [MessageReceiver]
        AccountResponceMessage Receive(AccountInformationMessage message)
        {
            IImplementation implementation = _implementation;
            if (implementation != null && OperationalState == OperationalStateEnum.Operational)
            {
                AccountInfo? updatedInfo = implementation.GetAccountInfoUpdate(message.AccountInfo);
                if (updatedInfo.HasValue)
                {
                    if (message.RequestResponce)
                    {
                        return new AccountResponceMessage(updatedInfo.Value, true);
                    }
                    else
                    {
                        SendResponding(message.TransportInfo, new AccountResponceMessage(updatedInfo.Value, true));
                    }
                }
            }

            if (message.RequestResponce)
            {
                return new AccountResponceMessage(message.AccountInfo, false);
            }
            else
            {
                return null;
            }
        }
        
        [MessageReceiver]
        GetDataSourceSymbolCompatibleResponceMessage Receive(GetDataSourceSymbolCompatibleMessage message)
        {
            IImplementation implementation = _implementation;
            if (implementation == null || OperationalState != OperationalStateEnum.Operational)
            {
                return new GetDataSourceSymbolCompatibleResponceMessage(false);
            }
            else
            {
                int result = implementation.IsDataSourceSymbolCompatible(message.DataSourceId, message.Symbol);
                return new GetDataSourceSymbolCompatibleResponceMessage(true) { CompatibilityLevel = result };
            }
        }

        [MessageReceiver]
        GetAvailableAccountsResponceMessage Receive(GetAvailableAccountsMessage message)
        {
            IImplementation implementation = _implementation;
            if (implementation == null || OperationalState != OperationalStateEnum.Operational)
            {
                return new GetAvailableAccountsResponceMessage(new AccountInfo[] { }, false);
            }

            return new GetAvailableAccountsResponceMessage(implementation.GetAvailableAccounts(), true);
        }

        #endregion
    }
}
