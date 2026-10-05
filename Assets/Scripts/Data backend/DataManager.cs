using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    
    private DataGetter _dataGetter;
    private List<List<DataPoint>> _dataCache = new List<List<DataPoint>>();
    
    private Dictionary<string, List<DataPoint>> _dataDict = new Dictionary<string, List<DataPoint>>()
    {
        {"", new List<DataPoint>()}
    };
    
    private void Awake()
    {
        //Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        _dataGetter = gameObject.AddComponent<DataGetter>();
    }
    

    /// <summary>
    ///   <para>Gets a specific data point of a specific tree.</para>
    /// </summary>
    /// <param name="treeId">The ID of the specific tree you want to get data from.</param>
    /// <param name="maxPoints">Amount of data points to return, starting at the end.</param>
    public DataPoint GetDataPoint(string treeId, int index)
    {
        DataPoint dataPoint = TryGetDataPoint(treeId, index);
        return dataPoint;
    }

    /// <summary>
    ///   <para>Gets a list of data points of a specific tree.</para>
    /// </summary>
    /// <param name="treeId">The ID of the specific tree you want to get data from.</param>
    /// <param name="maxPoints">Amount of data points to return, starting at the end.</param>
    public List<DataPoint> GetDataPoints(string treeId, int maxPoints)
    {
        List<DataPoint> dataPoints = TryGetDataPoints(treeId, maxPoints);
        return dataPoints;
    }

    private DataPoint TryGetDataPoint(string treeId, int index)
    {
        if (_dataDict.ContainsKey(treeId))
        {
            List<DataPoint> dataPointList = _dataDict[treeId];
            if (index < dataPointList.Count)
            {
                return dataPointList[index];
            }
        }
        List<DataPoint> dataPoints = _dataGetter.GetData(treeId, index);
        TrySaveDataPoints(treeId, dataPoints);
        return dataPoints[index];
    }

    private List<DataPoint> TryGetDataPoints(string treeId, int index)
    {
        if (_dataDict.ContainsKey(treeId))
        {
            List<DataPoint> dataPointList = _dataDict[treeId];
            if (index > dataPointList.Count)
            {
                for (int i = dataPointList.Count; i < index; i++)
                {
                    dataPointList[i] = dataPointList[i];
                }
                return dataPointList;
            }
        }
        List<DataPoint> dataPoints = _dataGetter.GetData(treeId, index);
        TrySaveDataPoints(treeId, dataPoints);
        return dataPoints;
    }

    private void TrySaveDataPoints(string treeID, List<DataPoint> dataPoints)
    {
        //for now i assume the data always starts at the end
        if (_dataDict.ContainsKey(treeID))
        {
            List<DataPoint> dataPointList = _dataDict[treeID];
            if (dataPoints.Count > dataPointList.Count)
            {
                int maxIndex = dataPointList.Count;
                for (int index = maxIndex; index < dataPoints.Count; index++)
                {
                    dataPointList[index] = dataPoints[index];
                }
            }
            else
            {
                Debug.Log("data exists already");
                return;
            }
        }
        else
        {
            TryAddDataPointsToDictionary(treeID, dataPoints);
        }
    }

    private void TryAddDataPointsToDictionary(string treeId, List<DataPoint> dataPoints)
    {
        bool isDataAvailable = _dataGetter.IsDataAvailable(treeId);
        if (!isDataAvailable)
            Debug.LogError($"{treeId} is empty");

        if (_dataDict.ContainsKey(treeId))
        {
            _dataDict[treeId].AddRange(dataPoints);
            return;
        }
        
        _dataDict.Add(treeId, dataPoints);
    }
}
