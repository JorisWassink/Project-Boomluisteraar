using System.Collections.Generic;
using UnityEngine;

public static class StaticVariables
{
    public static Dictionary<string, (string, DataType)> TreeIdDictionary = new Dictionary<string, (string, DataType)>()
    {
        {"9B261005", ("Sequoiadendron giganteum",DataType.CarbonSensor)},
        {"91261120", ("Quercus robur",DataType.CyberSensor)},
        {"9B261002", ("Fagus sylvatica",DataType.CarbonSensor)},
        {"91261115", ("Broussonetia papyrifera",DataType.CyberSensor)},
        {"91261114", ("Fagus sylvatica 'Atropurpunicea'",DataType.CyberSensor)},
        {"9B261009", ("Carpinus betulus",DataType.CarbonSensor)},
        {"91261121", ("tamme kastanje",DataType.CyberSensor)},
        
        {"93261020", ("tamme kastanje (soil)", DataType.SoilSensor)},
        {"93261019", ("Carpinus betulus (soil)", DataType.SoilSensor)}
    };
    
    public static string SelectedTreeId{get; set;}
    
    public enum DataType
    {
        None,
        CyberSensor,
        CarbonSensor,
        SoilSensor
    }

    public static (string, DataType) TryGetTreeData(string id)
    {
        if (TreeIdDictionary.ContainsKey(id))
            return TreeIdDictionary[id];
        
        Debug.LogError($"There is no such id '{id}'");
        return ("ID NOT DEFINED", DataType.None);
    }
}
