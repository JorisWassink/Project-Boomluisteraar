using System.Collections.Generic;
using UnityEngine;

public static class StaticVariables
{
    static TreeDataBase _treeDataBase;
    static TreeDataBase DB
    {
        get
        {
            if (_treeDataBase == null)
                _treeDataBase = Resources.Load<TreeDataBase>("TreeDataBase");
            return _treeDataBase;
        }
    }
    
    public static Dictionary<string, GameObject> treeModelDictionary = new Dictionary<string, GameObject>();

    public static TreeDataBase.TreeEntry GetTreeEntry(string id) => DB.GetTreeEntry(id);
    public static TreeDataBase.TreeEntry GetTreeEntryByName(string name) => DB.GetTreeEntryByName(name);
    public static GameObject GetTreeModel(string treeId) => DB.GetTreeModel(treeId);
    public static string GetTreeName(string treeId) => DB.GetTreeName(treeId);
    public static DataType GetTreeType(string treeId) => DB.GetTreeType(treeId);
    public static List<string> GetTreeIds => DB.GetTreeIds();
    
    public static string SelectedTreeId{get; set;}
    
    public enum DataType
    {
        None,
        CyberSensor,
        CarbonSensor,
        SoilSensor
    }
}

[System.Serializable]
public class TreeModel
{
    public string id;
    public GameObject prefab;
}