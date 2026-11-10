using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataTestDisplay : MonoBehaviour
{
    private List<string> AllSensorIds = new List<string>()
    {
        "9B261005",
        "91261120",
        "9B261002",
        "91261115",
        "91261114",
        "91261121",
        "9B261009",
        "93261020",
        "93261019"
    };
    
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button leftTitleButton;
    [SerializeField] private Button rightTitleButton;
    
    private List<DataPoint> _dataList;
    private int _currentDataPointIndex;
    private int _currentSensorIndex = 0;

    private void Start()
    {
        ChangeSensor(0);
        scrollbar.value = 1; //default to most recent data point
        scrollbar.numberOfSteps = _dataList.Count;
        scrollbar.onValueChanged.AddListener(OnScrollValueChanged);
        
        leftButton.onClick.AddListener(() => SetDataPoint(_currentDataPointIndex - 1));
        rightButton.onClick.AddListener(() => SetDataPoint(_currentDataPointIndex + 1));
        
        leftTitleButton.onClick.AddListener(() => ChangeSensor(_currentSensorIndex - 1));
        rightTitleButton.onClick.AddListener(() => ChangeSensor(_currentSensorIndex + 1));
        
        OnScrollValueChanged(1);
    }

    private void OnScrollValueChanged(float value)
    {
        int maxIndex = _dataList.Count - 1;
        int index = Mathf.FloorToInt(maxIndex * value);
        SetDataPoint(index);
    }

    private void ChangeSensor(int index)
    {
        if(index < 0)
            index = AllSensorIds.Count - 1;
        if(index >= AllSensorIds.Count)
            index = 0;
        
        _currentSensorIndex = index;
        string selectedSensorId = AllSensorIds[index];
        titleText.text = StaticVariables.GetTreeName(selectedSensorId);
        StaticVariables.DataType dataType = StaticVariables.GetTreeType(selectedSensorId);
        _dataList = DataManager.Instance.GetDataPoints(dataType, selectedSensorId, 1);
        OnScrollValueChanged(1);
    }

    private void SetDataPoint(int index)
    {
        if(index < 0)
            index = _dataList.Count - 1;
        if(index >= _dataList.Count)
            index = 0;
        
        _currentDataPointIndex = index;
        DataPoint currentDataPoint = _dataList[index];
        SetText(currentDataPoint);
    }
    
    private void SetText(DataPoint dataPoint)
    {
        string text = $"Data recorded at: {dataPoint.time}\n" + //1 + 2
                      $"Tree ID: {dataPoint.sensorId}\n" +
                      $"String type: {dataPoint.stringType}\n" +
                      $"timestamp: {dataPoint.timeStamp}\n"; 
        
        string dataString = "";
        switch (dataPoint.stringType)
        {
            case 3:
                SoilDataPoint soilDataPoint = (SoilDataPoint)dataPoint;
                dataString = $"Data Type: Soil\n {soilDataPoint.ToString()}";
                break;
            case 4 or 5:
                SapFlowDataPoint sapFlowDataPoint = (SapFlowDataPoint)dataPoint;
                List<SapReading> s = sapFlowDataPoint.SapFlowScans;

                dataString = $"Data Type: Sapflow\n";
                foreach (var reading in s)
                {
                    dataString += $"Scan {reading.scanNumber}:\n" +
                                  $"\tTimestamp: {reading.timeIndex}\n" +
                                  $"\tDownstream probe: {reading.downProbe}\n" +
                                  $"\tHeater: {reading.heatProbe}\n" +
                                  $"\tUpstream Probe: {reading.upProbe}\n\n";
                }
                break;
            case 6:
                SpectrometerDataPoint spectrometerDataPoint = (SpectrometerDataPoint)dataPoint;
                dataString = $"Data Type: Spectrometer\n {spectrometerDataPoint.ToString()}";
                break;
            case 7:
                Spectrometer2DataPoint spectrometer2DataPoint = (Spectrometer2DataPoint)dataPoint;
                dataString = $"Data Type: Spectrometer (2)\n {spectrometer2DataPoint.ToString()}";
                break;
            
            case 8:
                GeneralDataPoint generalDataPoint = (GeneralDataPoint)dataPoint;
                dataString = $"Data Type: rest\n {generalDataPoint.ToString()}";
                break;
            default:
                dataString = "<color=red>" + "Data not found" + "</color>";
                break;
        }
        text += dataString;
        
        label.text = text;
    }
}
