//using System;
//using System.Collections.Generic;
//using System.Text;
//using Arbiter;
//using CommonFinancial;
//using System.IO;
//using CommonSupport;
//using System.Configuration;
//using System.Runtime.Serialization;

//namespace ForexPlatform
//{
//    /// <summary>
//    /// Provides a source of bar dataDelivery from locally stored files. Data can serve as testing/analysis etc.
//    /// DEPRECATED, USE DATA STORE COMPONENT
//    /// </summary>
//    [Serializable]
//    [UserFriendlyName("Historical File Data Source")]
//    public class FileDataSource : DataSource
//    {
//        volatile string _dataFolderPath;
//        public string DataFolderPath
//        {
//            get { return _dataFolderPath; }
//            set { _dataFolderPath = value; }
//        }

//        //volatile string _sessionGroup;

//        /// <summary>
//        /// 
//        /// </summary>
//        public FileDataSource()
//            : base(null, false)
//        {
//        }

//        ///// <summary>
//        ///// Deserialization constructor.
//        ///// </summary>
//        //public FileDataSource(SerializationInfo orderInfo, StreamingContext context)
//        //    : base(orderInfo, context)
//        //{
//        //    _dataFolderPath = orderInfo.GetString("folderPath");
//        //}

//        ///// <summary>
//        ///// Custom serialization procedure.
//        ///// </summary>
//        //public override void GetObjectData(SerializationInfo orderInfo, StreamingContext context)
//        //{
//        //    base.GetObjectData(orderInfo, context);
//        //    orderInfo.AddValue("folderPath", _dataFolderPath);
//        //}

//        ///// <summary>
//        ///// 
//        ///// </summary>
//        //protected override bool OnSetInitialState(PlatformSettings dataDelivery)
//        //{
//        //    if (base.OnSetInitialState(dataDelivery) == false)
//        //    {
//        //        return false;
//        //    }

//        //    _dataFolderPath = dataDelivery.GetMappedFolder("QuoteDataFolder");
//        //    _sessionGroup = "Forex";
//        //    return true;
//        //}

//        ///// <summary>
//        ///// 
//        ///// </summary>
//        //protected override bool OnInitialize(Platform platform)
//        //{
//        //    if (base.OnInitialize(platform) == false)
//        //    {
//        //        return false;
//        //    }

//        //    UpdateAvailableSourceSessions();

//        //    ChangeOperationalState(OperationalStateEnum.Operational);
//        //    return true;
//        //}

//        ///// <summary>
//        ///// 
//        ///// </summary>
//        //void UpdateAvailableSourceSessions()
//        //{
//        //    _sessionGroup = "Forex";

//        //    string path = _dataFolderPath;
//        //    if (Path.IsPathRooted(_dataFolderPath) == false)
//        //    {
//        //        path = System.Windows.Forms.Application.StartupPath + "\\" + _dataFolderPath;
//        //    }

//        //    if (Directory.Exists(path) == false)
//        //    {
//        //        SystemMonitor.OperationWarning("Failed to find Quotes Data Path [" + path + "]");
//        //        return;
//        //    }

//        //    List<string> files = new List<string>();
//        //    files.AddRange(Directory.GetFiles(_dataFolderPath, "*.csv", SearchOption.TopDirectoryOnly));
//        //    files.AddRange(Directory.GetFiles(_dataFolderPath, "*.hst", SearchOption.TopDirectoryOnly));

//        //    for (int i = 0; i < files.Count; i++)
//        //    {
//        //        files[i] = Path.GetFileName(files[i]);
//        //    }

//        //    List<string> groups = new List<string>(Groups);

//        //    if (groups.Contains(_sessionGroup))
//        //    {
//        //        foreach (Info orderInfo in GetGroupSessions(_sessionGroup))
//        //        {// Clear existing sessions from container, leave only new ones.
//        //            files.Remove(orderInfo.Name);
//        //        }
//        //    }

//        //    List<Info> infos = new List<Info>();
//        //    foreach (string file in files)
//        //    {// For each new file name create a corresponding sessionInformation.
//        //        List<DataBar> datas;

//        //        FileDataBarReaderWriter reader = FinancialHelper.CreateFileReaderWriter(_dataFolderPath + "\\" + file);
//        //        if (reader.ReadData(0, 20, out datas))
//        //        {
//        //            BaseCurrency baseCurrency = new BaseCurrency("Forex", "Unknown");
//        //            Info orderInfo = new Info(Guid.NewGuid(), file, baseCurrency, reader.Period, 100000, FinancialHelper.EstablishDecimalDigits(datas));
//        //            infos.AddElement(orderInfo);
//        //        }
//        //    }

//        //    this.AddSessions(infos.ToArray());

//        //}

//        //[MessageReceiver]
//        //TradingValuesUpdateMessage Receive(RequestValuesMessage requestMessage)
//        //{// Someone requested values.

//        //    List<DataBar> bars;
//        //    lock (this)
//        //    {
//        //        FileDataBarReaderWriter reader = FinancialHelper.CreateFileReaderWriter(_dataFolderPath + "\\" + requestMessage.Info.Name);
//        //        if (reader.ReadData(0, 0, out bars))
//        //        {
//        //            return new TradingValuesUpdateMessage(requestMessage.Info, requestMessage.OperationId, decimal.MinValue, decimal.MinValue, bars.ToArray());
//        //        }
//        //        else
//        //        {
//        //            return new TradingValuesUpdateMessage(requestMessage.Info, requestMessage.OperationId, false);
//        //        }
//        //    }
//        //}

//        ///// <summary>
//        ///// 
//        ///// </summary>
//        //public override void UpdateSessions()
//        //{
//        //    UpdateAvailableSourceSessions();
//        //}

//        public override void UpdateSessions()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
