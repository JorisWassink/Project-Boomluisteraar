using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataTestDisplay : MonoBehaviour
{
    [SerializeField] private DataGetter dataGetter;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    
    private List<DataPoint> _dataList;
    private int _currentDataPointIndex;

    private void Start()
    {
        _dataList = dataGetter.GetData("9B261009", 2000);
        scrollbar.value = 1; //default to most recent data point
        scrollbar.numberOfSteps = _dataList.Count;
        scrollbar.onValueChanged.AddListener(OnScrollValueChanged);
        leftButton.onClick.AddListener(() => SetDataPoint(_currentDataPointIndex - 1));
        rightButton.onClick.AddListener(() => SetDataPoint(_currentDataPointIndex + 1));
        OnScrollValueChanged(1);
    }

    private void OnScrollValueChanged(float value)
    {
        int maxIndex = _dataList.Count - 1;
        int index = Mathf.FloorToInt(maxIndex * value);
        SetDataPoint(index);
    }

    private void SetDataPoint(int index)
    {
        _currentDataPointIndex = index;
        DataPoint currentDataPoint = _dataList[index];
        SetText(currentDataPoint);
    }
    
    private void SetText(DataPoint dataPoint)
    {
        string text = $"Data recorded at: {dataPoint.date}\n" + //1 + 2
                      $"Tree ID: {dataPoint.V2}\n" +
                      $"timestamp: {dataPoint.V4}\n"; 
        
        string dataString = "";
        switch (dataPoint.V3)
        {
            case 4 or 5:
                int type = dataPoint.V3 % 2 * dataPoint.V3; //todo: simplify/explain

                dataString = $"Data Type: Sapflow\n" +
                             $"Scan {1 + type}:\n" +
                             $"\tTimestamp: {dataPoint.V5}\n" +
                             $"\tDownstream probe: {dataPoint.V6}\n" +
                             $"\tHeater: {dataPoint.V7}\n" +
                             $"\tUpstream Probe: {dataPoint.V8}\n\n" +
                             $"Scan {2 + type}:\n" +
                             $"\tTimestamp: {dataPoint.V9}\n" +
                             $"\tDownstream probe: {dataPoint.V10}\n" +
                             $"\tHeater: {dataPoint.V11}\n" +
                             $"\tUpstream Probe: {dataPoint.V12}\n\n" +
                             $"Scan {3 + type}:\n" +
                             $"\tTimestamp: {dataPoint.V13}\n" +
                             $"\tDownstream probe: {dataPoint.V14}\n" +
                             $"\tHeater: {dataPoint.V15}\n" +
                             $"\tUpstream Probe: {dataPoint.V16}\n\n" +
                             $"Scan {4 + type}:\n" +
                             $"\tTimestamp: {dataPoint.V17}\n" +
                             $"\tDownstream probe: {dataPoint.V18}\n" +
                             $"\tHeater: {dataPoint.V19}\n" +
                             $"\tUpstream Probe: {dataPoint.V20}\n\n" +
                             $"Scan {5 + type}:\n" +
                             $"\tTimestamp: {dataPoint.V21}\n" +
                             $"\tDownstream probe: {dataPoint.V22}\n" +
                             $"\tHeater: {dataPoint.V23}\n" +
                             $"\tUpstream Probe: {dataPoint.V24}\n\n";
                break;
            case 6:
                dataString = $"Data Type: Spectrometer\n" +
                             $"Gain: {dataPoint.V5}\n" +
                             $"\t410: {dataPoint.V6}\n" +
                             $"\t435: {dataPoint.V7}\n" +
                             $"\t460: {dataPoint.V8}\n" +
                             $"\t485: {dataPoint.V9}\n" +
                             $"\t510: {dataPoint.V10}\n" +
                             $"\t535: {dataPoint.V11}\n" +
                             $"\t560: {dataPoint.V12}\n" +
                             $"\t585: {dataPoint.V13}\n" +
                             $"\t645: {dataPoint.V14}\n" +
                             $"\t705: {dataPoint.V15}\n" +
                             $"\t900: {dataPoint.V16}\n" +
                             $"\t940: {dataPoint.V17}\n" +
                             $"\t610: {dataPoint.V18}\n" +
                             $"\t680: {dataPoint.V19}\n" +
                             $"\t730: {dataPoint.V20}\n" +
                             $"\t760: {dataPoint.V21}\n" +
                             $"\t810: {dataPoint.V22}\n" +
                             $"\t860: {dataPoint.V23}\n";
                break;
            case 7:
                dataString = $"Data Type: Spectrometer (2)\n" +
                             $"Gain: {dataPoint.V5}\n" +
                             $"\t415: {dataPoint.V6}\n" +
                             $"\t445: {dataPoint.V7}\n" +
                             $"\t480: {dataPoint.V8}\n" +
                             $"\t515: {dataPoint.V9}\n" +
                             $"\t555: {dataPoint.V10}\n" +
                             $"\t590: {dataPoint.V11}\n" +
                             $"\t630: {dataPoint.V12}\n" +
                             $"\t680: {dataPoint.V13}\n" +
                             $"CLEAR No Calib: {dataPoint.V14}\n" +
                             $"NIT No Calib: {dataPoint.V15}\n";
                break;
            
            case 8:
                dataString = $"Data Type: rest\n" +
                             $"Time Stamp: {dataPoint.V4}\n" +
                             $"Position: {dataPoint.V5}, {dataPoint.V7}, {dataPoint.V9}\n" +
                             $"Position std: {dataPoint.V6}, {dataPoint.V8}, {dataPoint.V10}\n" +
                             $"Battery level: {dataPoint.V11} millivolts\n" +
                             $"Heater probe power input: {dataPoint.V12}\n" +
                             $"Air Temp: {dataPoint.V13 / 100} C\n" +
                             $"Air Humidity: {dataPoint.V14 / 100}% RH\n" +
                             $"Growth: {dataPoint.V15} d.n.\n";
                break;
            default:
                dataString = "<color=red>" + "Data not found" + "</color>";
                break;
        }
        text += dataString;
        
        label.text = text;
    }
}
