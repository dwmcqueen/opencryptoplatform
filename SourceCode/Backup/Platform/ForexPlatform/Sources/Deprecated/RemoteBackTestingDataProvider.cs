//using System;
//using System.Collections.Generic;
//using System.Runtime.Serialization;
//using Arbiter;
//using CommonFinancial;
//using CommonSupport;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Provides access to remote source dataDelivery and extends this access to allow back testing to be done
//    /// on this dataDelivery. Back testing is performed trough the ITimeControl interface, that allows
//    /// time to be controlled; thus performing simulations.
//    /// </summary>
//    [Serializable]
//    public class RemoteBackTestingDataProvider : RemoteDataProvider, ITimeControl
//    {
//        #endregion

//        public override decimal Ask
//        {
//            get
//            {
//                lock (this)
//                {
//                    if (CurrentDataUnit == null)
//                    {
//                        return decimal.MinValue;
//                    }

//                    return CurrentDataUnit.Value.Close;
//                }
//            }
//        }

//        public override decimal Bid
//        {
//            get
//            {
//                lock (this)
//                {
//                    if (CurrentDataUnit == null)
//                    {
//                        return decimal.MinValue;
//                    }

//                    return CurrentDataUnit.Value.Close - Spread;
//                }
//            }
//        }

//        /// <summary>
//        /// Persisted.
//        /// </summary>
//        decimal _spread = 0;
//        public override decimal Spread
//        {
//            get
//            {
//                lock (this)
//                {
//                    return _spread;
//                }
//            }
//        }

//        public event CurrentStepChangedDelegate CurrentStepChangedEvent;

//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        public RemoteBackTestingDataProvider(Info account, List<ArbiterClientId?> forwardTransportation)
//            : base(account, forwardTransportation)
//        {
//        }

//        /// <summary>
//        /// Deserialization.
//        /// </summary>
//        public RemoteBackTestingDataProvider(SerializationInfo orderInfo, StreamingContext context)
//            : base(orderInfo, context)
//        {
//            _spread = orderInfo.GetDecimal("spread");
//            _initialAvailablePeriodsCount = orderInfo.GetInt32("initialAvailablePeriodsCount");
//        }

//        /// <summary>
//        /// Serialization.
//        /// </summary>
//        public override void GetObjectData(SerializationInfo orderInfo, StreamingContext context)
//        {
//            base.GetObjectData(orderInfo, context);

//            orderInfo.AddValue("spread", _spread);
//            orderInfo.AddValue("initialAvailablePeriodsCount", _initialAvailablePeriodsCount);
//        }

//        bool DoStepForward(int stepsRemaining)
//        {
//            lock (this)
//            {
//                if (_dataUnits.Count == _totalUnits.Count)
//                {
//                    return false;
//                }

//                _dataUnits.AddElement(_totalUnits[_dataUnits.Count]);
//            }

//            RaiseValuesUpdateEvent(DataBarUpdateType.PeriodOpen, 1, stepsRemaining);
//            if (CurrentStepChangedEvent != null)
//            {
//                CurrentStepChangedEvent(this);
//            }
//            return true;
//        }

//        public bool StepForward()
//        {
//            return DoStepForward(0);
//        }

//        public bool StepBack()
//        {
//            lock (this)
//            {
//                return false;
//            }
//        }

//        public bool StepTo(int index)
//        {
//            if (index < CurrentStep)
//            {
//                return false;
//            }

//            // This is before the start.
//            int current = CurrentStep;
//            for (int i = current; i < index; i++)
//            {
//                // Last step is not marked as multi step to indicate stepping is over.
//                if (DoStepForward(index - 1 - i) == false)
//                {
//                    return false;
//                }
//            }

//            RaiseValuesUpdateEvent(DataBarUpdateType.Quote, 1, 0);
//            if (CurrentStepChangedEvent != null)
//            {
//                CurrentStepChangedEvent(this);
//            }
//            return true;
//        }

//        [MessageReceiver]
//        protected override void Receive(TradingValuesUpdateMessage requestMessage)
//        {
//            DataBar[] inputDataUnits = requestMessage.GenerateDataUnits();
//            decimal ask = requestMessage.Ask;
//            decimal bid = requestMessage.Bid;

//            int updatedItemsCount = 1;
//            DataBarUpdateType updateType = DataBarUpdateType.Quote;
//            lock(this)
//            {
//                if (_totalUnits.Count == 0)
//                {
//                    updateType = DataBarUpdateType.Initial;
//                    updatedItemsCount = inputDataUnits.Length;
//                }

//                _lastUpdateDateTime = DateTime.Now;
//                if (inputDataUnits.Length > 0)
//                {// In this scenario spread is a fixed percentage of the initial bar close price.
//                    int decimalSymbolsCount = 5;
//                    if (inputDataUnits[0].Close < 1)
//                    {
//                        decimalSymbolsCount = 4;
//                    }
//                    decimal roundValue1 = decimal.Round(inputDataUnits[0].Close, decimalSymbolsCount);
//                    decimal roundValue2 = decimal.Round(inputDataUnits[0].Close * 1.0005m, decimalSymbolsCount);
//                    //decimal roundValue1 = MathHelper.RoundToSymbolsCount(inputDataUnits[0].Close, decimalSymbolsCount);
//                    //decimal roundValue2 = MathHelper.RoundToSymbolsCount(inputDataUnits[0].Close * 1.0005, decimalSymbolsCount);
//                    _spread = roundValue2 - roundValue1;
//                }
//                else
//                {
//                    _spread = 0;
//                }
//                //_ask = ask;
//                //_bid = bid;

//                for (int i = 0; i < inputDataUnits.Length; i++)
//                {
//                    if (_totalUnits.Count == 0 || inputDataUnits[i].DateTime > _totalUnits[_totalUnits.Count - 1].DateTime)
//                    {
//                        if (updateType == DataBarUpdateType.Quote)
//                        {
//                            updateType = DataBarUpdateType.PeriodOpen;
//                        }

//                        if (updatedItemsCount != inputDataUnits.Length)
//                        {
//                            updatedItemsCount++;
//                        }

//                        _totalUnits.AddElement(inputDataUnits[i]);
//                    }
//                }

//                // Also check the last 5 units for any requotes that might have been sent,
//                // this happens when price changes and we get updates for the last unit.
//                for (int i = 0; i < 5 && inputDataUnits.Length - 1 - i > 0 && _totalUnits.Count - 1 - i > 0; i++)
//                {
//                    if (inputDataUnits[inputDataUnits.Length - 1 - i].DateTime == _totalUnits[_totalUnits.Count - 1 - i].DateTime
//                        && inputDataUnits[inputDataUnits.Length - 1 - i].Equals(_totalUnits[_totalUnits.Count - 1 - i]) == false)
//                    {
//                        _totalUnits[_totalUnits.Count - 1 - i] = inputDataUnits[inputDataUnits.Length - 1 - i];
//                        if (updateType == DataBarUpdateType.Quote)
//                        {
//                            updateType = DataBarUpdateType.HistoryUpdate;
//                        }

//                        if (updatedItemsCount != inputDataUnits.Length)
//                        {
//                            updatedItemsCount++;
//                        }
//                    }
//                }
//            } // lock(this)

//            if (updateType == DataBarUpdateType.Initial)
//            {// Start initially with _initialAvailablePeriods periods count.
//                lock (this)
//                {
//                    for (int i = 0; i < _initialAvailablePeriodsCount && i < _totalUnits.Count; i++)
//                    {
//                        _dataUnits.AddElement(_totalUnits[i]);
//                    }
//                }

//                if (OperationalState == OperationalStateEnum.Initialized)
//                {
//                    ChangeOperationalState(OperationalStateEnum.Operational);
//                }

//                RaiseValuesUpdateEvent(DataBarUpdateType.Initial, _dataUnits.Count, 0);
//            }
            
//        }


//    }
//}
