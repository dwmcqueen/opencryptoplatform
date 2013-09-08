using System;
using System.Collections.Generic;
using System.Text;
using CommonSupport;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace CommonFinancial
{
    /// <summary>
    /// Hosts the indicators calculation results. An indicator typically has one or a few output 
    /// signalling sets of ouput values (lines).
    /// </summary>
    [Serializable]
    public class IndicatorResults : ISerializable
    {
        Dictionary<string, List<double>> _resultSets = new Dictionary<string, List<double>>();
        Dictionary<string, LinesChartSeries.ChartTypeEnum> _resultSetsChartTypes = new Dictionary<string, LinesChartSeries.ChartTypeEnum>();

        /// <summary>
        /// Thread safe access to names of output sets.
        /// </summary>
        public List<string> SetsNamesList
        {
            get { lock (this) { return GeneralHelper.EnumerableToList<string>(_resultSets.Keys); } }
        }

        /// <summary>
        /// Thread unsafe way of accessing output sets names.
        /// </summary>
        public IEnumerable<string> SetsNamesUnsafe
        {
            get { lock (this) { return _resultSets.Keys; } }
        }

        public ReadOnlyCollection<double> this[int index]
        {
            get
            {
                lock (this)
                {
                    int i = 0;
                    foreach (string name in _resultSets.Keys)
                    {
                        if (i == index)
                        {
                            return _resultSets[name].AsReadOnly();
                        }
                    }
                }

                return null;
            }
        }
        
        public ReadOnlyCollection<double> this[string name]
        {
            get
            {
                lock (this)
                {
                    return _resultSets[name].AsReadOnly();
                }
            }
        }

        public int SetsCount
        {
            get
            {
                lock (this)
                {
                    return _resultSets.Count;
                }
            }
        }

        public int SetLength
        {
            get
            {
                lock (this)
                {
                    foreach (List<double> list in _resultSets.Values)
                    {
                        return list.Count;
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public IndicatorResults(Indicator indicator, string[] resultSetNames)
        {
            foreach (string name in resultSetNames)
            {
                _resultSets[name] = new List<double>();
            }
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public IndicatorResults(SerializationInfo info, StreamingContext context)
        {
            string[] names = (string[])info.GetValue("resultSetsNames", typeof(string[]));
            foreach (string name in names)
            {
                _resultSets.Add(name, new List<double>());
            }

            _resultSetsChartTypes = (Dictionary<string, LinesChartSeries.ChartTypeEnum>)info.GetValue("resultSetsChartTypes", typeof(Dictionary<string, LinesChartSeries.ChartTypeEnum>));
        }

        /// <summary>
        /// Serialization routine.
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("resultSetsNames", GeneralHelper.EnumerableToArray<string>(_resultSets.Keys));
            info.AddValue("resultSetsChartTypes", _resultSetsChartTypes);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                foreach (string resultSet in _resultSets.Keys)
                {
                    _resultSets[resultSet].Clear();
                }
            }
        }

        /// <summary>
        /// Clip all results to maximum count.
        /// </summary>
        /// <param name="count"></param>
        public void ClipTo(int count)
        {
            lock (this)
            {
                foreach (string resultSet in _resultSets.Keys)
                {
                    if (_resultSets[resultSet].Count > count)
                    {
                        _resultSets[resultSet].RemoveRange(count, _resultSets[resultSet].Count - count);
                    }

                    _resultSets[resultSet].Clear();
                }
            }
        }

        ///// <summary>
        ///// Append the result piece to the named result set.
        ///// </summary>
        //public bool AppendSetValues(string name, double[] inputResultPiece)
        //{
        //    return AddSetValues(name, this[name].Count, inputResultPiece.Length, true, inputResultPiece);
        //}

        /// <summary>
        /// 
        /// </summary>
        //public bool UpdateSetLatestValues(string name, double[] inputResultPiece)
        //{
        //    return AddSetValues(name, Math.Max(0, this.SetLength - inputResultPiece.Length), inputResultPiece.Length, true, inputResultPiece);
        //}

        ///<summary>
        /// This used to handle results.
        /// inputResultPiece stores results, where 0 corresponds to startingIndex; the length of inputResultPiece may be larger than count.
        ///</summary>
        public bool AddSetValues(string name, int startingIndex, int count, bool overrideExistingValues, double[] inputResultPiece)
        {
            lock (this)
            {
                if (_resultSets.ContainsKey(name) == false)
                {
                    SystemMonitor.Error("SetResultSetValues result set [" + name + "] not found.");
                    return false;
                }

                List<double> resultSet = _resultSets[name];

                for (int i = resultSet.Count; i < startingIndex; i++)
                {// Only if there are some empty spaces before the start, fill them with no value.
                    resultSet.Add(double.NaN);
                }

                // Get the dataDelivery from the result it is provided to us.
                for (int i = startingIndex; i < startingIndex + count; i++)
                {
                    if (resultSet.Count <= i)
                    {
                        resultSet.Add(inputResultPiece[i - startingIndex]);
                    }
                    else
                    {
                        if (overrideExistingValues)
                        {
                            resultSet[i] = inputResultPiece[i - startingIndex];
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public double? GetValueSetCurrentValue(int setIndex)
        {
            ReadOnlyCollection<double> set = this[setIndex];
            if (set == null || set.Count == 0)
            {
                return null;
            }
            return set[set.Count - 1];
        }

        /// <summary>
        /// 
        /// </summary>
        public double? GetValueSetCurrentValue(string setName)
        {
            ReadOnlyCollection<double> set = this[setName];
            if (set == null)
            {
                return null;
            }
            return set[set.Count - 1];
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetResultSetChartType(string setName, LinesChartSeries.ChartTypeEnum chartType)
        {
            _resultSetsChartTypes[setName] = chartType;
        }

        /// <summary>
        /// Obtain the chart type for this result set.
        /// </summary>
        public LinesChartSeries.ChartTypeEnum? GetResultSetChartType(string setName)
        {
            if (_resultSetsChartTypes.ContainsKey(setName))
            {
                return _resultSetsChartTypes[setName];
            }

            return null;
        }

    }
}



