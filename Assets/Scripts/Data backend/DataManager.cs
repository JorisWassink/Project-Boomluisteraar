using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    
    private DataGetter _dataGetter;
    private Dictionary<string, List<DataPoint>> _dataDict = new Dictionary<string, List<DataPoint>>();
    

    
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
    ///   <para>Gets a specific data point of a specific sensor/api.</para>
    /// </summary>
    /// <param name="dataType">The type of api you want to call.</param>
    /// <param name="sensorId">The ID of the specific sensor you want to get data from.</param>
    /// <param name="index">Index of the specific datapoint you want.</param>
    public DataPoint GetDataPoint(StaticVariables.DataType dataType, string sensorId, int index)
    {
        switch (dataType)
        {
            case  StaticVariables.DataType.TreeSensor or StaticVariables.DataType.SoilSensor:
                return TryGetTreeDataPoint(sensorId, index);
            default:
                Debug.LogError($"{dataType} is empty");
                return new DataPoint();
        }
    }

    /// <summary>
    ///   <para>Gets a list of data points of a specific sensor/api.</para>
    /// </summary>
    /// <param name="dataType">The type of api you want to call.</param>
    /// <param name="sensorId">The ID of the specific sensor you want to get data from.</param>
    /// <param name="maxPoints">Amount of data points to return, starting at the end.</param>
    public List<DataPoint> GetDataPoints(StaticVariables.DataType dataType, string sensorId, int maxPoints)
    {
        switch (dataType)
        {
            case  StaticVariables.DataType.TreeSensor or  StaticVariables.DataType.SoilSensor:
                return TryGetTreeDataPoints(sensorId, maxPoints);
            default:
                Debug.LogError($"{dataType} is empty");
                return new List<DataPoint>();
        }
    }

    private DataPoint TryGetTreeDataPoint(string sensorId, int index)
    {
        if (_dataDict.ContainsKey(sensorId))
        {
            List<DataPoint> dataPointList = _dataDict[sensorId];
            if (index < dataPointList.Count)
            {
                return dataPointList[index];
            }
        }
        List<DataPoint> dataPoints = _dataGetter.GetData(sensorId, index);
        TrySaveDataPoints(sensorId, dataPoints);
        return dataPoints[index];
    }

    private List<DataPoint> TryGetTreeDataPoints(string sensorId, int index)
    {
        if (_dataDict.ContainsKey(sensorId))
        {
            List<DataPoint> dataPointList = _dataDict[sensorId];
            if (index > dataPointList.Count)
            {
                for (int i = dataPointList.Count; i < index; i++)
                {
                    dataPointList[i] = dataPointList[i];
                }
                return dataPointList;
            }
        }
        List<DataPoint> dataPoints = _dataGetter.GetData(sensorId, index);
        TrySaveDataPoints(sensorId, dataPoints);
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

    private void TryAddDataPointsToDictionary(string sensorId, List<DataPoint> dataPoints)
    {
        bool isDataAvailable = _dataGetter.IsDataAvailable(sensorId);
        if (!isDataAvailable)
            Debug.LogError($"{sensorId} is empty");

        if (_dataDict.ContainsKey(sensorId))
        {
            _dataDict[sensorId].AddRange(dataPoints);
            return;
        }
        
        _dataDict.Add(sensorId, dataPoints);
    }
}
