using System;
using System.Collections.Generic;
using System.Threading;
using CommonFinancial;
using CommonSupport;
using ForexPlatform;
using FXCore;

namespace FXCMAdapter
{
    /// <summary>
    /// Class manages order in MBTrading integration.
    /// </summary>
    public class FXCMOrders : Operational, OrderExecutionSourceStub.IImplementation, IDisposable
	{
		FXCMAdapter _adapter;
		FXCMConnectionManager _manager;
		OperationPerformerStub _operationStub;
		BackgroundMessageLoopOperator _messageLoopOperator;

        Dictionary<string, AccountInfo> _accounts = new Dictionary<string, AccountInfo>();

        /// <summary>
        /// Constructor.
        /// </summary>
		public FXCMOrders(BackgroundMessageLoopOperator messageLoopOperator)
        {
            _messageLoopOperator = messageLoopOperator;
            _operationStub = new OperationPerformerStub();

            ChangeOperationalState(OperationalStateEnum.Constructed);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Initialize(FXCMAdapter adapter, FXCMConnectionManager manager)
		{
			SystemMonitor.CheckError(_messageLoopOperator.InvokeRequred == false, "Init must better be called on message loop method.");

			//TODO: Guess what this piece does
			StatusSynchronizationEnabled = true;
			StatusSynchronizationSource = manager;

			_adapter = adapter;
			_manager = manager;

			ChangeOperationalState(OperationalStateEnum.Operational);

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void UnInitialize()
		{
			_manager = null;
			_adapter = null;

			ChangeOperationalState(OperationalStateEnum.NotOperational);
		}

		#region IImplementation Members

		public AccountInfo? GetAccountInfoUpdate(AccountInfo accountInfo)
		{
            lock (this)
            {
                if (_accounts.ContainsKey(accountInfo.Id))
                {
                    return _accounts[accountInfo.Id];
                }

                //foreach (AccountInfo availableAccount in _availableAccounts)
                //{
                //    if (availableAccount.Id.Equals(accountInfo.Id))
                //    {
                //        return availableAccount;
                //    }
                //}
            }

			return null;
		}

		public AccountInfo[] GetAvailableAccounts()
		{
            // Perform update.
            lock (this)
            {
                TableAut accountsTable = (FXCore.TableAut)_manager.Desk.FindMainTable("accounts");
                foreach (RowAut item in (RowsEnumAut)accountsTable.Rows)
                {
                    string id = (string)item.CellValue("AccountID");

                    if (string.IsNullOrEmpty(id))
                    {
                        SystemMonitor.OperationWarning("Account with null/empty id found.");
                        continue;
                    }

                    AccountInfo info = new AccountInfo();

                    if (_accounts.ContainsKey(id))
                    {// Existing account info.
                        info = _accounts[id];
                    }
                    else
                    {// New account info.
                        info.Guid = Guid.NewGuid();
                        info.Id = id;
                    }

                    info.Name = (string)item.CellValue("AccountName");
                    info.Balance = Math.Round(new decimal((double)item.CellValue("Balance")), IntegrationAdapter.AdvisedAccountDecimalsPrecision);
                    info.Equity = Math.Round(new decimal((double)item.CellValue("Equity")), IntegrationAdapter.AdvisedAccountDecimalsPrecision);
                    info.Margin = Math.Round(new decimal((double)item.CellValue("UsableMargin")), IntegrationAdapter.AdvisedAccountDecimalsPrecision);
                    info.Profit = Math.Round(new decimal((double)item.CellValue("GrossPL")), IntegrationAdapter.AdvisedAccountDecimalsPrecision);
                    info.FreeMargin = Math.Round(new decimal((double)item.CellValue("UsableMargin")), IntegrationAdapter.AdvisedAccountDecimalsPrecision);

                    // Finally, assign the update structure.
                    _accounts[id] = info;
                }

                return GeneralHelper.EnumerableToArray<AccountInfo>(_accounts.Values);
            }

		}

		public bool GetOrdersInfos(AccountInfo accountInfo, List<string> ordersIds, out OrderInfo[] ordersInfos, out string operationResultMessage)
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// Submit an order.
        /// </summary>
		public string SubmitOrder(AccountInfo account, Symbol symbol, OrderTypeEnum orderType, int volume, 
            decimal? allowedSlippage, decimal? desiredPrice, decimal? takeProfit, decimal? stopLoss, 
            string comment, out string operationResultMessage)
		{
			operationResultMessage = string.Empty;
			string operationResultMessageCopy = string.Empty;
			object orderId, psd;
            bool isBuy = OrderInfo.TypeIsBuy(orderType);

			GeneralHelper.GenericReturnDelegate<string> operationDelegate = delegate()
			{
				_manager.Desk.OpenTrade(account.Id, symbol.Name, isBuy, 
                    _adapter.DefaultLotSize, (double)desiredPrice.Value, 
                    (string)_adapter.GetInstrumentData(symbol.Name, "QuoteID"), 
                    0, 
                    stopLoss.HasValue ? (double)stopLoss.Value : 0, 
                    takeProfit.HasValue ? (double)takeProfit.Value : 0, 
                    0, out orderId, out psd);
                
                return orderId.ToString();
			};

			object result;
			if (_messageLoopOperator.Invoke(operationDelegate, TimeSpan.FromSeconds(8), out result) == false)
			{// Timed out.
				operationResultMessage = "Timeout submiting order.";
				return null;
			}

			if (string.IsNullOrEmpty((string)result))
			{// Operation error.
				operationResultMessage = operationResultMessageCopy;
				return null;
			}

			// Return the ID of the submitted order.
			return (string)result;
		}

		public bool ExecuteMarketOrder(AccountInfo accountInfo, Symbol symbol, OrderTypeEnum orderType, int volume, decimal? allowedSlippage, decimal? desiredPrice, decimal? takeProfit, decimal? stopLoss, string comment, out OrderInfo? orderPlaced, out string operationResultMessage)
		{
			operationResultMessage = string.Empty;
			string operationResultMessageCopy = string.Empty;
			object orderId, psd;

            bool isBuy = OrderInfo.TypeIsBuy(orderType);

			OrderInfo? order = null;
			GeneralHelper.GenericReturnDelegate<bool> operationDelegate = delegate()
			{
				_manager.Desk.OpenTrade(accountInfo.Id, symbol.Name, isBuy, _adapter.DefaultLotSize, (double)desiredPrice.Value, (string)_adapter.GetInstrumentData(symbol.Name, "QuoteID"), 0, (double)stopLoss.Value, (double)takeProfit.Value, 0, out orderId, out psd); 

				order = new OrderInfo();
				OrderInfo tempOrder = order.Value;
				tempOrder.Id = orderId.ToString();
				
				TableAut accountsTable = (FXCore.TableAut)_manager.Desk.FindMainTable("trades");

				RowAut item = (RowAut)accountsTable.FindRow("OrderID", orderId, 0);

				return true;
			};

			orderPlaced = order;

			object result;
			if (_messageLoopOperator.Invoke(operationDelegate, TimeSpan.FromSeconds(8), out result) == false)
			{// Timed out.
				operationResultMessage = "Timeout submiting order.";
				return false;
			}

			if (string.IsNullOrEmpty((string)result))
			{// Operation error.
				operationResultMessage = operationResultMessageCopy;
				return false;
			}

			return true;
		}

		public bool ModifyOrder(AccountInfo accountInfo, string orderId, decimal? stopLoss, decimal? takeProfit, decimal? targetOpenPrice, out string modifiedId, out string operationResultMessage)
		{
			throw new NotImplementedException();
		}

		public bool DecreaseOrderVolume(AccountInfo accountInfo, string orderId, decimal volumeDecreasal, decimal? allowedSlippage, decimal? desiredPrice, out decimal decreasalPrice, out string modifiedId, out string operationResultMessage)
		{
			throw new NotImplementedException();
		}

		public bool IncreaseOrderVolume(AccountInfo accountInfo, string orderId, decimal volumeIncrease, decimal? allowedSlippage, decimal? desiredPrice, out decimal increasalPrice, out string modifiedId, out string operationResultMessage)
		{
			throw new NotImplementedException();
		}

		public bool CloseOrCancelOrder(AccountInfo accountInfo, string orderId, string orderTag, 
            decimal? allowedSlippage, decimal? desiredPrice, out decimal closingPrice, 
            out DateTime closingTime, out string modifiedId, out string operationResultMessage)
		{
			throw new NotImplementedException();
		}

		public bool IsPermittedSymbol(AccountInfo accountInfo, Symbol symbol)
		{
            return FXCMAdapter.ForexSymbols.Contains(symbol.Name);
		}

		public int IsDataSourceSymbolCompatible(ComponentId dataSourceId, Symbol symbol)
		{
            if (FXCMAdapter.ForexSymbols.Contains(symbol.Name))
            {
                return int.MaxValue;
            }
            else
            {
                return 0;
            }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			_adapter = null;
			_manager = null;
		}

		#endregion
	}
}
